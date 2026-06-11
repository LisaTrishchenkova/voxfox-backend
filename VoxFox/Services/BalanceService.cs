using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Balance;
using VoxFox.Models.DTOs;
using VoxFox.Models.DTOs.Money;
using VoxFox.Models.Entities;
using VoxFox.Models.Responses;

namespace VoxFox.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceRepository _balanceRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ApplicationContext _context; // только для DB-транзакций и Enrollments
        private readonly ILogger<BalanceService> _logger;
        private const decimal PlatformCommission = 0.15m;

        public BalanceService(
            IBalanceRepository balanceRepository,
            ICourseRepository courseRepository,
            ApplicationContext context,
            ILogger<BalanceService> logger)
        {
            _balanceRepository = balanceRepository;
            _courseRepository = courseRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<BalanceDto>> GetBalanceAsync(Guid userId)
        {
            var user = await _balanceRepository.GetUserByIdAsync(userId);
            if (user == null)
                return ServiceResult<BalanceDto>.Fail("Пользователь не найден", StatusCodes.Status404NotFound);

            return ServiceResult<BalanceDto>.Ok(new BalanceDto { UserId = userId, Balance = user.Balance });
        }

        public async Task<ServiceResult<BalanceDto>> TopUpAsync(Guid userId, decimal amount)
        {
            if (amount <= 0)
                return ServiceResult<BalanceDto>.Fail("Сумма пополнения должна быть больше нуля");

            var user = await _balanceRepository.GetUserByIdAsync(userId);
            if (user == null)
                return ServiceResult<BalanceDto>.Fail("Пользователь не найден", StatusCodes.Status404NotFound);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Balance += amount;
                await _balanceRepository.UpdateUserBalanceAsync(user);

                await _balanceRepository.AddTransactionAsync(new Transaction
                {
                    UserId = userId,
                    Type = TransactionType.TopUp,
                    Amount = amount,
                    CreatedAt = DateTime.UtcNow
                });

                await tx.CommitAsync();
                return ServiceResult<BalanceDto>.Ok(new BalanceDto { UserId = userId, Balance = user.Balance });
            }
            catch (System.Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Ошибка при пополнении баланса userId={UserId}", userId);
                return ServiceResult<BalanceDto>.Fail("Ошибка при пополнении баланса", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResult<TransactionDto>> PurchaseCourseAsync(Guid userId, Guid courseId)
        {
            var user = await _balanceRepository.GetUserByIdAsync(userId);
            if (user == null)
                return ServiceResult<TransactionDto>.Fail("Пользователь не найден", StatusCodes.Status404NotFound);

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return ServiceResult<TransactionDto>.Fail("Курс не найден", StatusCodes.Status404NotFound);

            if (course.Status != CourseStatus.Published || course.IsDeleted)
                return ServiceResult<TransactionDto>.Fail("Курс недоступен для покупки");

            if (course.Price <= 0)
                return ServiceResult<TransactionDto>.Fail("Этот курс бесплатный — запишитесь через enrollment");

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (alreadyEnrolled)
                return ServiceResult<TransactionDto>.Fail("Вы уже записаны на этот курс");

            if (user.Balance < course.Price)
                return ServiceResult<TransactionDto>.Fail("Недостаточно средств на балансе");

            var teacherAmount = Math.Round(course.Price * (1 - PlatformCommission), 2);
            var platformAmount = course.Price - teacherAmount;

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Balance -= course.Price;
                await _balanceRepository.UpdateUserBalanceAsync(user);

                var purchaseTx = new Transaction
                {
                    UserId = userId,
                    Type = TransactionType.Purchase,
                    Amount = -course.Price,
                    CourseId = courseId,
                    TotalAmount = course.Price,
                    TeacherAmount = teacherAmount,
                    PlatformAmount = platformAmount,
                    CreatedAt = DateTime.UtcNow
                };

                var transactions = new List<Transaction> { purchaseTx };

                if (course.AuthorId.HasValue)
                {
                    var teacher = await _balanceRepository.GetUserByIdAsync(course.AuthorId.Value);
                    if (teacher != null)
                    {
                        teacher.Balance += teacherAmount;
                        await _balanceRepository.UpdateUserBalanceAsync(teacher);

                        transactions.Add(new Transaction
                        {
                            UserId = course.AuthorId.Value,
                            Type = TransactionType.Earning,
                            Amount = teacherAmount,
                            CourseId = courseId,
                            TotalAmount = course.Price,
                            TeacherAmount = teacherAmount,
                            PlatformAmount = platformAmount,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _balanceRepository.AddTransactionsAsync(transactions);

                _context.Enrollments.Add(new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    Status = EnrollmentStatus.Active,
                    EnrolledAt = DateTime.UtcNow
                });
                course.EnrollmentCount += 1;
                await _courseRepository.UpdateAsync(course);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return ServiceResult<TransactionDto>.Ok(MapToDto(purchaseTx, user.Name, course.Title));
            }
            catch (System.Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Ошибка при покупке курса userId={UserId} courseId={CourseId}", userId, courseId);
                return ServiceResult<TransactionDto>.Fail("Ошибка при покупке курса", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResult<TransactionDto>> RefundPurchaseAsync(Guid originalTransactionId)
        {
            var original = await _balanceRepository.GetTransactionByIdAsync(originalTransactionId);
            if (original == null)
                return ServiceResult<TransactionDto>.Fail("Транзакция не найдена", StatusCodes.Status404NotFound);

            if (original.Type != TransactionType.Purchase)
                return ServiceResult<TransactionDto>.Fail("Отменить можно только транзакцию покупки");

            if (original.IsRefunded)
                return ServiceResult<TransactionDto>.Fail("Транзакция уже была отменена");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var refundTransactions = new List<Transaction>();

                var student = await _balanceRepository.GetUserByIdAsync(original.UserId);
                if (student != null)
                {
                    student.Balance += original.TotalAmount!.Value;
                    await _balanceRepository.UpdateUserBalanceAsync(student);
                }

                var refundTx = new Transaction
                {
                    UserId = original.UserId,
                    Type = TransactionType.Refund,
                    Amount = original.TotalAmount!.Value,
                    CourseId = original.CourseId,
                    TotalAmount = original.TotalAmount,
                    TeacherAmount = original.TeacherAmount,
                    PlatformAmount = original.PlatformAmount,
                    OriginalTransactionId = original.Id,
                    CreatedAt = DateTime.UtcNow
                };
                refundTransactions.Add(refundTx);

                if (original.CourseId.HasValue && original.Course?.AuthorId.HasValue == true)
                {
                    var teacher = await _balanceRepository.GetUserByIdAsync(original.Course.AuthorId.Value);
                    if (teacher != null)
                    {
                        teacher.Balance -= original.TeacherAmount!.Value;
                        await _balanceRepository.UpdateUserBalanceAsync(teacher);

                        refundTransactions.Add(new Transaction
                        {
                            UserId = teacher.Id,
                            Type = TransactionType.Refund,
                            Amount = -original.TeacherAmount!.Value,
                            CourseId = original.CourseId,
                            TotalAmount = original.TotalAmount,
                            TeacherAmount = original.TeacherAmount,
                            PlatformAmount = original.PlatformAmount,
                            OriginalTransactionId = original.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    var enrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e => e.UserId == original.UserId && e.CourseId == original.CourseId.Value);
                    if (enrollment != null)
                    {
                        _context.Enrollments.Remove(enrollment);
                        var course = await _courseRepository.GetByIdAsync(original.CourseId.Value);
                        if (course != null)
                        {
                            course.EnrollmentCount = Math.Max(0, course.EnrollmentCount - 1);
                            await _courseRepository.UpdateAsync(course);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                await _balanceRepository.AddTransactionsAsync(refundTransactions);

                original.IsRefunded = true;
                await _balanceRepository.UpdateTransactionAsync(original);

                await tx.CommitAsync();
                return ServiceResult<TransactionDto>.Ok(MapToDto(refundTx, student?.Name ?? "", original.Course?.Title));
            }
            catch (System.Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Ошибка при возврате транзакции {Id}", originalTransactionId);
                return ServiceResult<TransactionDto>.Fail("Ошибка при возврате", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResult<PaginatedResponse<TransactionDto>>> GetUserTransactionsAsync(Guid userId, int page, int pageSize)
        {
            var (items, total) = await _balanceRepository.GetUserTransactionsAsync(
                userId, (page - 1) * pageSize, pageSize);

            return ServiceResult<PaginatedResponse<TransactionDto>>.Ok(new PaginatedResponse<TransactionDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                PageSize = pageSize
            });
        }

        public async Task<ServiceResult<PaginatedResponse<TransactionDto>>> GetAllTransactionsAsync(int page, int pageSize)
        {
            var (items, total) = await _balanceRepository.GetAllTransactionsAsync(
                (page - 1) * pageSize, pageSize);

            return ServiceResult<PaginatedResponse<TransactionDto>>.Ok(new PaginatedResponse<TransactionDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                PageSize = pageSize
            });
        }

        public async Task<ServiceResult<PlatformStatsDto>> GetPlatformStatsAsync()
        {
            var totalRevenue = await _balanceRepository.GetTotalRevenueAsync();
            var totalRefunded = await _balanceRepository.GetTotalRefundedAsync();
            var totalPurchases = await _balanceRepository.GetPurchasesCountAsync();
            var totalRefunds = await _balanceRepository.GetRefundsCountAsync();

            return ServiceResult<PlatformStatsDto>.Ok(new PlatformStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalRefunded = totalRefunded,
                NetRevenue = totalRevenue - totalRefunded,
                TotalPurchases = totalPurchases,
                TotalRefunds = totalRefunds
            });
        }

        // Используется в Select — один параметр, данные берём из навигационных свойств
        private static TransactionDto MapToDto(Transaction t)
        {
            return new TransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserName = t.User?.Name ?? "",
                Type = t.Type,
                Amount = t.Amount,
                CourseId = t.CourseId,
                CourseTitle = t.Course?.Title,
                TotalAmount = t.TotalAmount,
                TeacherAmount = t.TeacherAmount,
                PlatformAmount = t.PlatformAmount,
                IsRefunded = t.IsRefunded,
                OriginalTransactionId = t.OriginalTransactionId,
                CreatedAt = t.CreatedAt
            };
        }

        // Используется когда навигационные свойства не загружены (сразу после создания)
        private static TransactionDto MapToDto(Transaction t, string userName, string? courseTitle)
        {
            return new TransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserName = userName,
                Type = t.Type,
                Amount = t.Amount,
                CourseId = t.CourseId,
                CourseTitle = courseTitle,
                TotalAmount = t.TotalAmount,
                TeacherAmount = t.TeacherAmount,
                PlatformAmount = t.PlatformAmount,
                IsRefunded = t.IsRefunded,
                OriginalTransactionId = t.OriginalTransactionId,
                CreatedAt = t.CreatedAt
            };
        }
    }
}

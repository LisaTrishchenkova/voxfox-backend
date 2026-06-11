using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Balance;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories
{
    public class BalanceRepository : IBalanceRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<BalanceRepository> _logger;

        public BalanceRepository(ApplicationContext context, ILogger<BalanceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserBalanceAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении баланса пользователя {UserId}", user.Id);
                throw;
            }
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении транзакции");
                throw;
            }
        }

        public async Task AddTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            try
            {
                _context.Transactions.AddRange(transactions);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении транзакций");
                throw;
            }
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Course)
                    .ThenInclude(c => c!.Author)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Update(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении транзакции {TransactionId}", transaction.Id);
                throw;
            }
        }

        public async Task<(List<Transaction> Items, int TotalCount)> GetUserTransactionsAsync(Guid userId, int skip, int take)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Course)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip(skip).Take(take).ToListAsync();
            return (items, total);
        }

        public async Task<(List<Transaction> Items, int TotalCount)> GetAllTransactionsAsync(int skip, int take)
        {
            var query = _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Course)
                .OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip(skip).Take(take).ToListAsync();
            return (items, total);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Transactions
                .Where(t => t.Type == TransactionType.Purchase && t.PlatformAmount != null)
                .SumAsync(t => t.PlatformAmount!.Value);
        }

        public async Task<decimal> GetTotalRefundedAsync()
        {
            return await _context.Transactions
                .Where(t => t.Type == TransactionType.Refund
                         && t.PlatformAmount != null
                         && t.Amount > 0) // Amount > 0 — это строка студента (возврат ему)
                .SumAsync(t => t.PlatformAmount!.Value);
        }

        public async Task<int> GetPurchasesCountAsync()
        {
            return await _context.Transactions
                .CountAsync(t => t.Type == TransactionType.Purchase);
        }

        public async Task<int> GetRefundsCountAsync()
        {
            return await _context.Transactions
                .CountAsync(t => t.Type == TransactionType.Refund && t.Amount > 0);
        }
    }
}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VoxFox.Enums;
using VoxFox.Interfaces.Achievement;
using VoxFox.Interfaces.Certificate;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.DTOs;
using VoxFox.Models.DTOs.Certificate;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly INotificationService _notificationService;
    private readonly IAchievementService _achievementService;

    public CertificateService(
        ICertificateRepository certificateRepository,
        INotificationService notificationService,
        IAchievementService achievementService)
    {
        _certificateRepository = certificateRepository;
        _notificationService = notificationService;
        _achievementService = achievementService;
    }

    public async Task<Certificate?> IssueCertificateAsync(
        Guid userId, Guid courseId, Guid enrollmentId, bool certificateEnabled)
    {
        if (!certificateEnabled)
            return null;

        var existing = await _certificateRepository.GetByEnrollmentIdAsync(enrollmentId);
        if (existing != null)
            return existing;

        var certificate = new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            EnrollmentId = enrollmentId,
            VerificationToken = Guid.NewGuid().ToString("N"),
            IssuedAt = DateTime.UtcNow
        };

        var created = await _certificateRepository.AddAsync(certificate);

        await _notificationService.SendAsync(
            userId,
            "Сертификат получен",
            "Поздравляем! Вы успешно завершили курс и получили сертификат.",
            NotificationType.CertificateIssued,
            relatedEntityId: created.Id,
            relatedCourseId: courseId);

        // ─── Ачивки за сертификат ─────────────────────────────────
        await _achievementService.CheckAndAwardAsync(userId, AchievementTrigger.CertificateIssued);

        return created;
    }

    public async Task<ServiceResult<IList<CertificateDto>>> GetMyCertificatesAsync(Guid userId)
    {
        var certs = await _certificateRepository.GetByUserIdAsync(userId);
        return ServiceResult<IList<CertificateDto>>.Ok(certs.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<CertificateDto>> GetByIdAsync(Guid id, Guid userId)
    {
        var cert = await _certificateRepository.GetByIdAsync(id);
        if (cert == null)
            return ServiceResult<CertificateDto>.Fail("Сертификат не найден", 404);

        if (cert.UserId != userId)
            return ServiceResult<CertificateDto>.Fail("Нет доступа", 403);

        return ServiceResult<CertificateDto>.Ok(MapToDto(cert));
    }

    public async Task<ServiceResult<CertificateDto>> VerifyAsync(string token)
    {
        var cert = await _certificateRepository.GetByTokenAsync(token);
        if (cert == null)
            return ServiceResult<CertificateDto>.Fail("Сертификат не найден или недействителен", 404);

        return ServiceResult<CertificateDto>.Ok(MapToDto(cert));
    }

    public async Task<ServiceResult<byte[]>> GeneratePdfAsync(Guid id, Guid userId)
    {
        var cert = await _certificateRepository.GetByIdAsync(id);
        if (cert == null)
            return ServiceResult<byte[]>.Fail("Сертификат не найден", 404);

        if (cert.UserId != userId)
            return ServiceResult<byte[]>.Fail("Нет доступа", 403);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.Background().Background("#f8fff8");

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(40).AlignCenter().Text("СЕРТИФИКАТ")
                        .FontSize(42).Bold().FontColor("#16a34a");

                    col.Item().PaddingTop(8).AlignCenter().Text("об успешном прохождении курса")
                        .FontSize(16).FontColor("#64748b");

                    col.Item().PaddingTop(50).AlignCenter()
                        .Text("Настоящим подтверждается, что")
                        .FontSize(14).FontColor("#374151");

                    col.Item().PaddingTop(16).AlignCenter()
                        .Text(cert.User?.Name ?? "Студент")
                        .FontSize(32).Bold().FontColor("#0f172a");

                    col.Item().PaddingTop(16).AlignCenter()
                        .Text("успешно завершил(а) курс")
                        .FontSize(14).FontColor("#374151");

                    col.Item().PaddingTop(16).AlignCenter()
                        .Text($"«{cert.Course?.Title ?? ""}»")
                        .FontSize(22).Bold().FontColor("#16a34a");

                    col.Item().PaddingTop(50).AlignCenter()
                        .Text($"Дата выдачи: {cert.IssuedAt:dd.MM.yyyy}")
                        .FontSize(13).FontColor("#64748b");

                    col.Item().PaddingTop(8).AlignCenter()
                        .Text($"Токен верификации: {cert.VerificationToken}")
                        .FontSize(10).FontColor("#94a3b8");
                });
            });
        }).GeneratePdf();

        return ServiceResult<byte[]>.Ok(pdf);
    }

    private static CertificateDto MapToDto(Certificate c) => new()
    {
        Id = c.Id,
        CourseId = c.CourseId,
        CourseTitle = c.Course?.Title ?? "",
        UserName = c.User?.Name ?? "",
        VerificationToken = c.VerificationToken,
        IssuedAt = c.IssuedAt
    };
}

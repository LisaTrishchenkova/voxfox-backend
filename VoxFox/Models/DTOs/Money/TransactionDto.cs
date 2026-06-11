using VoxFox.Enums;

namespace VoxFox.Models.DTOs.Money;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }

    public Guid? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TeacherAmount { get; set; }
    public decimal? PlatformAmount { get; set; }

    public bool IsRefunded { get; set; }
    public Guid? OriginalTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

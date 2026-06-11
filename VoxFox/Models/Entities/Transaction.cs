using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public TransactionType Type { get; set; }

    /// <summary>Сумма для данного пользователя (+ зачисление, - списание)</summary>
    public decimal Amount { get; set; }

    // Поля заполняются только для покупок (Type == Purchase / Refund)
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TeacherAmount { get; set; }
    public decimal? PlatformAmount { get; set; }

    public bool IsRefunded { get; set; } = false;

    /// <summary>Для Refund-транзакции — ссылка на исходную покупку</summary>
    public Guid? OriginalTransactionId { get; set; }
    public Transaction? OriginalTransaction { get; set; }

    public DateTime CreatedAt { get; set; }
}

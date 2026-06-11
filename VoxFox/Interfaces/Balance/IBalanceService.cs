using VoxFox.Models.DTOs.Money;
using VoxFox.Models.Responses;

namespace VoxFox.Interfaces.Balance;

public interface IBalanceService
{
    /// <summary>Пополнить баланс текущего пользователя</summary>
    Task<ServiceResult<BalanceDto>> TopUpAsync(Guid userId, decimal amount);

    /// <summary>Купить курс (списание + зачисление преподавателю)</summary>
    Task<ServiceResult<TransactionDto>> PurchaseCourseAsync(Guid userId, Guid courseId);

    /// <summary>Отменить покупку — возврат студенту, списание у преподавателя</summary>
    Task<ServiceResult<TransactionDto>> RefundPurchaseAsync(Guid originalTransactionId);

    /// <summary>История транзакций конкретного пользователя</summary>
    Task<ServiceResult<PaginatedResponse<TransactionDto>>> GetUserTransactionsAsync(Guid userId, int page, int pageSize);

    /// <summary>Все транзакции платформы (для админа)</summary>
    Task<ServiceResult<PaginatedResponse<TransactionDto>>> GetAllTransactionsAsync(int page, int pageSize);

    /// <summary>Сводка доходов платформы (для админа)</summary>
    Task<ServiceResult<PlatformStatsDto>> GetPlatformStatsAsync();

    /// <summary>Получить баланс пользователя</summary>
    Task<ServiceResult<BalanceDto>> GetBalanceAsync(Guid userId);
}

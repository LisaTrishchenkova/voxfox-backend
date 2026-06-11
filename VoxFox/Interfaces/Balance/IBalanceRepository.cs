using VoxFox.Models.Entities;

namespace VoxFox.Interfaces.Balance;

public interface IBalanceRepository
{
    Task<Models.Entities.User?> GetUserByIdAsync(Guid userId);
    System.Threading.Tasks.Task UpdateUserBalanceAsync(Models.Entities.User user);

    System.Threading.Tasks.Task AddTransactionAsync(Transaction transaction);
    System.Threading.Tasks.Task AddTransactionsAsync(IEnumerable<Transaction> transactions);

    Task<Transaction?> GetTransactionByIdAsync(Guid id);
    System.Threading.Tasks.Task UpdateTransactionAsync(Transaction transaction);

    Task<(List<Transaction> Items, int TotalCount)> GetUserTransactionsAsync(Guid userId, int skip, int take);
    Task<(List<Transaction> Items, int TotalCount)> GetAllTransactionsAsync(int skip, int take);

    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetTotalRefundedAsync();
    Task<int> GetPurchasesCountAsync();
    Task<int> GetRefundsCountAsync();
}

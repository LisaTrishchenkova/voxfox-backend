namespace VoxFox.Enums;

public enum TransactionType
{
    TopUp,    // Пополнение баланса
    Purchase, // Покупка курса (списание у студента)
    Earning,  // Поступление у преподавателя
    Refund    // Возврат (разворачивает Purchase + Earning)
}

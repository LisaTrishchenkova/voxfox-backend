namespace VoxFox.Models.DTOs.Money;

public class PlatformStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalRefunds { get; set; }
}

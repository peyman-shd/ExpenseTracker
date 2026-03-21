namespace ExpenseTracker.Models;

public class DashboardViewModel
{
    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetBalance { get; set; }
    
    public int TotalCards { get; set; }

    public int TotalTransactions { get; set; }
    
    public List<DashboardCardSummary> CardSummaries { get; set; } = new();
}
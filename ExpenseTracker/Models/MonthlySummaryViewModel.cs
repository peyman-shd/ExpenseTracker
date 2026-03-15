namespace ExpenseTracker.Models;

public class MonthlySummaryViewModel
{
    public int SelectedYear { get; set; }

    public int SelectedMonth { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetBalance { get; set; }
    
    public int? SelectedCardId { get; set; }
    
    public List<Transaction> Transactions { get; set; } = new();
    
    public List<Card> Cards { get; set; } = new();
}
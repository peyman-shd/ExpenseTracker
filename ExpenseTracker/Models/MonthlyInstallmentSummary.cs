namespace ExpenseTracker.Models;

public class MonthlyInstallmentSummary
{
    public int InstallmentPaymentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CardName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public int DueYear { get; set; }

    public int DueMonth { get; set; }
    
    public decimal RemainingAmount { get; set; }
}
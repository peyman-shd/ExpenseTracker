using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public class InstallmentPayment
{
    [Key]
    public int InstallmentPaymentId { get; set; }

    public int TransactionId { get; set; }
    public Transaction? Transaction { get; set; }

    public int DueYear { get; set; }

    public int DueMonth { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidDate { get; set; }
}
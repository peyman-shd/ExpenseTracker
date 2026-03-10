using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models;

public class Transaction
{
    public int TransactionId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string TransactionType { get; set; } = string.Empty;

    public string? Category { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Column(TypeName = "date")]
    public DateTime TransactionDate { get; set; }

    public int CardId { get; set; }

    public Card? Card { get; set; }
}
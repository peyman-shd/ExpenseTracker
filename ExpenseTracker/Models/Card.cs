using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models;

public class Card
{
    public int CardId { get; set; }

    [Required]
    public string CardName { get; set; }

    [Required]
    public decimal Limit { get; set; }

    [Required]
    public decimal CurrentBalance { get; set; }

    [Required]
    public string CardType { get; set; }

    public int UserId { get; set; }
}

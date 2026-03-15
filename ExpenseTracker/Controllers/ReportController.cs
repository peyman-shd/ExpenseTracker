using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

public class ReportController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public ReportController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> MonthlySummary(int? year, int? month, int? cardId)
    {
        int selectedYear = year ?? DateTime.Now.Year;
        int selectedMonth = month ?? DateTime.Now.Month;
        
        var cards = await _context.Cards.ToListAsync();

        var transactionsQuery = _context.Transactions
            .Include(t => t.Card)
            .Where(t => t.TransactionDate.Year == selectedYear &&
                        t.TransactionDate.Month == selectedMonth)
            .AsQueryable();

        if (cardId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.CardId == cardId.Value);
        }

        var transactions = await transactionsQuery
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        var totalIncome = transactions
            .Where(t => t.TransactionType == "Income")
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => t.TransactionType == "Expense")
            .Sum(t => t.Amount);

        var viewModel = new MonthlySummaryViewModel
        {
            SelectedYear = selectedYear,
            SelectedMonth = selectedMonth,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            Transactions = transactions,
            SelectedCardId = cardId,
            Cards = cards
        };

        return View(viewModel);
    }
}

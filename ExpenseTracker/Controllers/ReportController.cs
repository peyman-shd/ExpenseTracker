using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public ReportController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> MonthlySummary(int? year, int? month, int? cardId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        int selectedYear = year ?? DateTime.Now.Year;
        int selectedMonth = month ?? DateTime.Now.Month;

        var cards = await _context.Cards
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CardName)
            .ToListAsync();

        var userCardIds = cards.Select(c => c.CardId).ToList();

        var transactionsQuery = _context.Transactions
            .Include(t => t.Card)
            .Where(t => userCardIds.Contains(t.CardId) &&
                        t.TransactionDate.Year == selectedYear &&
                        t.TransactionDate.Month == selectedMonth)
            .AsQueryable();

        var endOfSelectedMonth = new DateTime(
            selectedYear,
            selectedMonth,
            DateTime.DaysInMonth(selectedYear, selectedMonth));

        var allTransactionsUpToMonth = await _context.Transactions
            .Include(t => t.Card)
            .Where(t => userCardIds.Contains(t.CardId) &&
                        t.TransactionDate <= endOfSelectedMonth)
            .ToListAsync();

        if (cardId.HasValue && userCardIds.Contains(cardId.Value))
        {
            transactionsQuery = transactionsQuery.Where(t => t.CardId == cardId.Value);

            allTransactionsUpToMonth = allTransactionsUpToMonth
                .Where(t => t.CardId == cardId.Value)
                .ToList();
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

        var overallIncome = allTransactionsUpToMonth
            .Where(t => t.TransactionType == "Income")
            .Sum(t => t.Amount);

        var overallExpense = allTransactionsUpToMonth
            .Where(t => t.TransactionType == "Expense")
            .Sum(t => t.Amount);

        var overallBalance = overallIncome - overallExpense;

        var viewModel = new MonthlySummaryViewModel
        {
            SelectedYear = selectedYear,
            SelectedMonth = selectedMonth,
            SelectedCardId = cardId,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            OverallBalance = overallBalance,
            Transactions = transactions,
            Cards = cards
        };

        return View(viewModel);
    }
}
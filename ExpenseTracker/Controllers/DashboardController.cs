using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

public class DashboardController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public DashboardController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalIncome = await _context.Transactions
            .Where(t => t.TransactionType == "Income")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var totalExpense = await _context.Transactions
            .Where(t => t.TransactionType == "Expense")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var totalCards = await _context.Cards.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();

        var viewModel = new DashboardViewModel
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            TotalCards = totalCards,
            TotalTransactions = totalTransactions
        };

        return View(viewModel);
    }
}
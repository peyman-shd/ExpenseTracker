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

        var installmentPaymentsQuery = _context.InstallmentPayments
            .Include(i => i.Transaction)
            .ThenInclude(t => t.Card)
            .Where(i => i.Transaction != null &&
                        userCardIds.Contains(i.Transaction.CardId) &&
                        i.DueYear == selectedYear &&
                        i.DueMonth == selectedMonth)
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

        var paidInstallmentsUpToMonth = await _context.InstallmentPayments
            .Include(i => i.Transaction)
            .ThenInclude(t => t.Card)
            .Where(i => i.Transaction != null &&
                        userCardIds.Contains(i.Transaction.CardId) &&
                        i.IsPaid &&
                        new DateTime(i.DueYear, i.DueMonth, 1) <= new DateTime(selectedYear, selectedMonth, 1))
            .ToListAsync();

        if (cardId.HasValue && userCardIds.Contains(cardId.Value))
        {
            transactionsQuery = transactionsQuery.Where(t => t.CardId == cardId.Value);

            installmentPaymentsQuery = installmentPaymentsQuery
                .Where(i => i.Transaction != null && i.Transaction.CardId == cardId.Value);

            allTransactionsUpToMonth = allTransactionsUpToMonth
                .Where(t => t.CardId == cardId.Value)
                .ToList();

            paidInstallmentsUpToMonth = paidInstallmentsUpToMonth
                .Where(i => i.Transaction != null && i.Transaction.CardId == cardId.Value)
                .ToList();
        }

        var transactions = await transactionsQuery
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        var installmentPayments = await installmentPaymentsQuery.ToListAsync();

        var installmentSummaries = installmentPayments
            .Select(i =>
            {
                var allPaymentsForThisTransaction = _context.InstallmentPayments
                    .Where(p => p.TransactionId == i.TransactionId)
                    .ToList();

                var totalAmount = allPaymentsForThisTransaction.Sum(p => p.Amount);

                var paidAmount = allPaymentsForThisTransaction
                    .Where(p => p.IsPaid)
                    .Sum(p => p.Amount);

                var remaining = totalAmount - paidAmount;

                return new MonthlyInstallmentSummary
                {
                    InstallmentPaymentId = i.InstallmentPaymentId,
                    Title = i.Transaction?.Title ?? "Installment",
                    CardName = i.Transaction?.Card?.CardName ?? "",
                    Amount = i.Amount,
                    IsPaid = i.IsPaid,
                    DueYear = i.DueYear,
                    DueMonth = i.DueMonth,
                    RemainingAmount = remaining
                };
            })
            .ToList();

        var totalIncome = transactions
            .Where(t => t.TransactionType == "Income")
            .Sum(t => t.Amount);

        var normalExpense = transactions
            .Where(t => t.TransactionType == "Expense" && !t.IsInstallment)
            .Sum(t => t.Amount);

        var paidInstallmentExpense = installmentPayments
            .Where(i => i.IsPaid)
            .Sum(i => i.Amount);

        var totalExpense = normalExpense + paidInstallmentExpense;

        var overallIncome = allTransactionsUpToMonth
            .Where(t => t.TransactionType == "Income")
            .Sum(t => t.Amount);

        var normalOverallExpense = allTransactionsUpToMonth
            .Where(t => t.TransactionType == "Expense" && !t.IsInstallment)
            .Sum(t => t.Amount);

        var paidOverallInstallments = paidInstallmentsUpToMonth
            .Sum(i => i.Amount);

        var overallExpense = normalOverallExpense + paidOverallInstallments;

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
            Cards = cards,
            Installments = installmentSummaries
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> MarkInstallmentAsPaid(int installmentPaymentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var installmentPayment = await _context.InstallmentPayments
            .Include(i => i.Transaction)
            .ThenInclude(t => t.Card)
            .FirstOrDefaultAsync(i =>
                i.InstallmentPaymentId == installmentPaymentId &&
                i.Transaction != null &&
                i.Transaction.Card != null &&
                i.Transaction.Card.UserId == userId);

        if (installmentPayment == null)
        {
            return NotFound();
        }

        installmentPayment.IsPaid = true;
        installmentPayment.PaidDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MonthlySummary), new
        {
            year = installmentPayment.DueYear,
            month = installmentPayment.DueMonth,
            cardId = installmentPayment.Transaction?.CardId
        });
    }
    
    public async Task<IActionResult> Charts(int? year, int? month, int? cardId)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    int selectedYear = year ?? DateTime.Now.Year;
    int selectedMonth = month ?? DateTime.Now.Month;

    var cards = await _context.Cards
        .Where(c => c.UserId == userId)
        .ToListAsync();

    var userCardIds = cards.Select(c => c.CardId).ToList();

    var transactions = await _context.Transactions
        .Where(t => userCardIds.Contains(t.CardId) &&
                    t.TransactionDate.Year == selectedYear &&
                    t.TransactionDate.Month == selectedMonth)
        .ToListAsync();

    var installmentPayments = await _context.InstallmentPayments
        .Include(i => i.Transaction)
        .Where(i => i.Transaction != null &&
                    userCardIds.Contains(i.Transaction.CardId) &&
                    i.DueYear == selectedYear &&
                    i.DueMonth == selectedMonth &&
                    i.IsPaid)
        .ToListAsync();

    var totalIncome = transactions
        .Where(t => t.TransactionType == "Income")
        .Sum(t => t.Amount);

    var normalExpense = transactions
        .Where(t => t.TransactionType == "Expense" && !t.IsInstallment)
        .Sum(t => t.Amount);

    var installmentExpense = installmentPayments.Sum(i => i.Amount);

    var totalExpense = normalExpense + installmentExpense;
    
    var categoryExpenses = transactions
        .Where(t => t.TransactionType == "Expense" && !t.IsInstallment)
        .GroupBy(t => t.Category ?? "Other")
        .Select(g => new
        {
            Category = g.Key,
            Total = g.Sum(t => t.Amount)
        })
        .ToList();

    ViewBag.TotalIncome = totalIncome;
    ViewBag.TotalExpense = totalExpense;

    ViewBag.CategoryLabels = categoryExpenses.Select(c => c.Category).ToList();
    ViewBag.CategoryValues = categoryExpenses.Select(c => c.Total).ToList();

    return View();
}
}
using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ExpenseTrackerDbContext _context;

        public DashboardController(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var cards = await _context.Cards
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CardType == "Debit")
                .ThenBy(c => c.CardName)
                .ToListAsync();

            var userCardIds = cards.Select(c => c.CardId).ToList();

            var currentMonthTransactions = await _context.Transactions
                .Where(t => userCardIds.Contains(t.CardId) &&
                            t.TransactionDate.Year == currentYear &&
                            t.TransactionDate.Month == currentMonth)
                .ToListAsync();

            var currentMonthInstallments = await _context.InstallmentPayments
                .Include(i => i.Transaction)
                .Where(i => i.Transaction != null &&
                            userCardIds.Contains(i.Transaction.CardId) &&
                            i.DueYear == currentYear &&
                            i.DueMonth == currentMonth &&
                            i.IsPaid)
                .ToListAsync();

            var totalIncome = currentMonthTransactions
                .Where(t => t.TransactionType == "Income")
                .Sum(t => t.Amount);

            var normalExpense = currentMonthTransactions
                .Where(t => t.TransactionType == "Expense" && !t.IsInstallment)
                .Sum(t => t.Amount);

            var paidInstallmentExpense = currentMonthInstallments
                .Sum(i => i.Amount);

            var totalExpense = normalExpense + paidInstallmentExpense;

            var allTransactions = await _context.Transactions
                .Where(t => userCardIds.Contains(t.CardId))
                .ToListAsync();

            var allInstallments = await _context.InstallmentPayments
                .Include(i => i.Transaction)
                .Where(i => i.Transaction != null &&
                            userCardIds.Contains(i.Transaction.CardId))
                .ToListAsync();

            var cardSummaries = cards.Select(card =>
            {
                decimal currentDebt = 0;
                decimal installmentPortion = 0;
                decimal regularDebt = 0;

                if (card.CardType == "Credit")
                {
                    var totalCreditExpenses = allTransactions
                        .Where(t => t.CardId == card.CardId && t.TransactionType == "Expense")
                        .Sum(t => t.Amount);

                    var totalCreditPayments = allTransactions
                        .Where(t => t.TransactionType == "Transfer" && t.ToCardId == card.CardId)
                        .Sum(t => t.Amount);

                    currentDebt = totalCreditExpenses - totalCreditPayments;

                    installmentPortion = allInstallments
                        .Where(i => i.Transaction != null &&
                                    i.Transaction.CardId == card.CardId &&
                                    !i.IsPaid)
                        .Sum(i => i.Amount);

                    regularDebt = currentDebt - installmentPortion;

                    if (regularDebt < 0)
                    {
                        regularDebt = 0;
                    }
                }

                return new DashboardCardSummary
                {
                    CardName = card.CardName,
                    CardType = card.CardType,
                    CurrentBalance = card.CurrentBalance,
                    CurrentDebt = currentDebt,
                    InstallmentPortion = installmentPortion,
                    RegularDebt = regularDebt
                };
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetBalance = totalIncome - totalExpense,
                CardSummaries = cardSummaries
            };

            return View(viewModel);
        }
    }
}
using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[Authorize]
public class TransactionController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public TransactionController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? cardId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userCards = await _context.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync();

        ViewBag.Cards = userCards;
        ViewBag.SelectedCardId = cardId;

        var userCardIds = userCards.Select(c => c.CardId).ToList();

        var transactionsQuery = _context.Transactions
            .Include(t => t.Card)
            .Where(t => userCardIds.Contains(t.CardId))
            .AsQueryable();

        if (cardId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.CardId == cardId.Value);
        }

        var transactions = await transactionsQuery
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        return View(transactions);
    }

    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var cards = await _context.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync();

        ViewBag.Cards = cards;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!ModelState.IsValid)
        {
            ViewBag.Cards = await _context.Cards
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(transaction);
        }

        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == transaction.CardId && c.UserId == userId);

        if (card == null)
        {
            return NotFound();
        }

        if (transaction.TransactionType == "Expense")
        {
            card.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.TransactionType == "Income")
        {
            card.CurrentBalance += transaction.Amount;
        }
        else if (transaction.TransactionType == "Transfer")
        {
            if (transaction.ToCardId == null)
            {
                ModelState.AddModelError("ToCardId", "Please select a destination card.");

                ViewBag.Cards = await _context.Cards
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return View(transaction);
            }

            if (transaction.ToCardId == transaction.CardId)
            {
                ModelState.AddModelError("ToCardId", "Source and destination cannot be the same.");

                ViewBag.Cards = await _context.Cards
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return View(transaction);
            }

            var destinationCard = await _context.Cards
                .FirstOrDefaultAsync(c => c.CardId == transaction.ToCardId && c.UserId == userId);

            if (destinationCard == null)
            {
                ModelState.AddModelError("ToCardId", "Destination card not found.");

                ViewBag.Cards = await _context.Cards
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return View(transaction);
            }

            card.CurrentBalance -= transaction.Amount;
            destinationCard.CurrentBalance += transaction.Amount;
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userCards = await _context.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var userCardIds = userCards.Select(c => c.CardId).ToList();

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == id && userCardIds.Contains(t.CardId));

        if (transaction == null)
        {
            return NotFound();
        }

        ViewBag.Cards = userCards;
        return View(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Transaction transaction)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!ModelState.IsValid)
        {
            ViewBag.Cards = await _context.Cards
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(transaction);
        }

        var userCards = await _context.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var userCardIds = userCards.Select(c => c.CardId).ToList();

        var existingTransaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId && userCardIds.Contains(t.CardId));

        if (existingTransaction == null)
        {
            return NotFound();
        }

        var oldSourceCard = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == existingTransaction.CardId && c.UserId == userId);

        if (oldSourceCard != null)
        {
            if (existingTransaction.TransactionType == "Expense")
            {
                oldSourceCard.CurrentBalance += existingTransaction.Amount;
            }
            else if (existingTransaction.TransactionType == "Income")
            {
                oldSourceCard.CurrentBalance -= existingTransaction.Amount;
            }
            else if (existingTransaction.TransactionType == "Transfer")
            {
                oldSourceCard.CurrentBalance += existingTransaction.Amount;

                var oldDestinationCard = await _context.Cards
                    .FirstOrDefaultAsync(c => c.CardId == existingTransaction.ToCardId && c.UserId == userId);

                if (oldDestinationCard != null)
                {
                    oldDestinationCard.CurrentBalance -= existingTransaction.Amount;
                }
            }
        }

        var newSourceCard = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == transaction.CardId && c.UserId == userId);

        if (newSourceCard == null)
        {
            ViewBag.Cards = userCards;
            return View(transaction);
        }

        if (transaction.TransactionType == "Expense")
        {
            newSourceCard.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.TransactionType == "Income")
        {
            newSourceCard.CurrentBalance += transaction.Amount;
        }
        else if (transaction.TransactionType == "Transfer")
        {
            if (transaction.ToCardId == null)
            {
                ModelState.AddModelError("ToCardId", "Please select a destination card.");
                ViewBag.Cards = userCards;
                return View(transaction);
            }

            if (transaction.ToCardId == transaction.CardId)
            {
                ModelState.AddModelError("ToCardId", "Source and destination cannot be the same.");
                ViewBag.Cards = userCards;
                return View(transaction);
            }

            var newDestinationCard = await _context.Cards
                .FirstOrDefaultAsync(c => c.CardId == transaction.ToCardId && c.UserId == userId);

            if (newDestinationCard == null)
            {
                ModelState.AddModelError("ToCardId", "Destination card not found.");
                ViewBag.Cards = userCards;
                return View(transaction);
            }

            newSourceCard.CurrentBalance -= transaction.Amount;
            newDestinationCard.CurrentBalance += transaction.Amount;
        }

        existingTransaction.Title = transaction.Title;
        existingTransaction.Amount = transaction.Amount;
        existingTransaction.TransactionType = transaction.TransactionType;
        existingTransaction.Category = transaction.Category;
        existingTransaction.TransactionDate = transaction.TransactionDate;
        existingTransaction.CardId = transaction.CardId;
        existingTransaction.ToCardId = transaction.ToCardId;
        existingTransaction.IsInstallment = transaction.IsInstallment;
        existingTransaction.NumberOfInstallments = transaction.NumberOfInstallments;
        existingTransaction.InstallmentAmount = transaction.InstallmentAmount;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userCardIds = await _context.Cards
            .Where(c => c.UserId == userId)
            .Select(c => c.CardId)
            .ToListAsync();

        var transaction = await _context.Transactions
            .Include(t => t.Card)
            .FirstOrDefaultAsync(t => t.TransactionId == id && userCardIds.Contains(t.CardId));

        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int transactionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userCardIds = await _context.Cards
            .Where(c => c.UserId == userId)
            .Select(c => c.CardId)
            .ToListAsync();

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId && userCardIds.Contains(t.CardId));

        if (transaction != null)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(c => c.CardId == transaction.CardId && c.UserId == userId);

            if (card != null)
            {
                if (transaction.TransactionType == "Expense")
                {
                    card.CurrentBalance += transaction.Amount;
                }
                else if (transaction.TransactionType == "Income")
                {
                    card.CurrentBalance -= transaction.Amount;
                }
                else if (transaction.TransactionType == "Transfer")
                {
                    var destinationCard = await _context.Cards
                        .FirstOrDefaultAsync(c => c.CardId == transaction.ToCardId && c.UserId == userId);

                    card.CurrentBalance += transaction.Amount;

                    if (destinationCard != null)
                    {
                        destinationCard.CurrentBalance -= transaction.Amount;
                    }
                }
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
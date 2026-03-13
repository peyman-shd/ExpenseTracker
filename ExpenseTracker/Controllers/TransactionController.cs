using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

public class TransactionController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public TransactionController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? cardId)
    {
        ViewBag.Cards = await _context.Cards.ToListAsync();
        ViewBag.SelectedCardId = cardId;

        var transactionsQuery = _context.Transactions
            .Include(t => t.Card)
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
        var cards = await _context.Cards.ToListAsync();
        ViewBag.Cards = cards;

        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        if (!ModelState.IsValid)
        {
            var cards = await _context.Cards.ToListAsync();
            ViewBag.Cards = cards;
            return View(transaction);
        }

        var card = await _context.Cards.FindAsync(transaction.CardId);

        if (card == null)
        {
            var cards = await _context.Cards.ToListAsync();
            ViewBag.Cards = cards;
            return View(transaction);
        }

        if (transaction.TransactionType == "Expense")
        {
            card.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.TransactionType == "Income")
        {
            card.CurrentBalance += transaction.Amount;
        }

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Card)
            .FirstOrDefaultAsync(t => t.TransactionId == id);

        if (transaction == null)
        {
            return NotFound();
        }

        return View(transaction);
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);

        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
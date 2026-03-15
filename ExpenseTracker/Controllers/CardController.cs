using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

public class CardController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public CardController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cards = await _context.Cards.ToListAsync();
        return View(cards);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Card card)
    {
        if (!ModelState.IsValid)
        {
            return View(card);
        }

        card.UserId = 1;
        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    public async Task<IActionResult> Edit(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        return View(card);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(Card card)
    {
        if (!ModelState.IsValid)
        {
            return View(card);
        }

        var existingCard = await _context.Cards.FindAsync(card.CardId);

        if (existingCard == null)
        {
            return NotFound();
        }

        existingCard.CardName = card.CardName;
        existingCard.Limit = card.Limit;
        existingCard.CurrentBalance = card.CurrentBalance;
        existingCard.CardType = card.CardType;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        return View(card);
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int cardId)
    {
        var card = await _context.Cards.FindAsync(cardId);

        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

    

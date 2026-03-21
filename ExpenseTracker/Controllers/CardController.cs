using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[Authorize]
public class CardController : Controller
{
    private readonly ExpenseTrackerDbContext _context;

    public CardController(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var cards = await _context.Cards
            .Where(c => c.UserId == userId)
            .ToListAsync();

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

        card.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == id && c.UserId == userId);

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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var existingCard = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == card.CardId && c.UserId == userId);

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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == id && c.UserId == userId);

        if (card == null)
        {
            return NotFound();
        }

        return View(card);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int cardId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var card = await _context.Cards
            .FirstOrDefaultAsync(c => c.CardId == cardId && c.UserId == userId);

        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
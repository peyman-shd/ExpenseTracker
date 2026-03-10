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
}

    

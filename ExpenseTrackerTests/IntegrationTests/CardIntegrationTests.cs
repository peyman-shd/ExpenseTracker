using System.Security.Claims;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTrackerTests.IntegrationTests;

public class CardIntegrationTests
{
    [Fact]
    public async Task Create_Card_Should_Save_Card_And_Assign_UserId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpenseTrackerDbContext(options);

        var userId = "user-1";

        var controller = new CardController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var card = new Card
        {
            CardName = "RBC Visa",
            CardType = "Credit",
            CurrentBalance = 3000m
        };

        // Act
        var result = await controller.Create(card);

        var savedCard = await context.Cards.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(savedCard);
        Assert.Equal("RBC Visa", savedCard.CardName);
        Assert.Equal("Credit", savedCard.CardType);
        Assert.Equal(3000m, savedCard.CurrentBalance);
        Assert.Equal(userId, savedCard.UserId);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
    
    [Fact]
    public async Task Delete_Card_Should_Remove_Card_And_Redirect_To_Index()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpenseTrackerDbContext(options);

        var userId = "user-1";

        var card = new Card
        {
            CardId = 1,
            CardName = "CIBC Visa",
            CardType = "Credit",
            CurrentBalance = 2500m,
            UserId = userId
        };

        context.Cards.Add(card);
        await context.SaveChangesAsync();

        var controller = new CardController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.DeleteConfirmed(1);

        var deletedCard = await context.Cards.FirstOrDefaultAsync();

        // Assert
        Assert.Null(deletedCard);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
    
    [Fact]
    public async Task Edit_Card_Should_Update_Card_And_Redirect_To_Index()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpenseTrackerDbContext(options);

        var userId = "user-1";

        var existingCard = new Card
        {
            CardId = 1,
            CardName = "Old Card",
            CardType = "Debit",
            CurrentBalance = 1500m,
            UserId = userId
        };

        context.Cards.Add(existingCard);
        await context.SaveChangesAsync();

        var controller = new CardController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var updatedCard = new Card
        {
            CardId = 1,
            CardName = "Updated Card",
            CardType = "Credit",
            CurrentBalance = 3000m,
            UserId = userId
        };

        // Act
        var result = await controller.Edit(updatedCard);

        var savedCard = await context.Cards.FirstAsync();

        // Assert
        Assert.Equal("Updated Card", savedCard.CardName);
        Assert.Equal("Credit", savedCard.CardType);
        Assert.Equal(3000m, savedCard.CurrentBalance);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
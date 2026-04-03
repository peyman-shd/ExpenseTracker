
using System.Security.Claims;
using ExpenseTracker.Controllers;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ExpenseTrackerTests;

public class TransactionIntegrationTests
{
    [Fact]
    public async Task Create_Expense_Transaction_Should_Save_And_Update_Card_Balance()
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
            CardName = "Test Card",
            CardType = "Debit",
            UserId = userId,
            CurrentBalance = 5000m
        };

        context.Cards.Add(card);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var transaction = new Transaction
        {
            Title = "Groceries",
            Amount = 1000m,
            TransactionType = "Expense",
            Category = "Grocery",
            TransactionDate = DateTime.UtcNow,
            CardId = 1
        };

        // Act
        var result = await controller.Create(transaction);

        var savedTransaction = await context.Transactions.FirstOrDefaultAsync();
        var updatedCard = await context.Cards.FirstAsync();

        // Assert
        Assert.NotNull(savedTransaction);
        Assert.Equal("Groceries", savedTransaction.Title);
        Assert.Equal(1000m, savedTransaction.Amount);
        Assert.Equal(4000m, updatedCard.CurrentBalance);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_Expense_Transaction_Should_Remove_Transaction_And_Restore_Card_Balance()
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
            CardName = "Test Card",
            CardType = "Debit",
            UserId = userId,
            CurrentBalance = 4000m
        };

        var transaction = new Transaction
        {
            TransactionId = 1,
            Title = "Groceries",
            Amount = 1000m,
            TransactionType = "Expense",
            Category = "Grocery",
            TransactionDate = DateTime.UtcNow,
            CardId = 1
        };

        context.Cards.Add(card);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

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

        var deletedTransaction = await context.Transactions.FirstOrDefaultAsync();
        var updatedCard = await context.Cards.FirstAsync();

        // Assert
        Assert.Null(deletedTransaction);
        Assert.Equal(5000m, updatedCard.CurrentBalance);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Expense_Transaction_Should_Update_Transaction_And_Recalculate_Card_Balance()
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
            CardName = "Test Card",
            CardType = "Debit",
            UserId = userId,
            CurrentBalance = 4000m
        };

        var existingTransaction = new Transaction
        {
            TransactionId = 1,
            Title = "Old Expense",
            Amount = 1000m,
            TransactionType = "Expense",
            Category = "Shopping",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            CardId = 1
        };

        context.Cards.Add(card);
        context.Transactions.Add(existingTransaction);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var updatedTransaction = new Transaction
        {
            TransactionId = 1,
            Title = "Updated Expense",
            Amount = 500m,
            TransactionType = "Expense",
            Category = "Grocery",
            TransactionDate = DateTime.UtcNow,
            CardId = 1
        };

        // Act
        var result = await controller.Edit(updatedTransaction);

        var savedTransaction = await context.Transactions.FirstAsync();
        var updatedCard = await context.Cards.FirstAsync();

        // Assert
        Assert.Equal("Updated Expense", savedTransaction.Title);
        Assert.Equal(500m, savedTransaction.Amount);
        Assert.Equal("Grocery", savedTransaction.Category);

        Assert.Equal(4500m, updatedCard.CurrentBalance);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Create_Transfer_Transaction_Should_Update_Both_Card_Balances()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ExpenseTrackerDbContext(options);

        var userId = "user-1";

        var sourceCard = new Card
        {
            CardId = 1,
            CardName = "Source Card",
            CardType = "Debit",
            UserId = userId,
            CurrentBalance = 5000m
        };

        var destinationCard = new Card
        {
            CardId = 2,
            CardName = "Destination Card",
            CardType = "Debit",
            UserId = userId,
            CurrentBalance = 2000m
        };

        context.Cards.Add(sourceCard);
        context.Cards.Add(destinationCard);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var transferTransaction = new Transaction
        {
            Title = "Transfer",
            Amount = 1000m,
            TransactionType = "Transfer",
            Category = "Other",
            TransactionDate = DateTime.UtcNow,
            CardId = 1,
            ToCardId = 2
        };

        // Act
        var result = await controller.Create(transferTransaction);

        var savedTransaction = await context.Transactions.FirstOrDefaultAsync();
        var updatedSourceCard = await context.Cards.FirstAsync(c => c.CardId == 1);
        var updatedDestinationCard = await context.Cards.FirstAsync(c => c.CardId == 2);

        // Assert
        Assert.NotNull(savedTransaction);
        Assert.Equal("Transfer", savedTransaction.Title);
        Assert.Equal(4000m, updatedSourceCard.CurrentBalance);
        Assert.Equal(3000m, updatedDestinationCard.CurrentBalance);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_Installment_Transaction_With_Paid_Installments_Should_Be_Blocked()
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
            CardName = "Credit Card",
            CardType = "Credit",
            UserId = userId,
            CurrentBalance = 2000m
        };

        var transaction = new Transaction
        {
            TransactionId = 1,
            Title = "Laptop",
            Amount = 3000m,
            TransactionType = "Expense",
            Category = "Shopping",
            TransactionDate = DateTime.UtcNow,
            CardId = 1,
            IsInstallment = true,
            NumberOfInstallments = 6,
            InstallmentAmount = 500m,
            InstallmentStartDate = DateTime.UtcNow
        };

        var paidInstallment = new InstallmentPayment
        {
            InstallmentPaymentId = 1,
            TransactionId = 1,
            Amount = 500m,
            DueMonth = 1,
            DueYear = 2026,
            IsPaid = true,
            PaidDate = DateTime.UtcNow
        };

        context.Cards.Add(card);
        context.Transactions.Add(transaction);
        context.InstallmentPayments.Add(paidInstallment);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        
        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            Mock.Of<ITempDataProvider>());

        // Act
        var result = await controller.DeleteConfirmed(1);

        var existingTransaction = await context.Transactions.FirstOrDefaultAsync();
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        // Assert
        Assert.NotNull(existingTransaction);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal(
            "This installment transaction cannot be deleted because some installments are already paid.",
            controller.TempData["ErrorMessage"]?.ToString());
    }
}
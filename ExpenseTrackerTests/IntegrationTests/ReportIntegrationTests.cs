using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTrackerTests.IntegrationTests;

public class ReportIntegrationTests
{
    [Fact]
    public async Task MarkInstallmentAsPaid_Should_Update_Status_And_Set_Date()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb1")
            .Options;

        using var context = new ExpenseTrackerDbContext(options);

        var userId = "user-1";

        var card = new Card
        {
            CardId = 1,
            CardName = "Test Card",
            CardType = "Credit",
            UserId = userId,
            CurrentBalance = 5000m
        };

        var transaction = new Transaction
        {
            TransactionId = 1,
            Title = "Laptop",
            Amount = 3000m,
            TransactionType = "Expense",
            Category = "Shopping",
            TransactionDate = DateTime.UtcNow,
            CardId = 1
        };

        var installment = new InstallmentPayment
        {
            InstallmentPaymentId = 1,
            TransactionId = 1,
            Amount = 500m,
            DueMonth = 1,
            DueYear = 2026,
            IsPaid = false
        };

        context.Cards.Add(card);
        context.Transactions.Add(transaction);
        context.InstallmentPayments.Add(installment);
        await context.SaveChangesAsync();

        var controller = new ReportController(context);

        // fake user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.MarkInstallmentAsPaid(1);

        var updatedInstallment = await context.InstallmentPayments.FirstAsync();

        // Assert
        Assert.True(updatedInstallment.IsPaid);
        Assert.NotNull(updatedInstallment.PaidDate);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
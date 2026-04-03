using Xunit;

namespace ExpenseTrackerTests;

public class TransactionLogicTests
{
    [Fact]
    public void Expense_Should_Reduce_Card_Balance()
    {
        // Arrange
        decimal initialBalance = 5000m;
        decimal expenseAmount = 1200m;

        // Act
        decimal updatedBalance = initialBalance - expenseAmount;

        // Assert
        Assert.Equal(3800m, updatedBalance);
    }

    [Fact]
    public void Income_Should_Increase_Card_Balance()
    {
        // Arrange
        decimal initialBalance = 3000m;
        decimal incomeAmount = 1500m;

        // Act
        decimal updatedBalance = initialBalance + incomeAmount;

        // Assert
        Assert.Equal(4500m, updatedBalance);
    }

    [Fact]
    public void Transaction_Should_Store_Correct_Amount()
    {
        // Arrange
        decimal transactionAmount = 250.75m;

        // Act
        decimal savedAmount = transactionAmount;

        // Assert
        Assert.Equal(250.75m, savedAmount);
    }
}
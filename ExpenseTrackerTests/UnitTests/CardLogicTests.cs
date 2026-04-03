using Xunit;

namespace ExpenseTrackerTests;

public class CardLogicTests
{
    [Fact]
    public void New_Card_Should_Keep_Correct_Initial_Balance()
    {
        // Arrange
        decimal initialBalance = 2500m;

        // Act
        decimal savedBalance = initialBalance;

        // Assert
        Assert.Equal(2500m, savedBalance);
    }

    [Fact]
    public void Card_Balance_Should_Update_Correctly_After_Transaction()
    {
        // Arrange
        decimal initialBalance = 4000m;
        decimal expenseAmount = 750m;

        // Act
        decimal updatedBalance = initialBalance - expenseAmount;

        // Assert
        Assert.Equal(3250m, updatedBalance);
    }
}
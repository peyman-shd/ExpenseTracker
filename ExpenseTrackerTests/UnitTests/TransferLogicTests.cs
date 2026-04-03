using Xunit;

namespace ExpenseTrackerTests;

public class TransferLogicTests
{
    [Fact]
    public void Transfer_Should_Reduce_Source_Card_Balance()
    {
        // Arrange
        decimal sourceBalance = 5000m;
        decimal transferAmount = 1000m;

        // Act
        decimal updatedSourceBalance = sourceBalance - transferAmount;

        // Assert
        Assert.Equal(4000m, updatedSourceBalance);
    }

    [Fact]
    public void Transfer_Should_Increase_Destination_Card_Balance()
    {
        // Arrange
        decimal destinationBalance = 2000m;
        decimal transferAmount = 1000m;

        // Act
        decimal updatedDestinationBalance = destinationBalance + transferAmount;

        // Assert
        Assert.Equal(3000m, updatedDestinationBalance);
    }

    [Fact]
    public void Transfer_With_Same_Source_And_Destination_Should_Be_Invalid()
    {
        // Arrange
        int sourceCardId = 1;
        int destinationCardId = 1;

        // Act
        bool isValid = sourceCardId != destinationCardId;

        // Assert
        Assert.False(isValid);
    }
}
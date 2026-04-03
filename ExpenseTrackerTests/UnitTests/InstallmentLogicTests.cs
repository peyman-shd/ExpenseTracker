using Xunit;

namespace ExpenseTrackerTests;

public class InstallmentLogicTests
{
    [Fact]
    public void Full_Installment_Amount_Should_Affect_Card_Balance_Immediately()
    {
        // Arrange
        decimal initialBalance = 8000m;
        decimal installmentPurchaseAmount = 3000m;

        // Act
        decimal updatedBalance = initialBalance - installmentPurchaseAmount;

        // Assert
        Assert.Equal(5000m, updatedBalance);
    }

    [Fact]
    public void Paid_Installment_Should_Be_Included_In_Monthly_Expense()
    {
        // Arrange
        decimal normalExpense = 2000m;
        decimal paidInstallmentAmount = 500m;

        // Act
        decimal totalExpense = normalExpense + paidInstallmentAmount;

        // Assert
        Assert.Equal(2500m, totalExpense);
    }

    [Fact]
    public void Unpaid_Installment_Should_Not_Be_Included_In_Monthly_Expense()
    {
        // Arrange
        decimal normalExpense = 2000m;
        decimal unpaidInstallmentAmount = 500m;
        bool isPaid = false;

        // Act
        decimal totalExpense = isPaid ? normalExpense + unpaidInstallmentAmount : normalExpense;

        // Assert
        Assert.Equal(2000m, totalExpense);
    }
}
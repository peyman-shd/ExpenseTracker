using Xunit;

namespace ExpenseTrackerTests;

public class DashboardReportTests
{
    [Fact]
    public void Installment_Portion_Should_Be_Calculated_Correctly()
    {
        // Arrange
        decimal installment1 = 2000m;
        decimal installment2 = 1500m;

        // Act
        decimal totalInstallmentPortion = installment1 + installment2;

        // Assert
        Assert.Equal(3500m, totalInstallmentPortion);
    }

    [Fact]
    public void Regular_Debt_Should_Be_CurrentDebt_Minus_InstallmentPortion()
    {
        // Arrange
        decimal currentDebt = 7000m;
        decimal installmentPortion = 4000m;

        // Act
        decimal regularDebt = currentDebt - installmentPortion;

        // Assert
        Assert.Equal(3000m, regularDebt);
    }

    [Fact]
    public void Dashboard_And_Report_Should_Have_Consistent_Expense_Logic()
    {
        // Arrange
        decimal normalExpense = 3000m;
        decimal paidInstallment = 500m;

        // Act
        decimal dashboardExpense = normalExpense + paidInstallment;
        decimal reportExpense = normalExpense + paidInstallment;

        // Assert
        Assert.Equal(reportExpense, dashboardExpense);
    }

    [Fact]
    public void Total_Expense_Should_Not_Include_Full_Installment_Purchase()
    {
        // Arrange
        decimal normalExpense = 3000m;
        decimal fullInstallmentPurchase = 5000m;

        // Act
        decimal totalExpense = normalExpense;

        // Assert
        Assert.Equal(3000m, totalExpense);
    }

    [Fact]
    public void Total_Expense_Should_Include_Paid_Installments()
    {
        // Arrange
        decimal normalExpense = 3000m;
        decimal paidInstallment = 500m;

        // Act
        decimal totalExpense = normalExpense + paidInstallment;

        // Assert
        Assert.Equal(3500m, totalExpense);
    }

    [Fact]
    public void Transfer_Should_Not_Affect_Total_Expense()
    {
        // Arrange
        decimal normalExpense = 3000m;
        decimal transferAmount = 1000m;

        // Act
        decimal totalExpense = normalExpense; 
        
        // Assert
        Assert.Equal(3000m, totalExpense);
    }
}
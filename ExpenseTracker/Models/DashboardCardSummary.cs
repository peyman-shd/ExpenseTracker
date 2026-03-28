namespace ExpenseTracker.Models;

    public class DashboardCardSummary
    {
        public string CardName { get; set; } = string.Empty;

        public string CardType { get; set; } = string.Empty;

        public decimal CurrentBalance { get; set; }

        public decimal CurrentDebt { get; set; }
        
        public decimal InstallmentPortion { get; set; }
        
        public decimal RegularDebt { get; set; }
        
        public List<DashboardCardSummary> CardSummaries { get; set; } = new();
    }

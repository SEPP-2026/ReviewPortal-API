using ReviewPortal.Domain.Entities;

namespace ReviewPortal.UnitTests.Domain;

public class ReviewTests
{
    [Fact]
    public void CalculateOverallRating_WithFiveScores_SetsArithmeticAverage()
    {
        var review = new Review
        {
            ToolId = 1,
            ReviewerName = "Alex",
            ReviewerEmail = "alex@example.com",
            ReviewText = "Very good experience.",
            EquipmentRating = 5,
            CustomerServiceRating = 4,
            TechnicalSupportRating = 3,
            AfterSalesRating = 4,
            ValueForMoneyRating = 4
        };

        review.CalculateOverallRating();

        Assert.Equal(4.0m, review.OverallRating);
    }
}

namespace ReviewPortal.Application.DTOs.Reviews;

public record CreateReviewRequest(
    string ReviewerName,
    string ReviewerEmail,
    string ReviewText,
    int EquipmentRating,
    int CustomerServiceRating,
    int TechnicalSupportRating,
    int AfterSalesRating,
    int ValueForMoneyRating
);

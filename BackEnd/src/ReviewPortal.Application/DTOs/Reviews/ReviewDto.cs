namespace ReviewPortal.Application.DTOs.Reviews;

public record ReviewDto(
    int Id,
    int ToolId,
    string ToolName,
    string ReviewerName,
    string ReviewText,
    int EquipmentRating,
    int CustomerServiceRating,
    int TechnicalSupportRating,
    int AfterSalesRating,
    int ValueForMoneyRating,
    decimal OverallRating,
    string Status,
    string? RejectionReason,
    DateTime CreatedDate,
    List<ReviewCommentDto> Comments,
    CompanyResponseDto? CompanyResponse
);

public record ReviewCommentDto(
    int Id,
    string CommenterName,
    string CommentText,
    string Status,
    DateTime CreatedDate
);

public record CompanyResponseDto(
    int Id,
    string ResponseText,
    string StaffName,
    DateTime CreatedDate,
    DateTime UpdatedDate
);

public record ReviewSummaryDto(
    int Id,
    int ToolId,
    string ToolName,
    string ReviewTextSnippet,
    decimal OverallRating,
    string Status,
    string? RejectionReason,
    DateTime CreatedDate,
    bool HasCompanyResponse
);

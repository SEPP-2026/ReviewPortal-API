namespace ReviewPortal.Application.DTOs.Tools;

public record ToolDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string Description,
    decimal HourlyRate,
    decimal DailyRate,
    decimal WeeklyRate,
    string? SpecialNotes,
    bool DepositRequired,
    decimal? DepositAmount,
    bool IsActive,
    decimal? OverallRating,
    int ReviewCount,
    List<ToolImageDto> Images,
    DateTime CreatedDate,
    DateTime UpdatedDate
);

public record ToolImageDto(
    int Id,
    string ImageUrl,
    int DisplayOrder
);

public record ToolSummaryDto(
    int Id,
    string Name,
    string CategoryName,
    decimal StartingPrice,
    string StartingPriceUnit,
    decimal DailyRate,
    decimal? OverallRating,
    int ReviewCount,
    string? ThumbnailUrl
);

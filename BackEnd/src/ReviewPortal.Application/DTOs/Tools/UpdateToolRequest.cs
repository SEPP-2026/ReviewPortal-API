namespace ReviewPortal.Application.DTOs.Tools;

public record UpdateToolRequest(
    int CategoryId,
    string Name,
    string Description,
    decimal HourlyRate,
    decimal DailyRate,
    decimal WeeklyRate,
    string? SpecialNotes,
    bool DepositRequired,
    decimal? DepositAmount
);

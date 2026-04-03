namespace ReviewPortal.Application.DTOs.Tools;

public record CreateToolRequest(
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

namespace ReviewPortal.Application.DTOs.Tools;

public record RentalCalculationRequest(
    DateTime StartDateTime,
    DateTime EndDateTime
);

public record RentalCalculationResponse(
    string ToolName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string Breakdown,
    decimal TotalCost
);

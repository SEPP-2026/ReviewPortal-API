namespace ReviewPortal.Application.DTOs.Categories;

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int ToolCount
);

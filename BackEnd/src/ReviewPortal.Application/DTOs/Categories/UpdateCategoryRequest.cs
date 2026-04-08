namespace ReviewPortal.Application.DTOs.Categories;

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl
);

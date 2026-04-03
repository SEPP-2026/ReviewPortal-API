namespace ReviewPortal.Application.DTOs.Categories;

public record CreateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl
);

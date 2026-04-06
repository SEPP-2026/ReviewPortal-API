using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;

namespace ReviewPortal.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}

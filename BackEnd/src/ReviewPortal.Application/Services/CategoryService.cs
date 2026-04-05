using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Interfaces;

namespace ReviewPortal.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllWithToolsAsync(cancellationToken);
        
        var categoryDtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ImageUrl,
            c.Tools.Count
        )).ToList();

        return Result<IReadOnlyList<CategoryDto>>.Success(categoryDtos);
    }

    public Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

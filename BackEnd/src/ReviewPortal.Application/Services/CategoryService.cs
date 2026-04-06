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

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            return Result<CategoryDto>.Failure($"Category with ID {id} not found.");

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.Tools?.Count ?? 0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new ReviewPortal.Domain.Entities.Category
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto(
            createdCategory.Id,
            createdCategory.Name,
            createdCategory.Description,
            createdCategory.ImageUrl,
            0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            return Result<CategoryDto>.Failure($"Category with ID {id} not found.");

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.Tools?.Count ?? 0
        );

        return Result<CategoryDto>.Success(categoryDto);
    }

    public async Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
            return Result<bool>.Failure($"Category with ID {id} not found.");

        if (category.Tools != null && category.Tools.Any())
            return Result<bool>.Failure($"Cannot delete category with ID {id} because it has associated tools.");

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

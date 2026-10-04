using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Categories;

public sealed record CreateCategoryCommand(Guid CompanyId, string Name, Guid? ParentCategoryId = null, string? Description = null);
public sealed record UpdateCategoryCommand(Guid CompanyId, Guid CategoryId, string Name, Guid? ParentCategoryId = null, string? Description = null, bool IsActive = true);

public interface ICategoryService
{
    Task<Category> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> ListAsync(Guid companyId, bool includeInactive = false, CancellationToken cancellationToken = default);
}

public sealed class CategoryService(IRepository<Category> categories, IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<Category> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name);
        if (command.ParentCategoryId == Guid.Empty) command = command with { ParentCategoryId = null };
        if (command.ParentCategoryId is not null && !await categories.Query().AnyAsync(x => x.Id == command.ParentCategoryId && x.CompanyId == command.CompanyId, cancellationToken))
            throw new InvalidOperationException("A categoria principal não existe nesta empresa.");
        if (await categories.Query().AnyAsync(x => x.CompanyId == command.CompanyId && x.Name == command.Name.Trim(), cancellationToken))
            throw new InvalidOperationException("Já existe uma categoria com este nome.");

        var category = new Category { CompanyId = command.CompanyId, Name = command.Name.Trim(), ParentCategoryId = command.ParentCategoryId, Description = command.Description?.Trim(), IsActive = true };
        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<Category> UpdateAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name);
        var category = await categories.Query().FirstOrDefaultAsync(x => x.Id == command.CategoryId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        if (command.ParentCategoryId == category.Id) throw new InvalidOperationException("Uma categoria não pode ser a sua própria categoria principal.");
        if (command.ParentCategoryId is not null && !await categories.Query().AnyAsync(x => x.Id == command.ParentCategoryId && x.CompanyId == command.CompanyId, cancellationToken))
            throw new InvalidOperationException("A categoria principal não existe nesta empresa.");
        if (await categories.Query().AnyAsync(x => x.CompanyId == command.CompanyId && x.Id != category.Id && x.Name == command.Name.Trim(), cancellationToken))
            throw new InvalidOperationException("Já existe uma categoria com este nome.");

        category.Name = command.Name.Trim();
        category.ParentCategoryId = command.ParentCategoryId;
        category.Description = command.Description?.Trim();
        category.IsActive = command.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;
        categories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<IReadOnlyList<Category>> ListAsync(Guid companyId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = categories.Query().Where(x => x.CompanyId == companyId);
        if (!includeInactive) query = query.Where(x => x.IsActive);
        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome da categoria é obrigatório.");
    }
}

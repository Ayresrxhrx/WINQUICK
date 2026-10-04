namespace WinQuick.Core.Entities;

public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

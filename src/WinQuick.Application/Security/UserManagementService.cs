using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;
using CoreRole = WinQuick.Core.Security.Role;
using CoreUserRole = WinQuick.Core.Security.UserRole;

namespace WinQuick.Application.Security;

public sealed record CreateUserCommand(Guid CompanyId, string Username, string DisplayName, string Password, Guid RoleId);

public sealed class UserManagementService(
    IRepository<User> users,
    IRepository<CoreRole> roles,
    IRepository<CoreUserRole> userRoles,
    IUnitOfWork unitOfWork)
{
    public Task<List<User>> ListAsync(Guid companyId, CancellationToken cancellationToken = default)
        => users.Query().Where(x => x.CompanyId == companyId).OrderBy(x => x.Username).ToListAsync(cancellationToken);

    public Task<List<CoreRole>> ListRolesAsync(Guid companyId, CancellationToken cancellationToken = default)
        => roles.Query().Where(x => x.CompanyId == companyId && x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<User> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username)) throw new ArgumentException("O utilizador é obrigatório.");
        if (command.Password.Length < 4) throw new ArgumentException("A senha deve ter pelo menos 4 caracteres.");
        var username = command.Username.Trim();
        if (await users.Query().AnyAsync(x => x.CompanyId == command.CompanyId && x.Username == username, cancellationToken))
            throw new InvalidOperationException("Este utilizador já existe.");
        if (!await roles.Query().AnyAsync(x => x.Id == command.RoleId && x.CompanyId == command.CompanyId && x.IsActive, cancellationToken))
            throw new InvalidOperationException("Perfil de acesso inválido.");

        var user = new User
        {
            CompanyId = command.CompanyId,
            Username = username,
            DisplayName = command.DisplayName.Trim(),
            PasswordHash = AuthenticationService.HashPassword(command.Password),
            IsActive = true
        };
        await users.AddAsync(user, cancellationToken);
        await userRoles.AddAsync(new CoreUserRole { UserId = user.Id, RoleId = command.RoleId }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task SetActiveAsync(Guid companyId, Guid userId, bool active, CancellationToken cancellationToken = default)
    {
        var user = await users.Query().FirstOrDefaultAsync(x => x.Id == userId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Utilizador não encontrado.");
        user.IsActive = active;
        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

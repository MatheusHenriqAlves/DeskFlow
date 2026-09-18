using DeskFlow.Application.Abstractions;
using DeskFlow.Application.DTOs.Users;
using DeskFlow.Domain.Models.Enums;


namespace DeskFlow.Application.Services;

public class UserService(IUserRepository users, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync() => users.GetAllAsync();

    public async Task<UserResponse?> UpdateRoleAsync(Guid id, UserRole role, Guid actorId)
    {
        if (!Enum.IsDefined(role)) throw new ArgumentException("Perfil inválido.");

        // Impede que um administrador remova a própria permissão de admin.
        // Sem essa checagem, um clique errado deixaria o sistema sem nenhum
        // administrador ativo, sem forma de reverter pela própria aplicação.
        if (id == actorId && role != UserRole.Admin)
            throw new InvalidOperationException("Você não pode remover sua própria permissão de administrador.");

        var user = await users.FindAsync(id);
        if (user is null) return null;
        user.Role = role;
        await unitOfWork.SaveChangesAsync();
        return new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt);
    }

    public async Task<UserResponse?> SetActiveAsync(Guid id, bool isActive, Guid actorId)
    {
        // Mesmo raciocínio: um administrador não pode se autodesativar e ficar
        // trancado para fora do próprio sistema.
        if (id == actorId && !isActive)
            throw new InvalidOperationException("Você não pode desativar a sua própria conta.");

        var user = await users.FindAsync(id);
        if (user is null) return null;
        user.IsActive = isActive;
        await unitOfWork.SaveChangesAsync();
        return new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt);
    }
}

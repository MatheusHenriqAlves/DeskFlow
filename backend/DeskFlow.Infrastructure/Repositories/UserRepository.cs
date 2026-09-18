using DeskFlow.Application.Abstractions;
using DeskFlow.Application.DTOs.Users;
using DeskFlow.Domain.Models;
using DeskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Repositories;

public class UserRepository(DeskFlowDbContext db) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email) => db.Users.AnyAsync(x => x.Email == email);
    public Task<User?> FindByEmailAsync(string email) => db.Users.FirstOrDefaultAsync(x => x.Email == email);
    public async Task<User?> FindAsync(Guid id) => await db.Users.FindAsync(id);
    public void Add(User user) => db.Users.Add(user);
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync() =>
        await db.Users.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new UserResponse(x.Id, x.Name, x.Email, x.Role, x.IsActive, x.CreatedAt))
            .ToListAsync();
}
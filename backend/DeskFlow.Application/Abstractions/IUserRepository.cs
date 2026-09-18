using DeskFlow.Domain.Models;
using DeskFlow.Application.DTOs.Users;

namespace DeskFlow.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindAsync(Guid id);
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    void Add(User user);
}
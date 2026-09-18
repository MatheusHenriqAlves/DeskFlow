using DeskFlow.Application.Abstractions;
using DeskFlow.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace DeskFlow.Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> hasher = new();
    public string Hash(User user, string password) => hasher.HashPassword(user, password);
    public bool Verify(User user, string password) =>
        hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
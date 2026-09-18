using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<DeskFlowDbContext>();
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();
        var users = new[]
        {
            Create("Admin DeskFlow", "admin@deskflow.local", UserRole.Admin, configuration["Seed:AdminPassword"] ?? "Admin@123!", hasher),
            Create("Técnico Demo", "tech@deskflow.local", UserRole.Technician, configuration["Seed:TechnicianPassword"] ?? "Tech@123!", hasher),
            Create("Usuário Demo", "user@deskflow.local", UserRole.User, configuration["Seed:UserPassword"] ?? "User@123!", hasher)
        };
        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }

    private static User Create(string name, string email, UserRole role, string password, PasswordHasher<User> hasher)
    {
        var user = new User { Name = name, Email = email, Role = role, IsActive = true };
        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}

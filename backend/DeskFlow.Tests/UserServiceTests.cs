using DeskFlow.Application.DTOs.Auth;
using DeskFlow.Application.Services;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Infrastructure.Data;
using DeskFlow.Infrastructure.Repositories;
using DeskFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DeskFlow.Tests;

public class UserServiceTests
{
    private static DeskFlowDbContext CreateDb() => new(
        new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Admin_CannotRemoveOwnAdminRole()
    {
        await using var db = CreateDb();
        var admin = new User { Name = "Admin", Email = "admin@test.local", PasswordHash = "hash", Role = UserRole.Admin };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var service = new UserService(new UserRepository(db), db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateRoleAsync(admin.Id, UserRole.User, actorId: admin.Id));

        // Garante que nada foi alterado no banco.
        var reloaded = await db.Users.FindAsync(admin.Id);
        Assert.Equal(UserRole.Admin, reloaded!.Role);
    }

    [Fact]
    public async Task Admin_CanPromoteAnotherUser()
    {
        await using var db = CreateDb();
        var admin = new User { Name = "Admin", Email = "admin@test.local", PasswordHash = "hash", Role = UserRole.Admin };
        var other = new User { Name = "Other", Email = "other@test.local", PasswordHash = "hash" };
        db.Users.AddRange(admin, other);
        await db.SaveChangesAsync();

        var service = new UserService(new UserRepository(db), db);
        var updated = await service.UpdateRoleAsync(other.Id, UserRole.Technician, actorId: admin.Id);

        Assert.NotNull(updated);
        Assert.Equal(UserRole.Technician, updated!.Role);
    }

    [Fact]
    public async Task Admin_CannotDeactivateOwnAccount()
    {
        await using var db = CreateDb();
        var admin = new User { Name = "Admin", Email = "admin@test.local", PasswordHash = "hash", Role = UserRole.Admin };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var service = new UserService(new UserRepository(db), db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetActiveAsync(admin.Id, isActive: false, actorId: admin.Id));

        var reloaded = await db.Users.FindAsync(admin.Id);
        Assert.True(reloaded!.IsActive);
    }

    [Fact]
    public async Task Admin_CanDeactivateAnotherUser()
    {
        await using var db = CreateDb();
        var admin = new User { Name = "Admin", Email = "admin@test.local", PasswordHash = "hash", Role = UserRole.Admin };
        var other = new User { Name = "Other", Email = "other@test.local", PasswordHash = "hash" };
        db.Users.AddRange(admin, other);
        await db.SaveChangesAsync();

        var service = new UserService(new UserRepository(db), db);
        var updated = await service.SetActiveAsync(other.Id, isActive: false, actorId: admin.Id);

        Assert.NotNull(updated);
        Assert.False(updated!.IsActive);
    }
}

public class AuthServiceErrorCasesTests
{
    private static DeskFlowDbContext CreateDb() => new(
        new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static AuthService CreateService(DeskFlowDbContext db)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-only-signing-key-with-at-least-32-characters"
        }).Build();
        return new AuthService(new UserRepository(db), db, new JwtTokenService(configuration), new PasswordService());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsError()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var first = await service.RegisterAsync(new RegisterRequest
        {
            Name = "Primeiro", Email = "duplicado@test.local", Password = "Senha@123"
        });
        Assert.NotNull(first.Response);

        var second = await service.RegisterAsync(new RegisterRequest
        {
            Name = "Segundo", Email = "DUPLICADO@test.local", Password = "OutraSenha@123"
        });

        Assert.Null(second.Response);
        Assert.NotNull(second.Error);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsError()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        await service.RegisterAsync(new RegisterRequest
        {
            Name = "Usuário", Email = "usuario@test.local", Password = "SenhaCorreta@123"
        });

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "usuario@test.local", Password = "SenhaErrada@123"
        });

        Assert.Null(result.Response);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsErrorWithoutRevealingWhetherUserExists()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "naoexiste@test.local", Password = "QualquerSenha@123"
        });

        Assert.Null(result.Response);
        Assert.Equal("E-mail ou senha inválidos.", result.Error);
    }
}

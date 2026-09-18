using DeskFlow.Application.DTOs.Auth;
using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Application.Services;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Domain.Services;
using DeskFlow.Infrastructure.Data;
using DeskFlow.Infrastructure.Repositories;
using DeskFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DeskFlow.Tests;

public class ArchitectureRegressionTests
{
    private static DeskFlowDbContext CreateDb() => new(
        new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public void InnerLayersDoNotReferenceInfrastructureOrWebFrameworks()
    {
        foreach (var assembly in new[] { typeof(Ticket).Assembly, typeof(TicketService).Assembly })
        {
            Assert.DoesNotContain(assembly.GetReferencedAssemblies(), reference =>
                reference.Name == "DeskFlow.Api" || reference.Name == "DeskFlow.Infrastructure" ||
                reference.Name!.StartsWith("Microsoft.EntityFrameworkCore") ||
                reference.Name.StartsWith("Microsoft.AspNetCore"));
        }
        Assert.DoesNotContain(typeof(Ticket).Assembly.GetReferencedAssemblies(),
            reference => reference.Name == "DeskFlow.Application");
    }

    [Fact]
    public void ExistingMigrationIsDiscoveredInInfrastructure()
    {
        using var db = new DeskFlowDbContext(new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused").Options);
        Assert.Equal(new[] { "202608210001_InitialCreate" }, db.Database.GetMigrations());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task PasswordsAndLoginRemainCompatibleWithExistingHashes()
    {
        await using var db = CreateDb();
        var passwords = new PasswordService();
        var user = new User { Name = "Existing", Email = "existing@test.local" };
        user.PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
            .HashPassword(user, "Test-only-password123!");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-only-signing-key-with-at-least-32-characters"
        }).Build();
        var service = new AuthService(new UserRepository(db), db, new JwtTokenService(configuration), passwords);
        var login = await service.LoginAsync(new LoginRequest { Email = user.Email, Password = "Test-only-password123!" });
        Assert.NotNull(login.Response);
        Assert.Equal(user.Id, login.Response.User.Id);
        var wrongPassword = await service.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong" });
        Assert.Null(wrongPassword.Response);
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Null((await service.LoginAsync(new LoginRequest { Email = user.Email, Password = "Test-only-password123!" })).Response);
    }

    [Fact]
    public async Task WorkflowPreservesVisibilityCommentsHistoryAndDeletion()
    {
        await using var db = CreateDb();
        var owner = new User { Name = "Owner", Email = "owner@test.local", PasswordHash = "hash" };
        var other = new User { Name = "Other", Email = "other@test.local", PasswordHash = "hash" };
        var technician = new User { Name = "Tech", Email = "tech@test.local", PasswordHash = "hash", Role = UserRole.Technician };
        db.Users.AddRange(owner, other, technician);
        await db.SaveChangesAsync();
        var service = new TicketService(new TicketRepository(db), new UserRepository(db), db, new SmartPriorityService());
        var ticket = await service.CreateAsync(new CreateTicketRequest
        {
            Title = "Acesso bloqueado", Description = "Sem acesso ao sistema corporativo", Category = TicketCategory.Access
        }, owner.Id);
        Assert.Empty((await service.GetAsync(new TicketQueryParameters(), other.Id, UserRole.User)).Items);
        Assert.Null(await service.AddCommentAsync(ticket.Id, "Not allowed", other.Id, UserRole.User));
        Assert.Null(await service.GetHistoryAsync(ticket.Id, other.Id, UserRole.User));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStatusAsync(ticket.Id, TicketStatus.Resolved, technician.Id, UserRole.Technician));
        var assigned = await service.AssignToMeAsync(ticket.Id, technician.Id, UserRole.Technician);
        Assert.Equal(TicketStatus.InProgress, assigned!.Status);
        Assert.Equal(technician.Id, assigned.AssignedToUserId);
        Assert.NotNull(await service.AddCommentAsync(ticket.Id, "Working on it", technician.Id, UserRole.Technician));
        await service.ChangeCategoryAsync(ticket.Id, TicketCategory.Software, technician.Id, UserRole.Technician);
        var resolved = await service.ChangeStatusAsync(ticket.Id, TicketStatus.Resolved, technician.Id, UserRole.Technician);
        Assert.NotNull(resolved!.ResolvedAt);
        Assert.Equal(5, (await service.GetHistoryAsync(ticket.Id, owner.Id, UserRole.User))!.Count);

        // Defesa em profundidade: um usuário comum nunca deve conseguir mudar status,
        // categoria, atribuir a si mesmo ou excluir um chamado, mesmo chamando o
        // serviço diretamente (sem passar pelo atributo [Authorize] do controller).
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ChangeStatusAsync(ticket.Id, TicketStatus.Closed, owner.Id, UserRole.User));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AssignToMeAsync(ticket.Id, owner.Id, UserRole.User));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ChangeCategoryAsync(ticket.Id, TicketCategory.Other, owner.Id, UserRole.User));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteAsync(ticket.Id, UserRole.Technician));

        Assert.True(await service.DeleteAsync(ticket.Id, UserRole.Admin));
        Assert.Null(await service.GetByIdAsync(ticket.Id, owner.Id, UserRole.User));
        Assert.False(await service.DeleteAsync(ticket.Id, UserRole.Admin));
    }
}

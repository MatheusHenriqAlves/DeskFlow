using DeskFlow.Domain.Services;
using DeskFlow.Infrastructure.Repositories;
using Xunit;
using DeskFlow.Infrastructure.Data;
using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsTicketAndHistory()
    {
        var options = new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new DeskFlowDbContext(options);
        var user = new User { Name = "Test", Email = "test@test.local", PasswordHash = "hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new TicketService(new TicketRepository(db), new UserRepository(db), db, new SmartPriorityService());
        var ticket = await service.CreateAsync(new CreateTicketRequest
        {
            Title = "Notebook não inicia",
            Description = "O notebook do usuário não inicia corretamente.",
            Category = TicketCategory.Hardware
        }, user.Id);

        Assert.True(ticket.Id > 0);
        Assert.Single(await db.Tickets.ToListAsync());
        Assert.Single(await db.TicketHistories.ToListAsync());
    }

    [Fact]
    public async Task UserCannotReadAnotherUsersTicket()
    {
        var options = new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new DeskFlowDbContext(options);
        var owner = new User { Name = "Owner", Email = "owner@test.local", PasswordHash = "hash" };
        var other = new User { Name = "Other", Email = "other@test.local", PasswordHash = "hash" };
        db.Users.AddRange(owner, other);
        await db.SaveChangesAsync();
        var service = new TicketService(new TicketRepository(db), new UserRepository(db), db, new SmartPriorityService());
        var created = await service.CreateAsync(new CreateTicketRequest
        {
            Title = "Problema de acesso",
            Description = "Não consigo acessar o sistema corporativo.",
            Category = TicketCategory.Access
        }, owner.Id);

        var result = await service.GetByIdAsync(created.Id, other.Id, UserRole.User);
        Assert.Null(result);
    }
}

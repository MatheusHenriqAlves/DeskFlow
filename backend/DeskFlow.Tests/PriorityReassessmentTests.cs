using System.Reflection;
using System.Security.Claims;
using DeskFlow.Api.Controllers;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DeskFlow.Tests;

public class PriorityReassessmentTests
{
    [Fact]
    public async Task ActiveTicketsAreReassessedOnceWithHistoryAndCompletedTicketsArePreserved()
    {
        await using var db = new DeskFlowDbContext(new DbContextOptionsBuilder<DeskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var tickets = Enum.GetValues<TicketStatus>().Select(status => new Ticket
        {
            Title = "Sistema de segurança caiu",
            Description = "Estamos sofrendo ataque hacker",
            Status = status,
            Priority = TicketPriority.Medium,
            PriorityScore = 5,
            PriorityReason = "Regra antiga"
        }).ToArray();
        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        await PriorityReassessment.RunAsync(db);
        foreach (var ticket in tickets)
        {
            var active = ticket.Status is TicketStatus.Open or TicketStatus.InProgress;
            Assert.Equal(active ? TicketPriority.Critical : TicketPriority.Medium, ticket.Priority);
            Assert.Equal(active ? 1 : 0, await db.TicketHistories.CountAsync(h => h.TicketId == ticket.Id));
        }
        var updatedTimes = tickets.Select(t => t.UpdatedAt).ToArray();
        await PriorityReassessment.RunAsync(db);
        Assert.Equal(2, await db.TicketHistories.CountAsync());
        Assert.Equal(updatedTimes, tickets.Select(t => t.UpdatedAt).ToArray());
        Assert.All(await db.TicketHistories.ToListAsync(), h =>
        {
            Assert.Equal("PriorityReassessed", h.Action);
            Assert.Null(h.UserId);
            Assert.Contains("Medium (score 5)", h.Description);
            Assert.Contains("Critical", h.Description);
        });
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Technician", false)]
    [InlineData("User", false)]
    [InlineData(null, false)]
    public async Task DeleteEndpointPolicyAllowsOnlyAuthenticatedAdmin(string? role, bool allowed)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        using var provider = services.BuildServiceProvider();
        var method = typeof(TicketsController).GetMethod(nameof(TicketsController.Delete))!;
        var metadata = typeof(TicketsController).GetCustomAttributes<AuthorizeAttribute>()
            .Concat(method.GetCustomAttributes<AuthorizeAttribute>());
        var policy = await AuthorizationPolicy.CombineAsync(provider.GetRequiredService<IAuthorizationPolicyProvider>(), metadata);
        var identity = role is null ? new ClaimsIdentity() :
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "Test");
        var result = await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(new ClaimsPrincipal(identity), null, policy!);
        Assert.Equal(allowed, result.Succeeded);
    }
}

using DeskFlow.Application.Abstractions;
using DeskFlow.Infrastructure.Data;
using DeskFlow.Application.DTOs.Metrics;
using DeskFlow.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Repositories;

public class MetricsReader(DeskFlowDbContext db) : IMetricsReader
{
    public async Task<MetricsResponse> GetAsync()
    {
        var totalTickets = await db.Tickets.CountAsync();
        var totalUsers = await db.Users.CountAsync();
        var activeTechnicians = await db.Users.CountAsync(x => x.IsActive && x.Role == UserRole.Technician);
        var statusCounts = await db.Tickets.GroupBy(x => x.Status).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
        var priority = await db.Tickets.GroupBy(x => x.Priority).Select(g => new MetricSlice(g.Key.ToString(), g.Count())).ToListAsync();
        var category = await db.Tickets.GroupBy(x => x.Category).Select(g => new MetricSlice(g.Key.ToString(), g.Count())).ToListAsync();

        int Count(TicketStatus status) => statusCounts.FirstOrDefault(x => x.Key == status)?.Count ?? 0;
        return new MetricsResponse(totalTickets, Count(TicketStatus.Open), Count(TicketStatus.InProgress),
            Count(TicketStatus.Resolved), Count(TicketStatus.Closed), totalUsers, activeTechnicians, priority, category);
    }
}

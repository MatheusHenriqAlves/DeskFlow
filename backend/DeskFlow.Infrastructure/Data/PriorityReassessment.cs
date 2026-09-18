using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Data;

public static class PriorityReassessment
{
    // Persist changes and audit together. An unchanged assessment on subsequent
    // startups does not create another history entry or alter the update timestamp.
    public static async Task RunAsync(DeskFlowDbContext db, CancellationToken cancellationToken = default)
    {
        var calculator = new SmartPriorityService();
        var activeTickets = await db.Tickets
            .Where(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)
            .ToListAsync(cancellationToken);
        foreach (var ticket in activeTickets)
        {
            var assessment = calculator.Calculate(ticket.Title, ticket.Description);
            var reason = string.Join("; ", assessment.Reasons);
            if (ticket.Priority == assessment.Priority && ticket.PriorityScore == assessment.Score && ticket.PriorityReason == reason)
                continue;
            db.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                Action = "PriorityReassessed",
                Description = $"Criticidade recalculada pela revisão do Smart Priority: {ticket.Priority} (score {ticket.PriorityScore}) → {assessment.Priority} (score {assessment.Score})."
            });
            ticket.Priority = assessment.Priority;
            ticket.PriorityScore = assessment.Score;
            ticket.PriorityReason = reason;
            ticket.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}

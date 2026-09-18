using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.DTOs.Tickets;

public class TicketQueryParameters
{
    public string? Search { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public TicketCategory? Category { get; set; }
    public bool? Assigned { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

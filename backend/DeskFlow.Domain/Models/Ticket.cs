using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Domain.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public int PriorityScore { get; set; }
    public string PriorityReason { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketCategory Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}

namespace DeskFlow.Domain.Models;

public class TicketHistory
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

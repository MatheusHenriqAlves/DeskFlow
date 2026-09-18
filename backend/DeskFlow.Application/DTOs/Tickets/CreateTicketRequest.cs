using System.ComponentModel.DataAnnotations;
using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.DTOs.Tickets;

public class CreateTicketRequest
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    public TicketCategory Category { get; set; } = TicketCategory.Other;
}

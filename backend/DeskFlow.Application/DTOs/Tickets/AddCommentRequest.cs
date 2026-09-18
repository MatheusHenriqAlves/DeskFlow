using System.ComponentModel.DataAnnotations;

namespace DeskFlow.Application.DTOs.Tickets;

public class AddCommentRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}

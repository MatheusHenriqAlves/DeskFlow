using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.DTOs.Tickets;

public record TicketListItemResponse(
    int Id,
    string Title,
    TicketPriority Priority,
    int PriorityScore,
    TicketStatus Status,
    TicketCategory Category,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    string? AssignedTo);

public record CommentResponse(int Id, string Content, DateTime CreatedAt, Guid UserId, string UserName, UserRole UserRole);

public record HistoryResponse(int Id, string Action, string Description, DateTime CreatedAt, Guid? UserId, string? UserName);

public record TicketDetailsResponse(
    int Id,
    string Title,
    string Description,
    TicketPriority Priority,
    int PriorityScore,
    string PriorityReason,
    TicketStatus Status,
    TicketCategory Category,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    Guid CreatedByUserId,
    string CreatedBy,
    Guid? AssignedToUserId,
    string? AssignedTo,
    IReadOnlyList<CommentResponse> Comments,
    IReadOnlyList<HistoryResponse> History);

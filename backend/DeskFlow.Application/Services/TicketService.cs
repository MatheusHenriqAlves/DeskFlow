using DeskFlow.Application.Abstractions;
using DeskFlow.Domain.Services;
using DeskFlow.Application.DTOs.Common;
using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.Services;

public class TicketService(ITicketRepository tickets, IUserRepository users, IUnitOfWork unitOfWork, SmartPriorityService priorityService)
{
    public Task<PagedResponse<TicketListItemResponse>> GetAsync(
        TicketQueryParameters parameters, Guid actorId, UserRole actorRole) =>
        tickets.SearchAsync(parameters, actorRole == UserRole.User ? actorId : null);

    public async Task<TicketDetailsResponse?> GetByIdAsync(int id, Guid actorId, UserRole actorRole)
    {
        var ticket = await tickets.GetDetailsAsync(id);
        if (ticket is null || !CanAccess(ticket, actorId, actorRole)) return null;
        return MapDetails(ticket);
    }

    public async Task<TicketDetailsResponse> CreateAsync(CreateTicketRequest request, Guid userId)
    {
        if (!Enum.IsDefined(request.Category))
            throw new ArgumentException("Categoria inválida.");

        var assessment = priorityService.Calculate(request.Title, request.Description);
        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category,
            Priority = assessment.Priority,
            PriorityScore = assessment.Score,
            PriorityReason = string.Join("; ", assessment.Reasons),
            Status = TicketStatus.Open,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tickets.Add(ticket);
        tickets.AddHistory(new TicketHistory
        {
            Ticket = ticket,
            UserId = userId,
            Action = "Created",
            Description = $"Chamado criado com prioridade {assessment.Priority} (score {assessment.Score})."
        });
        await unitOfWork.SaveChangesAsync();
        return (await GetByIdAsync(ticket.Id, userId, UserRole.User))!;
    }

    public async Task<TicketDetailsResponse?> AssignToMeAsync(int id, Guid technicianId, UserRole actorRole)
    {
        EnsureStaff(actorRole);
        var ticket = await tickets.FindAsync(id);
        if (ticket is null) return null;
        ticket.AssignedToUserId = technicianId;
        if (ticket.Status == TicketStatus.Open) ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;
        AddHistory(ticket.Id, technicianId, "Assigned", "Chamado assumido por um técnico.");
        await unitOfWork.SaveChangesAsync();
        return await GetForStaffAsync(id);
    }

    public async Task<TicketDetailsResponse?> ChangeStatusAsync(int id, TicketStatus status, Guid actorId, UserRole actorRole)
    {
        EnsureStaff(actorRole);
        if (!Enum.IsDefined(status)) throw new ArgumentException("Status inválido.");
        var ticket = await tickets.FindAsync(id);
        if (ticket is null) return null;
        var old = ticket.Status;
        ValidateStatusTransition(old, status);
        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.ResolvedAt = status is TicketStatus.Resolved or TicketStatus.Closed ? DateTime.UtcNow : null;
        AddHistory(id, actorId, "StatusChanged", $"Status alterado de {old} para {status}.");
        await unitOfWork.SaveChangesAsync();
        return await GetForStaffAsync(id);
    }

    public async Task<TicketDetailsResponse?> ChangeCategoryAsync(int id, TicketCategory category, Guid actorId, UserRole actorRole)
    {
        EnsureStaff(actorRole);
        if (!Enum.IsDefined(category)) throw new ArgumentException("Categoria inválida.");
        var ticket = await tickets.FindAsync(id);
        if (ticket is null) return null;
        var old = ticket.Category;
        ticket.Category = category;
        ticket.UpdatedAt = DateTime.UtcNow;
        AddHistory(id, actorId, "CategoryChanged", $"Categoria alterada de {old} para {category}.");
        await unitOfWork.SaveChangesAsync();
        return await GetForStaffAsync(id);
    }

    public async Task<CommentResponse?> AddCommentAsync(int id, string content, Guid actorId, UserRole actorRole)
    {
        var ticket = await tickets.FindAsync(id);
        if (ticket is null || !CanAccess(ticket, actorId, actorRole)) return null;

        var comment = new TicketComment
        {
            TicketId = id,
            UserId = actorId,
            Content = content.Trim()
        };
        tickets.AddComment(comment);
        AddHistory(id, actorId, "Commented", "Comentário adicionado ao chamado.");
        await unitOfWork.SaveChangesAsync();

        var user = (await users.FindAsync(actorId))!;
        return new CommentResponse(comment.Id, comment.Content, comment.CreatedAt, user.Id, user.Name, user.Role);
    }

    public async Task<IReadOnlyList<HistoryResponse>?> GetHistoryAsync(int id, Guid actorId, UserRole actorRole)
    {
        var ticket = await tickets.FindAsync(id);
        if (ticket is null || !CanAccess(ticket, actorId, actorRole)) return null;
        return await tickets.GetHistoryAsync(id);
    }

    public async Task<bool> DeleteAsync(int id, UserRole actorRole)
    {
        if (actorRole != UserRole.Admin)
            throw new UnauthorizedAccessException("Apenas administradores podem excluir chamados.");
        var ticket = await tickets.FindAsync(id);
        if (ticket is null) return false;
        tickets.Remove(ticket);
        await unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task<TicketDetailsResponse?> GetForStaffAsync(int id)
    {
        var ticket = await tickets.GetDetailsAsync(id);
        return ticket is null ? null : MapDetails(ticket);
    }

    private static bool CanAccess(Ticket ticket, Guid actorId, UserRole role) =>
        role != UserRole.User || ticket.CreatedByUserId == actorId;

    /// <summary>
    /// Segunda camada de proteção (defesa em profundidade): mesmo que uma rota fique
    /// desprotegida por engano no controller (ex.: atributo [Authorize(Roles=...)]
    /// esquecido), o serviço nunca permite que um usuário comum execute ações de
    /// equipe técnica/administrativa.
    /// </summary>
    private static void EnsureStaff(UserRole role)
    {
        if (role is not (UserRole.Technician or UserRole.Admin))
            throw new UnauthorizedAccessException("Apenas técnicos ou administradores podem executar esta ação.");
    }

    private void AddHistory(int ticketId, Guid? userId, string action, string description) =>
        tickets.AddHistory(new TicketHistory
        {
            TicketId = ticketId,
            UserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

    private static void ValidateStatusTransition(TicketStatus oldStatus, TicketStatus newStatus)
    {
        if (oldStatus == newStatus) return;
        var allowed = oldStatus switch
        {
            TicketStatus.Open => newStatus is TicketStatus.InProgress or TicketStatus.Closed,
            TicketStatus.InProgress => newStatus is TicketStatus.Resolved or TicketStatus.Open,
            TicketStatus.Resolved => newStatus is TicketStatus.Closed or TicketStatus.InProgress,
            TicketStatus.Closed => false,
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException($"Transição de status inválida: {oldStatus} -> {newStatus}.");
    }

    private static TicketDetailsResponse MapDetails(Ticket x) => new(
        x.Id, x.Title, x.Description, x.Priority, x.PriorityScore, x.PriorityReason,
        x.Status, x.Category, x.CreatedAt, x.UpdatedAt, x.ResolvedAt,
        x.CreatedByUserId, x.CreatedByUser.Name, x.AssignedToUserId, x.AssignedToUser?.Name,
        x.Comments.OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse(c.Id, c.Content, c.CreatedAt, c.UserId, c.User.Name, c.User.Role)).ToList(),
        x.History.OrderByDescending(h => h.CreatedAt)
            .Select(h => new HistoryResponse(h.Id, h.Action, h.Description, h.CreatedAt, h.UserId, h.User?.Name)).ToList());
}

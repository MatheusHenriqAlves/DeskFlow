using DeskFlow.Application.Abstractions;
using DeskFlow.Application.DTOs.Common;
using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Infrastructure.Repositories;

public class TicketRepository(DeskFlowDbContext db) : ITicketRepository
{
    public async Task<PagedResponse<TicketListItemResponse>> SearchAsync(
        TicketQueryParameters parameters,
        Guid? ownerId)
    {
        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        IQueryable<Ticket> query = db.Tickets.AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Include(x => x.AssignedToUser);

        if (ownerId.HasValue)
            query = query.Where(x => x.CreatedByUserId == ownerId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            // EF.Functions.ILike é traduzido para o operador nativo ILIKE do Postgres:
            // busca "case-insensitive" feita pelo próprio banco (não em memória) e
            // compatível com índices trigram (pg_trgm), diferente de ToLower()+Contains(),
            // que força a avaliação de ToLower() linha a linha.
            var search = $"%{parameters.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, search) || EF.Functions.ILike(x.Description, search));
        }

        if (parameters.Status.HasValue)
            query = query.Where(x => x.Status == parameters.Status.Value);
        if (parameters.Priority.HasValue)
            query = query.Where(x => x.Priority == parameters.Priority.Value);
        if (parameters.Category.HasValue)
            query = query.Where(x => x.Category == parameters.Category.Value);
        if (parameters.Assigned.HasValue)
            query = parameters.Assigned.Value
                ? query.Where(x => x.AssignedToUserId != null)
                : query.Where(x => x.AssignedToUserId == null);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TicketListItemResponse(
                x.Id, x.Title, x.Priority, x.PriorityScore, x.Status, x.Category,
                x.CreatedAt, x.UpdatedAt, x.CreatedByUser.Name, x.AssignedToUser != null ? x.AssignedToUser.Name : null))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResponse<TicketListItemResponse>(items, page, pageSize, total, totalPages);
    }

    private IQueryable<Ticket> DetailsQuery() => db.Tickets.AsNoTracking()
        .Include(x => x.CreatedByUser)
        .Include(x => x.AssignedToUser)
        .Include(x => x.Comments).ThenInclude(x => x.User)
        .Include(x => x.History).ThenInclude(x => x.User);

    public async Task<Ticket?> FindAsync(int id) => await db.Tickets.FindAsync(id);
    public Task<Ticket?> GetDetailsAsync(int id) => DetailsQuery().FirstOrDefaultAsync(x => x.Id == id);
    public void Add(Ticket ticket) => db.Tickets.Add(ticket);
    public void AddComment(TicketComment comment) => db.TicketComments.Add(comment);
    public void AddHistory(TicketHistory history) => db.TicketHistories.Add(history);
    public void Remove(Ticket ticket) => db.Tickets.Remove(ticket);
    public async Task<IReadOnlyList<HistoryResponse>> GetHistoryAsync(int id)
    {
        return await db.TicketHistories.AsNoTracking()
            .Where(x => x.TicketId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new HistoryResponse(x.Id, x.Action, x.Description, x.CreatedAt, x.UserId, x.User != null ? x.User.Name : null))
            .ToListAsync();
    }

}

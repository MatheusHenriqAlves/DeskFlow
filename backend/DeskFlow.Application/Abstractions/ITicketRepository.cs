using DeskFlow.Application.DTOs.Common;
using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Domain.Models;
using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.Abstractions;

public interface ITicketRepository
{
    Task<PagedResponse<TicketListItemResponse>> SearchAsync(TicketQueryParameters parameters, Guid? ownerId);
    Task<Ticket?> FindAsync(int id);
    Task<Ticket?> GetDetailsAsync(int id);
    Task<IReadOnlyList<HistoryResponse>> GetHistoryAsync(int id);
    void Add(Ticket ticket);
    void AddComment(TicketComment comment);
    void AddHistory(TicketHistory history);
    void Remove(Ticket ticket);
}
namespace DeskFlow.Application.DTOs.Metrics;

public record MetricSlice(string Name, int Count);
public record MetricsResponse(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int TotalUsers,
    int ActiveTechnicians,
    IReadOnlyList<MetricSlice> ByPriority,
    IReadOnlyList<MetricSlice> ByCategory);

using DeskFlow.Application.Abstractions;
using DeskFlow.Application.DTOs.Metrics;

namespace DeskFlow.Application.Services;

public class MetricsService(IMetricsReader metrics)
{
    public Task<MetricsResponse> GetAsync() => metrics.GetAsync();
}
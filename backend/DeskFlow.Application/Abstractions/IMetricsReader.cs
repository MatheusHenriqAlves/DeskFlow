using DeskFlow.Application.DTOs.Metrics;

namespace DeskFlow.Application.Abstractions;

public interface IMetricsReader
{
    Task<MetricsResponse> GetAsync();
}
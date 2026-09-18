namespace DeskFlow.Application.DTOs.Common;

public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems, int TotalPages);

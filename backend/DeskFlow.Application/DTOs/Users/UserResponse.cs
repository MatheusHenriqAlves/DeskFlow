using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.DTOs.Users;

public record UserResponse(Guid Id, string Name, string Email, UserRole Role, bool IsActive, DateTime CreatedAt);

using DeskFlow.Application.DTOs.Users;

namespace DeskFlow.Application.DTOs.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, UserResponse User);

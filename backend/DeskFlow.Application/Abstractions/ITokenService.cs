using DeskFlow.Domain.Models;

namespace DeskFlow.Application.Abstractions;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) Create(User user);
}
using DeskFlow.Domain.Models;

namespace DeskFlow.Application.Abstractions;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
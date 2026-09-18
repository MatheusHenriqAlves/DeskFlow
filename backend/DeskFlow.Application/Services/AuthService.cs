using DeskFlow.Application.Abstractions;
using DeskFlow.Application.DTOs.Auth;
using DeskFlow.Application.DTOs.Users;
using DeskFlow.Domain.Models;



namespace DeskFlow.Application.Services;

public class AuthService(IUserRepository users, IUnitOfWork unitOfWork, ITokenService jwt, IPasswordService passwords)
{


    public async Task<(AuthResponse? Response, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(email))
            return (null, "Já existe um usuário com este e-mail.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email
        };
        user.PasswordHash = passwords.Hash(user, request.Password);
        users.Add(user);
        await unitOfWork.SaveChangesAsync();
        return (BuildResponse(user), null);
    }

    public async Task<(AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return (null, "E-mail ou senha inválidos.");

        var verification = passwords.Verify(user, request.Password);
        if (!verification)
            return (null, "E-mail ou senha inválidos.");

        return (BuildResponse(user), null);
    }

    public async Task<UserResponse?> GetCurrentUserAsync(Guid id)
    {
        var user = await users.FindAsync(id);
        return user is null ? null : new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = jwt.Create(user);
        return new AuthResponse(token, expiresAt,
            new UserResponse(user.Id, user.Name, user.Email, user.Role, user.IsActive, user.CreatedAt));
    }
}

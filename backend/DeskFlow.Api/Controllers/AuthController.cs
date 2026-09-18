using DeskFlow.Application.DTOs.Auth;
using DeskFlow.Api.Extensions;
using DeskFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var (response, error) = await authService.RegisterAsync(request);
        return response is null ? Conflict(new { message = error }) : Created("/api/auth/me", response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var (response, error) = await authService.LoginAsync(request);
        return response is null ? Unauthorized(new { message = error }) : Ok(response);
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await authService.GetCurrentUserAsync(User.GetUserId());
        return user is null ? NotFound() : Ok(user);
    }

}

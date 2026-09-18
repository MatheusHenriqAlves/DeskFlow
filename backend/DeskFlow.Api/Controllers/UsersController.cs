using DeskFlow.Application.DTOs.Users;
using DeskFlow.Api.Extensions;
using DeskFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await userService.GetAllAsync());

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleRequest request)
    {
        var user = await userService.UpdateRoleAsync(id, request.Role, User.GetUserId());
        return user is null ? NotFound(new { message = "Usuário não encontrado." }) : Ok(user);
    }

    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, UpdateUserStatusRequest request)
    {
        var user = await userService.SetActiveAsync(id, request.IsActive, User.GetUserId());
        return user is null ? NotFound(new { message = "Usuário não encontrado." }) : Ok(user);
    }
}

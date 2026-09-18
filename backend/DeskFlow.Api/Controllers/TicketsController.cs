using DeskFlow.Application.DTOs.Tickets;
using DeskFlow.Api.Extensions;
using DeskFlow.Domain.Models.Enums;
using DeskFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController(TicketService ticketService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketQueryParameters parameters) =>
        Ok(await ticketService.GetAsync(parameters, User.GetUserId(), User.GetUserRole()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await ticketService.GetByIdAsync(id, User.GetUserId(), User.GetUserRole());
        return ticket is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        if (!Enum.IsDefined(request.Category)) return BadRequest(new { message = "Categoria inválida." });
        var ticket = await ticketService.CreateAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> Comment(int id, AddCommentRequest request)
    {
        var comment = await ticketService.AddCommentAsync(id, request.Content, User.GetUserId(), User.GetUserRole());
        return comment is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(comment);
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> History(int id)
    {
        var history = await ticketService.GetHistoryAsync(id, User.GetUserId(), User.GetUserRole());
        return history is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(history);
    }

    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = "Technician,Admin")]
    public async Task<IActionResult> Assign(int id)
    {
        var ticket = await ticketService.AssignToMeAsync(id, User.GetUserId(), User.GetUserRole());
        return ticket is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(ticket);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Technician,Admin")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeStatusRequest request)
    {
        if (!Enum.IsDefined(request.Status)) return BadRequest(new { message = "Status inválido." });
        var ticket = await ticketService.ChangeStatusAsync(id, request.Status, User.GetUserId(), User.GetUserRole());
        return ticket is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(ticket);
    }

    [HttpPut("{id:int}/category")]
    [Authorize(Roles = "Technician,Admin")]
    public async Task<IActionResult> ChangeCategory(int id, ChangeCategoryRequest request)
    {
        if (!Enum.IsDefined(request.Category)) return BadRequest(new { message = "Categoria inválida." });
        var ticket = await ticketService.ChangeCategoryAsync(id, request.Category, User.GetUserId(), User.GetUserRole());
        return ticket is null ? NotFound(new { message = "Chamado não encontrado." }) : Ok(ticket);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) =>
        await ticketService.DeleteAsync(id, User.GetUserRole()) ? NoContent() : NotFound(new { message = "Chamado não encontrado." });
}

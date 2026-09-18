using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Application.DTOs.Users;

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

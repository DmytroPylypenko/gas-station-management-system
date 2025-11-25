namespace GasStationSystem.Web.Models;

public class UserWithRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string?> AllRoles { get; set; } = new();
}
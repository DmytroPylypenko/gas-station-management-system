namespace GasStationSystem.Web.Models;

/// <summary>
/// ViewModel used in the Admin Panel for managing users and their roles.
/// </summary>
public class UserWithRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string?> AllRoles { get; set; } = new();
}
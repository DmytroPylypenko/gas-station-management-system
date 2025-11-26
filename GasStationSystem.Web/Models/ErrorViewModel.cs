namespace GasStationSystem.Web.Models;

/// <summary>
/// ViewModel used to display detailed error information to the user.
/// Contains the Request ID to help administrators trace the issue in logs.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
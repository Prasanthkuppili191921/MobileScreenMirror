
namespace MobileScreenMirror.Models;

public class AndroidDevice
{
    public string DeviceId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public bool IsConnected =>
        Status.Equals("device", StringComparison.OrdinalIgnoreCase);

    public override string ToString()
    {
        return $"{Model} ({DeviceId}) - {Status}";
    }
}

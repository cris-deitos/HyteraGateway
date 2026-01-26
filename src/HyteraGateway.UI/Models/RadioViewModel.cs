namespace HyteraGateway.UI.Models;

public class RadioViewModel
{
    public int DmrId { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Offline";
    public string SignalStrength { get; set; } = "---";
    public string LastActivity { get; set; } = "Never";
}

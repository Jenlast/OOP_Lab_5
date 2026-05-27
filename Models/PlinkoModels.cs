using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

public partial class PlinkoBall : ObservableObject
{
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
}

public class PlinkoPeg
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class PlinkoSlot
{
    public double X { get; set; }
    public double Y { get; set; }
    public string MultiplierText { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public string ColorHex { get; set; } = string.Empty;
}
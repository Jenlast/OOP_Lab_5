using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

// 1. Модель кульки, яка падає (кожна має власні координати на екрані)
public partial class PlinkoBall : ObservableObject
{
    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
}

// 2. Кілочок (цвяшок), від якого відбивається кулька
public class PlinkoPeg
{
    public double X { get; set; }
    public double Y { get; set; }
}

// 3. Слот внизу (множник виграшу)
public class PlinkoSlot
{
    public double X { get; set; }
    public double Y { get; set; }
    public string MultiplierText { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public string ColorHex { get; set; } = string.Empty;
}
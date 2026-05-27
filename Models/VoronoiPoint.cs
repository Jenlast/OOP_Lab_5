using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

public partial class VoronoiPoint : ObservableObject 
{ 
    public double X { get; set; } 
    public double Y { get; set; } 
    public Color RegionColor { get; set; }
    
    [ObservableProperty] private int _areaPixels; 
}
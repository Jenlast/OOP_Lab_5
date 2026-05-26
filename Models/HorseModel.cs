using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

public partial class HorseModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public Color HorseColor { get; set; }
    public double BaseSpeed { get; set; }
    public double Coefficient { get; set; }
    
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _viewTop;
    [ObservableProperty] private int _rank;
    [ObservableProperty] private TimeSpan _finishTime;
    [ObservableProperty] private bool _isFinished;

    public void Move(Random rnd)
    {
        if (IsFinished) return;
        double acceleration = 0.7 + (rnd.NextDouble() * 0.3);
        PositionX += BaseSpeed * acceleration;
    }
}
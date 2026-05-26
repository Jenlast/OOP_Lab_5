using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

public partial class HorseModel : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public Color HorseColor { get; set; }
    public double BaseSpeed { get; set; }
    public double Coefficient { get; set; }
    
    public Bitmap[] AnimationFrames { get; set; } = Array.Empty<Bitmap>();
    private int _currentFrameIndex = 0;
    
    [ObservableProperty] private Bitmap? _currentFrame; 
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _viewPositionX;
    [ObservableProperty] private double _viewTop;
    [ObservableProperty] private int _rank;
    [ObservableProperty] private TimeSpan _finishTime;
    [ObservableProperty] private bool _isFinished;

    // НОВА ВЛАСТИВІСТЬ: Для живого відображення часу в таблиці
    [ObservableProperty] private string _liveTimeDisplay = "0.000 сек";

    public void Move(Random rnd)
    {
        if (IsFinished) return;
        
        double acceleration = 0.7 + (rnd.NextDouble() * 0.3);
        PositionX += BaseSpeed * acceleration;

        if (AnimationFrames.Length > 0)
        {
            _currentFrameIndex = (_currentFrameIndex + 1) % AnimationFrames.Length;
            CurrentFrame = AnimationFrames[_currentFrameIndex];
        }
    }
}
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
    
    // Масив з 12 кадрами анімації
    public Bitmap[] AnimationFrames { get; set; } = Array.Empty<Bitmap>();
    private int _currentFrameIndex = 0;
    
    [ObservableProperty] private Bitmap? _currentFrame; // Поточна картинка для UI
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _viewTop;
    [ObservableProperty] private int _rank;
    [ObservableProperty] private TimeSpan _finishTime;
    [ObservableProperty] private bool _isFinished;

    public void Move(Random rnd)
    {
        if (IsFinished) return;
        
        // Рух
        double acceleration = 0.7 + (rnd.NextDouble() * 0.3);
        PositionX += BaseSpeed * acceleration;

        // Анімація: перемикаємо на наступний кадр (від 0 до 11)
        if (AnimationFrames.Length > 0)
        {
            _currentFrameIndex = (_currentFrameIndex + 1) % AnimationFrames.Length;
            CurrentFrame = AnimationFrames[_currentFrameIndex];
        }
    }
}
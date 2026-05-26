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
    [ObservableProperty] private double _viewTop;
    [ObservableProperty] private int _rank;
    [ObservableProperty] private TimeSpan _finishTime;
    [ObservableProperty] private bool _isFinished;

    // Зручна властивість для виведення в таблицю
    public string FormattedFinishTime 
    {
        get
        {
            if (!IsFinished) return "В процесі...";
            // Формат: Секунди.Мілісекунди (напр. 5.432 сек)
            return $"{(int)FinishTime.TotalSeconds}.{FinishTime.Milliseconds:D3} сек";
        }
    }

    // Тригер: коли змінюється час або статус фінішу, оновлюємо текст в таблиці
    partial void OnFinishTimeChanged(TimeSpan value) => OnPropertyChanged(nameof(FormattedFinishTime));
    partial void OnIsFinishedChanged(bool value) => OnPropertyChanged(nameof(FormattedFinishTime));

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
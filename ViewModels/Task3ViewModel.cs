using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using OOP_Lab5.Models;

namespace OOP_Lab5.ViewModels;

public partial class Task3ViewModel : ViewModelBase
{
    private readonly RouletteGame _game = new();

    // --- ВЛАСТИВОСТІ ДЛЯ UI ---

    private decimal _balance;
    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    private string _gameMessage = "Робіть ваші ставки!";
    public string GameMessage
    {
        get => _gameMessage;
        set => SetProperty(ref _gameMessage, value);
    }

    private bool _isSpinning;
    public bool IsSpinning
    {
        get => _isSpinning;
        set
        {
            SetProperty(ref _isSpinning, value);
            OnPropertyChanged(nameof(CanPlaceBet)); // Оновлюємо стан кнопок
        }
    }

    public bool CanPlaceBet => !IsSpinning;

    public decimal SelectedChip { get; set; } = 10m; // Стандартна ставка

    // --- ВЛАСТИВОСТІ ДЛЯ ФІЗИКИ ТА АНІМАЦІЇ ---

    private double _wheelAngle;
    public double WheelAngle
    {
        get => _wheelAngle;
        set => SetProperty(ref _wheelAngle, value);
    }

    private double _ballAngle;
    public double BallAngle
    {
        get => _ballAngle;
        set => SetProperty(ref _ballAngle, value);
    }

    private double _ballRadius = 140; // Відстань від центру (140 - зовнішнє коло, 90 - на числах)
    public double BallRadius
    {
        get => _ballRadius;
        set => SetProperty(ref _ballRadius, value);
    }

    public ObservableCollection<WheelSectorUI> UIWheelSectors { get; } = new();
    public System.Collections.Generic.IEnumerable<WheelSectorUI> SortedTableNumbers => UIWheelSectors.OrderBy(x => x.Number);

    private double _ballX = 150;
    public double BallX { get => _ballX; set => SetProperty(ref _ballX, value); }

    private double _ballY = 10;
    public double BallY { get => _ballY; set => SetProperty(ref _ballY, value); }

    // --- КОМАНДИ ---
    public ICommand SpinCommand { get; }
    public ICommand PlaceColorBetCommand { get; }
    public ICommand PlaceStraightBetCommand { get; }
    public ICommand ClearBetsCommand { get; }

    public Task3ViewModel()
    {
        Balance = _game.Player.Balance;

        SpinCommand = new RelayCommand(async _ => await SpinAsync(), _ => !IsSpinning);
        ClearBetsCommand = new RelayCommand(_ => ClearBets(), _ => !IsSpinning);
        
        PlaceColorBetCommand = new RelayCommand(PlaceColorBet, _ => !IsSpinning);
        PlaceStraightBetCommand = new RelayCommand(PlaceStraightBet, _ => !IsSpinning);
        double angleStep = 360.0 / 37.0;
        for (int i = 0; i < _game.Wheel.Sectors.Count; i++)
        {
            var sector = _game.Wheel.Sectors[i];
            string colorHex = sector.Color == RouletteColor.Green ? "#2E7D32" : 
                            sector.Color == RouletteColor.Red ? "#C62828" : "#151515";
            
            UIWheelSectors.Add(new WheelSectorUI 
            { 
                Number = sector.Value, 
                ColorHex = colorHex, 
                Angle = i * angleStep 
            });
        }
    }

    // --- ЛОГІКА СТАВОК ---

    private void PlaceColorBet(object? parameter)
    {
        if (parameter is string colorStr && Enum.TryParse(colorStr, true, out RouletteColor color))
        {
            if (_game.PlaceBet(new ColorBet(SelectedChip, color)))
            {
                Balance = _game.Player.Balance;
                GameMessage = $"Ставка {SelectedChip} на {color} прийнята.";
            }
            else GameMessage = "Недостатньо коштів!";
        }
    }

    private void PlaceStraightBet(object? parameter)
    {
        if (parameter is string numStr && int.TryParse(numStr, out int number))
        {
            if (_game.PlaceBet(new StraightBet(SelectedChip, number)))
            {
                Balance = _game.Player.Balance;
                GameMessage = $"Ставка {SelectedChip} на число {number} прийнята.";
            }
            else GameMessage = "Недостатньо коштів!";
        }
    }

    private void ClearBets()
    {
        _game.ClearBets();
        Balance = _game.Player.Balance;
        GameMessage = "Ставки скасовано.";
    }

    // --- ФІЗИКА ТА БАГАТОПОТОЧНІСТЬ ---

    private async Task SpinAsync()
    {
        if (_game.CurrentBets.Count == 0)
        {
            GameMessage = "Зробіть хоча б одну ставку!";
            return;
        }

        IsSpinning = true;

        var (winningNumber, targetIndex) = _game.Wheel.Spin();

        await AnimateRouletteAsync(targetIndex);

        // 🎯 ВИПРАВЛЕНЕ ВИЗНАЧЕННЯ РЕЗУЛЬТАТУ (НЕ З MODEL!)
        double sectorAngle = 360.0 / 37.0;

        double finalAngle = NormalizeAngle(WheelAngle + BallAngle);

        int correctedIndex = GetWinningIndex(finalAngle, sectorAngle);

        var correctedNumber = _game.Wheel.Sectors[correctedIndex];

        decimal totalPayout = _game.ResolveBets(correctedNumber);

        Balance = _game.Player.Balance;

        GameMessage = totalPayout > 0
            ? $"Випало {correctedNumber.Value} ({correctedNumber.Color}). Ви ВИГРАЛИ {totalPayout}!"
            : $"Випало {correctedNumber.Value} ({correctedNumber.Color}). Ставки програли.";

        IsSpinning = false;
    }

    private async Task AnimateRouletteAsync(int targetIndex)
    {
        double currentWheelAngle = WheelAngle;
        double currentBallAngle = BallAngle;
        double currentRadius = 140;

        double wheelSpeed = -3.0;
        double ballVelocity = 5.0;

        int totalFrames = 250;
        double sectorAngle = 360.0 / 37.0;

        double finalWheelAngle = currentWheelAngle + wheelSpeed * totalFrames;

        // 🎯 ФІКСОВАНА ЦІЛЬ (центр сектора!)
        double fixedTargetAngle = finalWheelAngle + (targetIndex * sectorAngle);

        for (int i = 0; i < totalFrames; i++)
        {
            double progress = (double)i / totalFrames;
            double speedMultiplier = 1.0 - Math.Pow(progress, 2);

            currentWheelAngle += wheelSpeed * speedMultiplier;

            ballVelocity *= 0.985;
            currentBallAngle += ballVelocity * speedMultiplier;

            if (progress > 0.65)
            {
                currentRadius = Math.Max(90, currentRadius - 1.2);

                double diff = fixedTargetAngle - currentBallAngle;

                diff = (diff + 540) % 360 - 180;

                ballVelocity += diff * 0.012;
                ballVelocity *= 0.92;
            }

            if (progress > 0.90)
                ballVelocity *= 0.85;

            if (progress > 0.97)
                ballVelocity = 0;

            double renderAngle = NormalizeAngle(currentBallAngle);

            double rad = renderAngle * Math.PI / 180.0;

            double bx = 150 + currentRadius * Math.Sin(rad) - 6;
            double by = 150 - currentRadius * Math.Cos(rad) - 6;

            Dispatcher.UIThread.Post(() =>
            {
                WheelAngle = currentWheelAngle;
                BallAngle = currentBallAngle;
                BallRadius = currentRadius;
                BallX = bx;
                BallY = by;
            });

            await Task.Delay(15);
        }
    }
    private double NormalizeAngle(double a)
    {
        a %= 360;
        if (a < 0) a += 360;
        return a;
    }

    private int GetWinningIndex(double angle, double sectorAngle)
    {
        angle = NormalizeAngle(angle + sectorAngle / 2.0);
        int index = (int)(angle / sectorAngle);
        return index % 37;
    }
}

// Допоміжний клас для команд, якщо у тебе немає CommunityToolkit
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object? parameter) => _execute(parameter);
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
public class WheelSectorUI
{
    public int Number { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public double Angle { get; set; }
}


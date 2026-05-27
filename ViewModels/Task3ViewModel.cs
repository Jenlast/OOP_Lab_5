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
    public ObservableCollection<Bet> TableBets { get; } = new();
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

    private decimal _selectedChip = 10m;
    public decimal SelectedChip
    {
        get => _selectedChip;
        set => SetProperty(ref _selectedChip, value);
    }

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

    private double _ballRadius = 200; // Відстань від центру (140 - зовнішнє коло, 90 - на числах)
    public double BallRadius
    {
        get => _ballRadius;
        set => SetProperty(ref _ballRadius, value);
    }

    public ObservableCollection<WheelSectorUI> UIWheelSectors { get; } = new();
    public System.Collections.Generic.IEnumerable<WheelSectorUI> SortedTableNumbers => UIWheelSectors.OrderBy(x => x.Number);

    private double _ballX = 225;
    public double BallX { get => _ballX; set => SetProperty(ref _ballX, value); }

    private double _ballY = 25;
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

    private void PlaceColorBet(object? parameter)
    {
        if (parameter is string colorStr && Enum.TryParse(colorStr, true, out RouletteColor color))
        {
            var bet = new ColorBet(SelectedChip, color);
            if (_game.PlaceBet(bet))
            {
                TableBets.Add(bet); // Додаємо в таблицю на екрані
                Balance = _game.Player.Balance;
                GameMessage = $"Ставка {SelectedChip}$ на {bet.BetDescription} прийнята.";
            }
            else GameMessage = "Недостатньо коштів!";
        }
    }

    private void PlaceStraightBet(object? parameter)
    {
        // ВИПРАВЛЕНО: Parameter тепер int, бо кнопка передає число!
        if (parameter is int number) 
        {
            var bet = new StraightBet(SelectedChip, number);
            if (_game.PlaceBet(bet))
            {
                TableBets.Add(bet); // Додаємо в таблицю на екрані
                Balance = _game.Player.Balance;
                GameMessage = $"Ставка {SelectedChip}$ на число {number} прийнята.";
            }
            else GameMessage = "Недостатньо коштів!";
        }
    }

    private void ClearBets()
    {
        _game.ClearBets();
        TableBets.Clear(); // Очищаємо таблицю
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

        // 1. Отримуємо ЧЕСНЕ виграшне число з нашої Моделі
        var (winningNumber, targetIndex) = _game.Wheel.Spin();

        // 2. Запускаємо фізичну анімацію, яка ГАРАНТОВАНО приземлить кульку на targetIndex
        await AnimateRouletteAsync(targetIndex);

        // 3. Розраховуємо виграші (більше ніяких підмін індексів!)
        decimal totalPayout = _game.ResolveBets(winningNumber);

        Balance = _game.Player.Balance;

        GameMessage = totalPayout > 0
            ? $"Випало {winningNumber.Value} ({winningNumber.Color}). Ви ВИГРАЛИ {totalPayout}$!"
            : $"Випало {winningNumber.Value} ({winningNumber.Color}). Ставки програли.";

        IsSpinning = false;
        TableBets.Clear();
    }

    private async Task AnimateRouletteAsync(int targetIndex)
    {
        double startWheelAngle = WheelAngle;
        double startBallAngle = BallAngle;

        int totalFrames = 300; 
        double sectorAngle = 360.0 / 37.0;

        // Крутимо колесо на 3 повних оберти назад
        double endWheelAngle = startWheelAngle - (3 * 360);
        
        // ІДЕАЛЬНА МАТЕМАТИКА (без зсувів на половину сектора!)
        double finalPocketAngle = endWheelAngle + (targetIndex * sectorAngle);

        // Рахуємо шлях кульки
        double ballDistance = (finalPocketAngle - startBallAngle) % 360;
        if (ballDistance < 0) ballDistance += 360;

        // Кулька робить 5 обертів + відстань до комірки
        double endBallAngle = startBallAngle + ballDistance + (5 * 360);

        for (int i = 0; i <= totalFrames; i++)
        {
            double t = (double)i / totalFrames; 
            
            // Плавне гальмування
            double ease = 1.0 - Math.Pow(1.0 - t, 3);

            double currentWheelAngle = startWheelAngle + (endWheelAngle - startWheelAngle) * ease;
            double currentBallAngle = startBallAngle + (endBallAngle - startBallAngle) * ease;

            // Радіус падіння
            double currentRadius = 200; 
            if (t > 0.5) 
            {
                double dropProgress = (t - 0.5) / 0.5; 
                double dropEase = dropProgress * dropProgress * (3 - 2 * dropProgress);
                currentRadius = 200 - (70 * dropEase); 
            }

            // Розрахунок координат
            double renderAngle = NormalizeAngle(currentBallAngle);
            double rad = renderAngle * Math.PI / 180.0;
            
            double bx = 225 + currentRadius * Math.Sin(rad) - 8;
            double by = 225 - currentRadius * Math.Cos(rad) - 8;

            Dispatcher.UIThread.Post(() =>
            {
                WheelAngle = currentWheelAngle;
                BallAngle = currentBallAngle;
                BallRadius = currentRadius;
                BallX = bx;
                BallY = by;
            });

            await Task.Delay(16);
        }
    }
    private double NormalizeAngle(double a)
    {
        a %= 360;
        if (a < 0) a += 360;
        return a;
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


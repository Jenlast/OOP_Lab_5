using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OOP_Lab5.Models;

namespace OOP_Lab5.ViewModels;

public partial class Task4ViewModel : ViewModelBase
{
    // Колекції для відмальовки
    public ObservableCollection<PlinkoPeg> Pegs { get; } = new();
    public ObservableCollection<PlinkoSlot> Slots { get; } = new();
    public ObservableCollection<PlinkoBall> Balls { get; } = new();

    private readonly object _balanceLock = new object();

    [ObservableProperty] private decimal _balance = 1000;
    [ObservableProperty] private decimal _betAmount = 10;
    [ObservableProperty] private string _gameMessage = "Натисніть DROP, щоб кинути кульку!";

    [ObservableProperty] private int _selectedRiskIndex = 1;

    partial void OnSelectedRiskIndexChanged(int value)
    {
        UpdateSlots();
    }

    // Налаштування дошки
    private const int Rows = 12; // Кількість рядків з кілочками
    private const double StartX = 400;
    private const double StartY = 50;
    private const double PegSpacingX = 40;
    private const double PegSpacingY = 40;

    public Task4ViewModel()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        Pegs.Clear();
        // 1. Генеруємо піраміду кілочків (вона незмінна)
        for (int row = 2; row < Rows + 2; row++)
        {
            int pegsInRow = row + 1;
            double firstPegX = StartX - (row * PegSpacingX / 2.0);

            for (int col = 0; col < pegsInRow; col++)
            {
                Pegs.Add(new PlinkoPeg { X = firstPegX + (col * PegSpacingX), Y = StartY + (row * PegSpacingY) });
            }
        }

        // 2. Генеруємо слоти (залежить від ризику)
        UpdateSlots();
    }

    private void UpdateSlots()
    {
        Slots.Clear();

        decimal[] multipliers;

        // Вибір коефіцієнтів залежно від рівня ризику
        if (SelectedRiskIndex == 0) // НИЗЬКИЙ
        {
            multipliers = new decimal[] { 10, 4, 2, 1.4m, 1.1m, 1, 0.5m, 1, 1.1m, 1.4m, 2, 4, 10 };
        }
        else if (SelectedRiskIndex == 2) // ВИСОКИЙ
        {
            multipliers = new decimal[] { 170, 43, 10, 3, 1.5m, 0.2m, 0.2m, 0.2m, 1.5m, 3, 10, 43, 170 };
        }
        else // СЕРЕДНІЙ (За замовчуванням)
        {
            multipliers = new decimal[] { 33, 14, 5, 2, 1.5m, 0.4m, 0.2m, 0.4m, 1.5m, 2, 5, 14, 33 };
        }

        // Правильні кольори казино (Краї - Зелені, Центр - Червоний)
        string[] colors = { 
            "#00FF00", "#32CD32", "#ADFF2F", "#FFD700", "#FFA500", "#FF4500", "#FF0000", 
            "#FF4500", "#FFA500", "#FFD700", "#ADFF2F", "#32CD32", "#00FF00" 
        };

        double slotsY = StartY + ((Rows + 1) * PegSpacingY);
        double firstSlotX = StartX - (Rows * PegSpacingX / 2.0);

        for (int i = 0; i < multipliers.Length; i++)
        {
            Slots.Add(new PlinkoSlot
            {
                X = firstSlotX + (i * PegSpacingX),
                Y = slotsY,
                Multiplier = multipliers[i],
                MultiplierText = $"{multipliers[i]}x",
                ColorHex = colors[i]
            });
        }
    }

    [RelayCommand]
    private void DropBall()
    {
        if (Balance < BetAmount)
        {
            GameMessage = "Недостатньо коштів!";
            return;
        }

        // Знімаємо гроші відразу
        Balance -= BetAmount;
        GameMessage = "Кулька пішла!";

        // Створюємо нову кульку
        var ball = new PlinkoBall { X = StartX, Y = StartY };
        Balls.Add(ball);

        // ЗАПУСКАЄМО КУЛЬКУ В ОКРЕМОМУ НЕЗАЛЕЖНОМУ ПОТОЦІ
        Task.Run(() => SimulateBallPhysicsAsync(ball, BetAmount));
    }

    private async Task SimulateBallPhysicsAsync(PlinkoBall ball, decimal betAmount)
    {
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        double currentX = ball.X;
        double currentY = ball.Y;
        int rightJumps = 0; 

        // 1. Падіння по цвяшках
        for (int row = 0; row <= Rows; row++)
        {
            bool goRight = rnd.NextDouble() > 0.5;
            if (goRight) rightJumps++;

            double targetX = currentX + (goRight ? PegSpacingX / 2.0 : -PegSpacingX / 2.0);
            double targetY = currentY + PegSpacingY;

            int frames = 15;
            for (int i = 1; i <= frames; i++)
            {
                double t = (double)i / frames;
                double arc = Math.Sin(t * Math.PI) * 10; 

                double nextX = currentX + (targetX - currentX) * t;
                double nextY = currentY + (targetY - currentY) * t - arc;

                Dispatcher.UIThread.Post(() =>
                {
                    ball.X = nextX;
                    ball.Y = nextY;
                });

                await Task.Delay(16); 
            }

            currentX = targetX;
            currentY = targetY;
        }

        // 2. НОВЕ: Кулька долетіла до верху слота. Робимо плавне занурення всередину!
        int dropFrames = 8;
        for (int i = 1; i <= dropFrames; i++)
        {
            // Опускаємо кульку ще на 20 пікселів вниз (прямо в центр кольорового квадратика)
            double finalDropY = currentY + (20.0 / dropFrames) * i;
            
            Dispatcher.UIThread.Post(() =>
            {
                ball.Y = finalDropY;
            });
            await Task.Delay(16);
        }

        // 3. КУЛЬКА ВПАЛА В СЛОТ - РАХУЄМО ГРОШІ
        var winSlot = Slots[rightJumps];
        decimal winAmount = betAmount * winSlot.Multiplier;

        lock (_balanceLock)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Balance += winAmount;
                GameMessage = $"Виграш: {winAmount}$ ({winSlot.MultiplierText})";
                Balls.Remove(ball); // Видаляємо кульку, бо вона вже "сховалася" в слоті
            });
        }
    }
}
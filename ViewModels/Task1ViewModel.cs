using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OOP_Lab5.Models;

namespace OOP_Lab5.ViewModels;

public partial class Task1ViewModel : ViewModelBase
{
    public ObservableCollection<HorseModel> Horses { get; set; } = new();
    
    [ObservableProperty] private int _balance = 1000;
    [ObservableProperty] private int _betAmount = 100;
    [ObservableProperty] private HorseModel? _selectedHorseForBet;
    [ObservableProperty] private bool _isSimulationRunning;
    [ObservableProperty] private int _horseCount = 5;
    
    private const double TrackLength = 600;
    private Bitmap[] _horseFrames = new Bitmap[12];

    public Task1ViewModel() 
    { 
        LoadImages();
        InitializeHorses(HorseCount); 
    }

    partial void OnHorseCountChanged(int value)
    {
        if (!IsSimulationRunning)
        {
            InitializeHorses(value);
            SelectedHorseForBet = null;
        }
    }

    private void LoadImages()
    {
        string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;
        for (int i = 0; i < 12; i++)
        {
            string fileName = $"avares://{assemblyName}/Assets/Images/Horses/WithOutBorder_{i:D4}.png";
            try { _horseFrames[i] = new Bitmap(AssetLoader.Open(new Uri(fileName))); }
            catch (Exception ex) { Console.WriteLine($"[ПОМИЛКА] {ex.Message}"); }
        }
    }

    private void InitializeHorses(int count)
    {
        Horses.Clear();
        var rnd = new Random();
        
        var colors = new[] { 
            Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple,
            Colors.Cyan, Colors.Magenta, Colors.Brown, Colors.DeepPink, Colors.Teal 
        };
        
        double startY = 110; 
        double endY = 360;   
        double step = count > 1 ? (endY - startY) / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            Horses.Add(new HorseModel
            {
                Name = $"Кінь {i + 1}", 
                HorseColor = colors[i % colors.Length],
                BaseSpeed = rnd.Next(5, 11), 
                Coefficient = Math.Round(1.5 + rnd.NextDouble() * 3, 2),
                PositionX = 0, 
                ViewTop = startY + (i * step), 
                AnimationFrames = _horseFrames,
                CurrentFrame = _horseFrames[0]
            });
        }
    }

    [RelayCommand]
    private async Task StartSimulationAsync()
    {
        var betHorse = SelectedHorseForBet;
        if (IsSimulationRunning || betHorse == null) return;

        Balance -= BetAmount;
        IsSimulationRunning = true;
        
        // Скидання параметрів перед новим забігом
        foreach (var h in Horses) 
        { 
            h.PositionX = 0; 
            h.IsFinished = false; 
            h.Rank = 0; 
            h.FinishTime = TimeSpan.Zero; 
        }

        var rnd = new Random();
        var sw = Stopwatch.StartNew();
        int rankCounter = 1;

        // Цикл бігу (поки хоч один не добіг)
        while (Horses.Any(h => !h.IsFinished))
        {
            var tasks = Horses.Where(h => !h.IsFinished).Select(horse => Task.Run(() =>
            {
                horse.Move(rnd);
                // Фіксація фінішу
                if (horse.PositionX >= TrackLength) { 
                    horse.FinishTime = sw.Elapsed; // Точний запис часу
                    horse.PositionX = TrackLength; 
                    horse.IsFinished = true; 
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Присвоюємо місця (ранг) тим, хто щойно фінішував, сортуючи їх за мілісекундами
                var newlyFinished = Horses.Where(h => h.IsFinished && h.Rank == 0).OrderBy(h => h.FinishTime);
                foreach (var h in newlyFinished) 
                {
                    h.Rank = rankCounter++;
                }
            });
            
            await Task.Delay(50);
        }

        sw.Stop();

        // СОРТУВАННЯ ТАБЛИЦІ В КІНЦІ ЗАБІГУ
        // Перебудовуємо список один раз, щоб відобразити від 1 до останнього місця
        var sortedList = Horses.OrderBy(h => h.Rank).ToList();
        var tempSelected = SelectedHorseForBet;
        
        Horses.Clear();
        foreach (var h in sortedList) 
        {
            Horses.Add(h);
        }
        SelectedHorseForBet = tempSelected;

        IsSimulationRunning = false;
        if (betHorse.Rank == 1) Balance += (int)(BetAmount * betHorse.Coefficient);
    }

    [RelayCommand]
    private void ResetGame()
    {
        Balance = 1000;
        BetAmount = 100;
        InitializeHorses(HorseCount);
        SelectedHorseForBet = null;
    }
}
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
    public ObservableCollection<HorseModel> LeaderboardHorses { get; set; } = new();
    
    [ObservableProperty] private int _balance = 1000;
    [ObservableProperty] private int _betAmount = 100;
    [ObservableProperty] private HorseModel? _selectedHorseForBet;
    [ObservableProperty] private bool _isSimulationRunning;
    [ObservableProperty] private int _horseCount = 5;
    [ObservableProperty] private int _cameraHorseIndex = -1;
    [ObservableProperty] private string _cameraButtonText = "Камера: Загальна";

    [ObservableProperty] private double _finishLineViewPositionX = 660; 
    
    // НОВА ВЛАСТИВІСТЬ ДЛЯ ТЕКСТУ ПОМИЛКИ
    [ObservableProperty] private string _errorMessage = string.Empty;
    
    private const double TrackLength = 3000;
    private const double ScreenWidth = 700; 
    private Bitmap[] _horseFrames = new Bitmap[12];

    public Task1ViewModel() 
    { 
        LoadImages();
        InitializeHorses(HorseCount); 
    }

    // Автоматично прибираємо помилку, якщо гравець змінив кількість коней
    partial void OnHorseCountChanged(int value)
    {
        if (!IsSimulationRunning)
        {
            InitializeHorses(value);
            SelectedHorseForBet = null;
            ErrorMessage = string.Empty; 
        }
    }

    // Автоматично прибираємо помилку, якщо гравець обрав коня або змінив ставку
    partial void OnSelectedHorseForBetChanged(HorseModel? value) => ErrorMessage = string.Empty;
    partial void OnBetAmountChanged(int value) => ErrorMessage = string.Empty;

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
        LeaderboardHorses.Clear(); 
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
            var horse = new HorseModel
            {
                Name = $"Кінь {i + 1}", 
                HorseColor = colors[i % colors.Length],
                BaseSpeed = rnd.Next(5, 11), 
                Coefficient = Math.Round(1.5 + rnd.NextDouble() * 3, 2),
                PositionX = 0, 
                ViewTop = startY + (i * step), 
                AnimationFrames = _horseFrames,
                CurrentFrame = _horseFrames[0],
                LiveTimeDisplay = "0.000 сек"
            };
            Horses.Add(horse);
            LeaderboardHorses.Add(horse);
        }
    }

    [RelayCommand]
    private async Task StartSimulationAsync()
    {
        if (IsSimulationRunning) return;

        ErrorMessage = string.Empty; // Очищаємо старі помилки перед перевіркою

        int selectedIndex = SelectedHorseForBet != null ? Horses.IndexOf(SelectedHorseForBet) : -1;
        
        // ПЕРЕВІРКА 1: Чи обраний кінь
        if (selectedIndex == -1)
        {
            ErrorMessage = "Оберіть фаворита для ставки!";
            return;
        }

        // ПЕРЕВІРКА 2: Чи вистачає грошей
        if (Balance < BetAmount)
        {
            ErrorMessage = "Недостатньо коштів на балансі!";
            return;
        }

        InitializeHorses(HorseCount);
        SelectedHorseForBet = Horses[selectedIndex];
        var betHorse = SelectedHorseForBet;

        Balance -= BetAmount;
        IsSimulationRunning = true;

        var rnd = new Random();
        var sw = Stopwatch.StartNew();
        int rankCounter = 1;

        while (Horses.Any(h => !h.IsFinished))
        {
            var tasks = Horses.Where(h => !h.IsFinished).Select(horse => Task.Run(() =>
            {
                horse.Move(rnd);
                if (horse.PositionX >= TrackLength) { 
                    horse.IsFinished = true;
                    horse.PositionX = TrackLength; 
                    horse.FinishTime = sw.Elapsed; 
                    horse.LiveTimeDisplay = $"{(int)horse.FinishTime.TotalSeconds}.{horse.FinishTime.Milliseconds:D3} сек";
                }
            })).ToArray();

            await Task.WhenAll(tasks);
            var currentElapsed = sw.Elapsed;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                double cameraOffsetX = 0;

                if (CameraHorseIndex != -1 && CameraHorseIndex < Horses.Count)
                {
                    var targetHorse = Horses[CameraHorseIndex];
                    cameraOffsetX = targetHorse.PositionX - 150; 
                    
                    if (cameraOffsetX < 0) cameraOffsetX = 0; 
                }

                foreach (var h in Horses)
                {
                    if (CameraHorseIndex == -1)
                    {
                        h.ViewPositionX = (h.PositionX / TrackLength) * ScreenWidth;
                    }
                    else 
                    {
                        h.ViewPositionX = h.PositionX - cameraOffsetX;
                    }

                    if (!h.IsFinished)
                    {
                        h.LiveTimeDisplay = $"{(int)currentElapsed.TotalSeconds}.{currentElapsed.Milliseconds:D3} сек";
                    }
                }

                FinishLineViewPositionX = CameraHorseIndex == -1 
                    ? ScreenWidth 
                    : TrackLength - cameraOffsetX;

                var newlyFinished = Horses.Where(h => h.IsFinished && h.Rank == 0).OrderBy(h => h.FinishTime).ToList();
                foreach (var h in newlyFinished) 
                {
                    var tiedHorse = Horses.FirstOrDefault(other => other.Rank > 0 && Math.Abs((other.FinishTime - h.FinishTime).TotalMilliseconds) <= 20);
                    if (tiedHorse != null) { h.Rank = tiedHorse.Rank; rankCounter++; }
                    else { h.Rank = rankCounter++; }
                }

                var currentOrder = Horses
                    .OrderByDescending(h => h.IsFinished)    
                    .ThenBy(h => h.IsFinished ? h.Rank : 0)  
                    .ThenByDescending(h => h.PositionX)      
                    .ToList();

                // НОВИЙ НАДІЙНИЙ КОД:
                LeaderboardHorses.Clear();
                foreach (var h in currentOrder)
                {
                    LeaderboardHorses.Add(h);
                }
            });
            
            await Task.Delay(50);
        }

        sw.Stop();
        IsSimulationRunning = false;
        
        if (betHorse.Rank == 1) Balance += (int)(BetAmount * betHorse.Coefficient);

        var rand = new Random();
        foreach (var h in Horses)
        {
            double newBase = 1.0 +(h.Rank * 0.4);
            h.Coefficient = Math.Round(newBase + (rand.NextDouble() * 0.5), 2);
        }
    }

    [RelayCommand]
    private void SwitchCamera()
    {
        CameraHorseIndex++;
        if (CameraHorseIndex >= HorseCount) 
        {
            CameraHorseIndex = -1; // Повертаємось на загальний вигляд
        }

        CameraButtonText = CameraHorseIndex == -1 
            ? "Камера: Загальна" 
            : $"Камера: {Horses[CameraHorseIndex].Name}";
    }
}
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
    
    private const double TrackLength = 700;
    
    // Масив для зберігання завантажених кадрів
    private Bitmap[] _horseFrames = new Bitmap[12];

    public Task1ViewModel() 
    { 
        LoadImages();
        InitializeHorses(5); 
    }

    private void LoadImages()
    {
        // Отримуємо назву проекту автоматично
        string assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;

        for (int i = 0; i < 12; i++)
        {
            string fileName = $"avares://{assemblyName}/Assets/Images/Horses/WithOutBorder_{i:D4}.png";
            
            try
            {
                // Завантажуємо картинку з пам'яті програми
                _horseFrames[i] = new Bitmap(AssetLoader.Open(new Uri(fileName)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ПОМИЛКА] Не вдалося завантажити: {fileName}. Причина: {ex.Message}");
            }
        }
    }

    private void InitializeHorses(int count)
    {
        Horses.Clear();
        var rnd = new Random();
        var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple };
        
        for (int i = 0; i < count; i++)
        {
            Horses.Add(new HorseModel
            {
                Name = $"Кінь {i + 1}", 
                HorseColor = colors[i % colors.Length],
                BaseSpeed = rnd.Next(5, 11), 
                Coefficient = Math.Round(1.5 + rnd.NextDouble() * 3, 2),
                PositionX = 0, 
                ViewTop = i * 60 + 10, // Зробив відстань трохи більшою (60) для картинок
                AnimationFrames = _horseFrames,
                CurrentFrame = _horseFrames[0] // Ставимо перший кадр за замовчуванням
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
        foreach (var h in Horses) { h.PositionX = 0; h.IsFinished = false; h.Rank = 0; }

        var rnd = new Random();
        var sw = Stopwatch.StartNew();
        int rankCounter = 1;

        while (Horses.Any(h => !h.IsFinished))
        {
            var tasks = Horses.Where(h => !h.IsFinished).Select(horse => Task.Run(() =>
            {
                horse.Move(rnd);
                if (horse.PositionX >= TrackLength) { horse.IsFinished = true; horse.FinishTime = sw.Elapsed; horse.PositionX = TrackLength; }
            })).ToArray();

            await Task.WhenAll(tasks);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var h in Horses.Where(h => h.IsFinished && h.Rank == 0)) h.Rank = rankCounter++;
                var sorted = Horses.OrderByDescending(h => h.PositionX).ThenBy(h => h.FinishTime).ToList();
                var tempSelected = SelectedHorseForBet;
                Horses.Clear();
                foreach (var h in sorted) Horses.Add(h);
                SelectedHorseForBet = tempSelected;
            });
            await Task.Delay(50);
        }

        sw.Stop();
        IsSimulationRunning = false;
        if (betHorse.Rank == 1) Balance += (int)(BetAmount * betHorse.Coefficient);
    }
}
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
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

    public Task1ViewModel() { InitializeHorses(5); }

    private void InitializeHorses(int count)
    {
        Horses.Clear();
        var rnd = new Random();
        var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple };
        
        for (int i = 0; i < count; i++)
        {
            Horses.Add(new HorseModel
            {
                Name = $"Кінь {i + 1}", HorseColor = colors[i % colors.Length],
                BaseSpeed = rnd.Next(5, 11), Coefficient = Math.Round(1.5 + rnd.NextDouble() * 3, 2),
                PositionX = 0, ViewTop = i * 40 + 20
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
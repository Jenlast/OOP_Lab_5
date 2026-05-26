using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OOP_Lab5.Models;

namespace OOP_Lab5.ViewModels;

public partial class Task2ViewModel : ViewModelBase
{
    public ObservableCollection<VoronoiPoint> Points { get; set; } = new();
    
    [ObservableProperty] private WriteableBitmap? _voronoiImage;
    [ObservableProperty] private bool _useMultiThreading = true;
    [ObservableProperty] private int _selectedMetricIndex = 0;
    [ObservableProperty] private string _performanceInfo = "";

    private const int Width = 800; private const int Height = 600;

    public Task2ViewModel() { VoronoiImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888); }
    partial void OnSelectedMetricIndexChanged(int value) => GenerateVoronoi();

    [RelayCommand]
    private void AddRandomPoints()
    {
        var rnd = new Random();
        for (int i = 0; i < 5; i++) Points.Add(new VoronoiPoint { X = rnd.Next(10, Width - 10), Y = rnd.Next(10, Height - 10), RegionColor = Color.FromRgb((byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256)) });
        GenerateVoronoi();
    }

    [RelayCommand]
    private void ClearPoints()
    {
        Points.Clear();
        VoronoiImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888);
        PerformanceInfo = "Очищено";
    }

    public void GenerateVoronoi()
    {
        if (Points.Count == 0) return;
        IDistanceMetric metric = SelectedMetricIndex == 0 ? new EuclideanMetric() : new ManhattanMetric();
        var sw = Stopwatch.StartNew();
        var newImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888);

        using (var buf = newImage.Lock())
        {
            if (UseMultiThreading) Parallel.For(0, Height, y => ProcessRow(y, Width, buf, metric));
            else for (int y = 0; y < Height; y++) ProcessRow(y, Width, buf, metric);
            DrawPoints(buf);
        }
        sw.Stop();
        VoronoiImage = newImage;
        PerformanceInfo = $"Час: {sw.ElapsedMilliseconds} мс | Потоки: {(UseMultiThreading ? "Багато" : "Один")}";
    }

    private unsafe void ProcessRow(int y, int width, ILockedFramebuffer buf, IDistanceMetric metric)
    {
        uint* ptr = (uint*)buf.Address + y * (buf.RowBytes / 4);
        for (int x = 0; x < width; x++)
        {
            double minDist = double.MaxValue; Color closestColor = Colors.Gray;
            foreach (var p in Points) { double dist = metric.Calculate(x, y, p.X, p.Y); if (dist < minDist) { minDist = dist; closestColor = p.RegionColor; } }
            *ptr++ = (uint)((255 << 24) | (closestColor.R << 16) | (closestColor.G << 8) | closestColor.B);
        }
    }

    private unsafe void DrawPoints(ILockedFramebuffer buf)
    {
        uint* basePtr = (uint*)buf.Address;
        foreach (var p in Points)
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = (int)p.X + dx, ny = (int)p.Y + dy;
                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height) *(basePtr + (ny * (buf.RowBytes / 4)) + nx) = 0xFF000000;
                }
    }
}
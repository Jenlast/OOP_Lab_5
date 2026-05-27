using System;
using System.Threading;
using System.Linq;
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
   [ObservableProperty] private int _pointsToAddCount = 50;
   [ObservableProperty] private string _errorMessage = string.Empty;

   partial void OnPointsToAddCountChanged(int value)
   {
       ErrorMessage = string.Empty;
   }

   private const int Width = 800; private const int Height = 600;

   public Task2ViewModel() { VoronoiImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888); }
   partial void OnSelectedMetricIndexChanged(int value) => GenerateVoronoi();

   [RelayCommand]
   private void AddRandomPoints()
   {
       if (PointsToAddCount < 1)
       {
           ErrorMessage = "Помилка: Кількість точок має бути більшою за 0!";
           return;
       }
       if (PointsToAddCount > 300)
       {
           ErrorMessage = $"Помилка: Забагато точок! Максимум 300 за раз. (Ви ввели {PointsToAddCount})";
           return;
       }

       ErrorMessage = string.Empty;

       var rnd = new Random();
       for (int i = 0; i < PointsToAddCount; i++)
       {
           Points.Add(new VoronoiPoint
           {
               X = rnd.Next(10, Width - 10),
               Y = rnd.Next(10, Height - 10),
               RegionColor = Color.FromRgb((byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256))
           });
       }
      
       GenerateVoronoi();
   }

   [RelayCommand]
   private void ClearPoints()
   {
       Points.Clear();
       VoronoiImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888);
       PerformanceInfo = "Очищено";
   }

   [RelayCommand]
   private void RemoveSmallestRegions()
   {
       if (Points.Count == 0) return;

       int countToRemove = Math.Max(1, (int)(Points.Count * 0.3));
      
       if (countToRemove >= Points.Count) countToRemove = Points.Count - 1;

       var toRemove = Points.OrderBy(p => p.AreaPixels).Take(countToRemove).ToList();

       foreach (var p in toRemove)
       {
           Points.Remove(p);
       }

       GenerateVoronoi();
       PerformanceInfo = $"Видалено {countToRemove} найменших локусів";
   }

   public void GenerateVoronoi()
   {
       if (Points.Count == 0) return;
       IDistanceMetric metric = SelectedMetricIndex == 0 ? new EuclideanMetric() : new ManhattanMetric();

       GC.Collect();
       long memoryBefore = GC.GetTotalMemory(true);
       var currentProcess = Process.GetCurrentProcess();
       TimeSpan cpuTimeBefore = currentProcess.TotalProcessorTime;
       var sw = Stopwatch.StartNew();
       var newImage = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888);

       int[] pixelCounts = new int[Points.Count];

       using (var buf = newImage.Lock())
       {
           if (UseMultiThreading) Parallel.For(0, Height, y => ProcessRow(y, Width, buf, metric, pixelCounts));
           else for (int y = 0; y < Height; y++) ProcessRow(y, Width, buf, metric, pixelCounts);
           DrawPoints(buf);
       }
      
       for (int i = 0; i < Points.Count; i++)
       {
           Points[i].AreaPixels = pixelCounts[i];
       }

       sw.Stop();
       TimeSpan cpuTimeAfter = currentProcess.TotalProcessorTime;
       long memoryAfter = GC.GetTotalMemory(false);

       double cpuMs = (cpuTimeAfter - cpuTimeBefore).TotalMilliseconds;
       double memUsedMb = Math.Max(0,(memoryAfter - memoryBefore) / (1024.0 * 1024.0));
       VoronoiImage = newImage;
       PerformanceInfo = $"Час: {sw.ElapsedMilliseconds} мс | CPU: {cpuMs:F1} мс | Пам'ять: {memUsedMb:F2} МБ";
   }

   private unsafe void ProcessRow(int y, int width, ILockedFramebuffer buf, IDistanceMetric metric, int[] pixelCounts)
   {
       uint* ptr = (uint*)buf.Address + y * (buf.RowBytes / 4);
       for (int x = 0; x < width; x++)
       {
           double minDist = double.MaxValue;
           Color closestColor = Colors.Gray;
           int closestIndex = -1;

           for (int i = 0; i < Points.Count; i++)
           {
               double dist = metric.Calculate(x, y, Points[i].X, Points[i].Y);
               if (dist < minDist)
               {
                   minDist = dist;
                   closestColor = Points[i].RegionColor;
                   closestIndex = i; 
               }
           }
          
           if (closestIndex != -1)
           {
               Interlocked.Increment(ref pixelCounts[closestIndex]);
           }

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
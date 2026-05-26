using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using OOP_Lab5.Models;
using OOP_Lab5.ViewModels;

namespace OOP_Lab5.Views;

public partial class Task2View : UserControl
{
   public Task2View()
   {
       InitializeComponent();
   }

   private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
   {
       if (DataContext is Task2ViewModel viewModel)
       {
           // Отримуємо координати кліку відносно картинки
           var point = e.GetPosition((Visual)sender!);
          
           // ЛІВИЙ КЛІК: Додати точку
           if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
           {
               var rnd = new Random();
               viewModel.Points.Add(new VoronoiPoint
               {
                   X = point.X,
                   Y = point.Y,
                   RegionColor = Color.FromRgb((byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256), (byte)rnd.Next(50, 256))
               });
               viewModel.GenerateVoronoi();
           }
           // ПРАВИЙ КЛІК: Видалити точку
           else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
           {
               if (viewModel.Points.Count == 0) return;

               VoronoiPoint? closestPoint = null;
               double minDist = double.MaxValue;

               // Шукаємо найближчу точку до місця кліку
               foreach (var p in viewModel.Points)
               {
                   double dist = Math.Sqrt(Math.Pow(p.X - point.X, 2) + Math.Pow(p.Y - point.Y, 2));
                   if (dist < minDist)
                   {
                       minDist = dist;
                       closestPoint = p;
                   }
               }

               // Видаляємо, якщо клікнули досить близько (радіус 15 пікселів)
               if (closestPoint != null && minDist < 15)
               {
                   viewModel.Points.Remove(closestPoint);
                   viewModel.GenerateVoronoi();
               }
           }
       }
   }
}
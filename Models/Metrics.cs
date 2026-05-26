using System;
namespace OOP_Lab5.Models;

public interface IDistanceMetric { double Calculate(double x1, double y1, double x2, double y2); }
public class EuclideanMetric : IDistanceMetric { public double Calculate(double x1, double y1, double x2, double y2) => Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)); }
public class ManhattanMetric : IDistanceMetric { public double Calculate(double x1, double y1, double x2, double y2) => Math.Abs(x1 - x2) + Math.Abs(y1 - y2); }
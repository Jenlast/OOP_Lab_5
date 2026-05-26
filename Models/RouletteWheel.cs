using System;
using System.Collections.Generic;

namespace OOP_Lab5.Models;

public class RouletteWheel
{
    private readonly Random _random = new();
    public List<RouletteNumber> Sectors { get; } = new();

    public RouletteWheel()
    {
        foreach (var num in RouletteConstants.WheelSequence)
        {
            Sectors.Add(new RouletteNumber(num, RouletteConstants.GetColorForNumber(num)));
        }
    }

    public (RouletteNumber Number, int TargetIndex) Spin()
    {
        int targetIndex = _random.Next(Sectors.Count);
        return (Sectors[targetIndex], targetIndex);
    }
}
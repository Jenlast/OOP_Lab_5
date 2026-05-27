using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Models;

public abstract partial class Bet : ObservableObject
{
    public decimal Amount { get; }
    
    public abstract string BetDescription { get; }

    [ObservableProperty] 
    private string _resultText = "⏳ В грі...";

    protected Bet(decimal amount)
    {
        Amount = amount;
    }

    public abstract decimal CalculatePayout(RouletteNumber winningNumber);
}

public partial class StraightBet : Bet
{
    public int TargetNumber { get; }
    public override string BetDescription => $"Число {TargetNumber}";

    public StraightBet(decimal amount, int targetNumber) : base(amount)
    {
        TargetNumber = targetNumber;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Value == TargetNumber ? Amount * 36 : 0;
    }
}

public partial class ColorBet : Bet
{
    public RouletteColor TargetColor { get; }
    public override string BetDescription => TargetColor == RouletteColor.Red ? "Червоне" : "Чорне";

    public ColorBet(decimal amount, RouletteColor targetColor) : base(amount)
    {
        TargetColor = targetColor;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Color == TargetColor ? Amount * 2 : 0;
    }
}

public partial class EvenOddBet : Bet
{
    public bool IsEven { get; }
    public override string BetDescription => IsEven ? "Парне (Even)" : "Непарне (Odd)";

    public EvenOddBet(decimal amount, bool isEven) : base(amount)
    {
        IsEven = isEven;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        if (winningNumber.Value == 0) return 0;
        
        bool isNumberEven = winningNumber.Value % 2 == 0;
        return isNumberEven == IsEven ? Amount * 2 : 0;
    }
}

public partial class HighLowBet : Bet
{
    public bool IsHigh { get; }
    public override string BetDescription => IsHigh ? "Великі (19-36)" : "Малі (1-18)";

    public HighLowBet(decimal amount, bool isHigh) : base(amount)
    {
        IsHigh = isHigh;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        if (winningNumber.Value == 0) return 0;

        bool isNumberHigh = winningNumber.Value >= 19;
        return isNumberHigh == IsHigh ? Amount * 2 : 0;
    }
}
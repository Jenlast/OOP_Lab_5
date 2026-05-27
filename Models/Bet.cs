namespace OOP_Lab5.Models;

public abstract class Bet
{
    public decimal Amount { get; }
    
    // Властивість для відображення в DataGrid (на що поставили)
    public abstract string BetDescription { get; }

    protected Bet(decimal amount)
    {
        Amount = amount;
    }

    public abstract decimal CalculatePayout(RouletteNumber winningNumber);
}

public class StraightBet : Bet
{
    public int TargetNumber { get; }
    
    public override string BetDescription => $"Число {TargetNumber}";

    public StraightBet(decimal amount, int targetNumber) : base(amount)
    {
        TargetNumber = targetNumber;
    }

    // Коефіцієнт x36 для одного числа
    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Value == TargetNumber ? Amount * 36 : 0;
    }
}

public class ColorBet : Bet
{
    public RouletteColor TargetColor { get; }

    public override string BetDescription => TargetColor == RouletteColor.Red ? "Червоне" : "Чорне";

    public ColorBet(decimal amount, RouletteColor targetColor) : base(amount)
    {
        TargetColor = targetColor;
    }

    // Коефіцієнт x2 для кольору
    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Color == TargetColor ? Amount * 2 : 0;
    }
}
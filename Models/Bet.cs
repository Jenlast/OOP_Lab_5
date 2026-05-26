namespace OOP_Lab5.Models;

public abstract class Bet
{
    public decimal Amount { get; }

    protected Bet(decimal amount)
    {
        Amount = amount;
    }

    public abstract decimal CalculatePayout(RouletteNumber winningNumber);
}

public class StraightBet : Bet
{
    public int TargetNumber { get; }

    public StraightBet(decimal amount, int targetNumber) : base(amount)
    {
        TargetNumber = targetNumber;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Value == TargetNumber ? Amount * 36 : 0;
    }
}

public class ColorBet : Bet
{
    public RouletteColor TargetColor { get; }

    public ColorBet(decimal amount, RouletteColor targetColor) : base(amount)
    {
        TargetColor = targetColor;
    }

    public override decimal CalculatePayout(RouletteNumber winningNumber)
    {
        return winningNumber.Color == TargetColor ? Amount * 2 : 0;
    }
}
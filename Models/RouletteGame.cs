using System.Collections.Generic;

namespace OOP_Lab5.Models;

public class Player
{
    public decimal Balance { get; set; } = 1000m;
}

public class RouletteGame
{
    public Player Player { get; } = new();
    public RouletteWheel Wheel { get; } = new();
    
    private readonly List<Bet> _currentBets = new();

    public IReadOnlyList<Bet> CurrentBets => _currentBets.AsReadOnly();

    public bool PlaceBet(Bet bet)
    {
        if (Player.Balance < bet.Amount)
            return false; 
        Player.Balance -= bet.Amount; 
        _currentBets.Add(bet);
        return true;
    }

    public void ClearBets()
    {
        foreach (var bet in _currentBets)
        {
            Player.Balance += bet.Amount;
        }
        _currentBets.Clear();
    }

    public decimal ResolveBets(RouletteNumber winningNumber)
    {
        decimal totalPayout = 0;

        foreach (var bet in _currentBets)
        {
            totalPayout += bet.CalculatePayout(winningNumber);
        }

        Player.Balance += totalPayout;
        _currentBets.Clear(); 

        return totalPayout; 
    }
}
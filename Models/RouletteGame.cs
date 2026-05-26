using System.Collections.Generic;

namespace OOP_Lab5.Models;

public class Player
{
    public decimal Balance { get; set; } = 1000m; // Стартовий баланс
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
            return false; // Недостатньо грошей

        Player.Balance -= bet.Amount; // Гроші знімаються при ставці
        _currentBets.Add(bet);
        return true;
    }

    public void ClearBets()
    {
        // Якщо гравець передумав до спіну - повертаємо гроші
        foreach (var bet in _currentBets)
        {
            Player.Balance += bet.Amount;
        }
        _currentBets.Clear();
    }

    // Викликається ПІСЛЯ того, як кулька фізично зупинилася на екрані
    public decimal ResolveBets(RouletteNumber winningNumber)
    {
        decimal totalPayout = 0;

        foreach (var bet in _currentBets)
        {
            totalPayout += bet.CalculatePayout(winningNumber);
        }

        Player.Balance += totalPayout; // Нараховуємо виграш
        _currentBets.Clear(); // Очищаємо стіл для наступного раунду

        return totalPayout; // Повертаємо суму виграшу для повідомлення на екрані
    }
}
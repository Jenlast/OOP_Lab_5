using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OOP_Lab5.Services;

public partial class BankService : ObservableObject
{
    public static BankService Instance { get; } = new();

    private readonly object _bankLock = new object(); 

    [ObservableProperty] private decimal _balance = 1000;
    [ObservableProperty] private decimal _debt = 0;

    public bool HasDebt => Debt > 0;

    public void AddMoney(decimal amount)
    {
        lock (_bankLock) 
        {
            Balance += amount;
        }
    }

    public bool TrySpendMoney(decimal amount)
    {
        lock (_bankLock)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }
    }

    public void TakeLoan(decimal amount)
    {
        lock (_bankLock)
        {
            Balance += amount;
            Debt += amount + (amount * 0.2m);
            OnPropertyChanged(nameof(HasDebt));
        }
    }

    public void RepayLoan(decimal amount)
    {
        lock (_bankLock)
        {
            if (Debt <= 0 || Balance < amount) return; 

            decimal actualRepayment = Math.Min(amount, Debt);

            Balance -= actualRepayment;
            Debt -= actualRepayment;

            OnPropertyChanged(nameof(HasDebt));
        }
    }
}
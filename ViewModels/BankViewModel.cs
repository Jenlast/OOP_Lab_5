using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OOP_Lab5.Services;

namespace OOP_Lab5.ViewModels;

public partial class BankViewModel : ViewModelBase
{
    public BankService Bank => BankService.Instance;

    [RelayCommand]
    private void TakeLoan(string amountStr)
    {
        if (decimal.TryParse(amountStr, out decimal amount))
        {
            Bank.TakeLoan(amount);
        }
    }

    [RelayCommand]
    private void RepayLoan(string amountStr)
    {
        if (decimal.TryParse(amountStr, out decimal amount))
        {
            Bank.RepayLoan(amount);
        }
    }
}
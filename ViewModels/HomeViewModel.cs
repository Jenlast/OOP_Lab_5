using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OOP_Lab5.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Action<ViewModelBase> _navigateAction;

    public HomeViewModel(Action<ViewModelBase> navigateAction)
    {
        _navigateAction = navigateAction;
    }

    [RelayCommand]
    private void GoToTask1() => _navigateAction(new Task1ViewModel());

    [RelayCommand]
    private void GoToTask2() => _navigateAction(new Task2ViewModel());

    [RelayCommand]
    private void GoToTask3() => _navigateAction(new Task3ViewModel());

    [RelayCommand]
    private void GoToTask4() => _navigateAction(new Task4ViewModel());
}
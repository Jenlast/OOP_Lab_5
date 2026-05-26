using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OOP_Lab5.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    // Дія (Action), яка буде передана з MainWindow, щоб перемикати екрани
    private readonly Action<ViewModelBase> _navigateAction;

    public HomeViewModel(Action<ViewModelBase> navigateAction)
    {
        _navigateAction = navigateAction;
    }

    [RelayCommand]
    private void GoToTask1() => _navigateAction(new Task1ViewModel());

    [RelayCommand]
    private void GoToTask2() => _navigateAction(new Task2ViewModel());

}
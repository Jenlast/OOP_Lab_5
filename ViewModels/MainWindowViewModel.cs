using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OOP_Lab5.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new Task1ViewModel(); // Відкриваємо Коней за замовчуванням
    }

    [RelayCommand]
    private void SwitchToTask1() => CurrentViewModel = new Task1ViewModel();

    [RelayCommand]
    private void SwitchToTask2() => CurrentViewModel = new Task2ViewModel();
}
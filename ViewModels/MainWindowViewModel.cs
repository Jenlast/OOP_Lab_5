using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OOP_Lab5.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBackButtonVisible))]
    private ViewModelBase _currentViewModel;

    public bool IsBackButtonVisible => CurrentViewModel is not HomeViewModel;

    public MainWindowViewModel()
    {
        _currentViewModel = new HomeViewModel(NavigateTo);
    }

    private void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }

    [RelayCommand]
    private void GoHome()
    {
        CurrentViewModel = new HomeViewModel(NavigateTo);
    }
}
using KWRP.Avalonia.Frontend.ViewModels;

namespace KWRP.Avalonia.Frontend.Services
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
        void NavigateTo<LayoutTwoViewModel, LeftViewModel, RightViewModel>()
            where LayoutTwoViewModel: ViewModelBase
            where LeftViewModel : ViewModelBase 
            where RightViewModel: ViewModelBase;

        void SwitchPageTo<TViewModel>() where TViewModel : ViewModelBase;
    }
}

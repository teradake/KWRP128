using KWRP.Avalonia.Frontend.ViewModels;
using R3;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class NavigationPageStore
    {
        private readonly Subject<ViewModelBase?> _subject = new();
        private ViewModelBase? _currentPageViewModel;

        public Observable<ViewModelBase?> ViewModelProvider => _subject;
        public ViewModelBase? CurrentPageViewModel => _currentPageViewModel;

        public void SetViewModel(ViewModelBase? viewModel)
        {
            _currentPageViewModel?.Dispose();
            _currentPageViewModel = viewModel;
            _subject.OnNext(viewModel);
        }
    }
}

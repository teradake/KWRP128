using KWRP.Avalonia.Frontend.ViewModels;
using R3;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class NavigationStore
    {
        private readonly Subject<ViewModelBase?> _subject = new();
        private ViewModelBase? _currentViewModel;

        public Observable<ViewModelBase?> ViewModelObservable => _subject;
        public ViewModelBase? CurrentViewModel => _currentViewModel;

        public void SetViewModel(ViewModelBase? viewModel)
        {
            _currentViewModel?.Dispose();
            _currentViewModel = viewModel;
            _subject.OnNext(viewModel);
        }
    }
}

using KWRP.Avalonia.Frontend.ViewModels;
using R3;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class NavigationTwoPanelStore
    {
        private readonly Subject<ViewModelBase?> _leftSubject = new();
        private readonly Subject<ViewModelBase?> _rightSubject = new();
        private ViewModelBase? _left;
        private ViewModelBase? _right;

        public Observable<ViewModelBase?> ObservableLeftViewModel => _leftSubject;
        public Observable<ViewModelBase?> ObservableRightViewModel => _rightSubject;

        public void SetLeftViewModel(ViewModelBase? viewModel)
        {
            _left?.Dispose();
            _left = viewModel;
            _leftSubject.OnNext(viewModel);
        }

        public void SetRightViewModel(ViewModelBase? viewModel)
        {
            _right?.Dispose();
            _right= viewModel;
            _rightSubject.OnNext(viewModel);
        }
    }
}

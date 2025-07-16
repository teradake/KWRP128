using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class StartUpViewModel : ViewModelBase
    {
        private readonly NavigationPageStore _navigationStore;
        private readonly INavigationService _navigationService;

        public StartUpViewModel(NavigationPageStore navigationStore, INavigationService navigationService)
        {
            _navigationStore = navigationStore;
            _navigationService = navigationService;

            CurrentPage = _navigationStore
                .ViewModelProvider
                .ToReadOnlyBindableReactiveProperty(_navigationStore.CurrentPageViewModel)
                .AddTo(Disposables);

            HomePageCommand = new ReactiveCommand(_ => _navigationService.SwitchPageTo<TopPageViewModel>()).AddTo(Disposables);
            MachinePageCommand = new ReactiveCommand(_ => _navigationService.SwitchPageTo<MachinePageViewModel>()).AddTo(Disposables);
            SettingPageCommand = new ReactiveCommand(_ => _navigationService.SwitchPageTo<SettingPageViewModel>()).AddTo(Disposables);
            LogPageCommand = new ReactiveCommand(_ => _navigationService.SwitchPageTo<LogPageViewModel>()).AddTo(Disposables);
            DxfPageCommand = new ReactiveCommand(_ => _navigationService.SwitchPageTo<DxfPageViewModel>()).AddTo(Disposables);
        }

        public ReactiveCommand HomePageCommand { get; }
        public ReactiveCommand MachinePageCommand { get; }
        public ReactiveCommand SettingPageCommand { get; }
        public ReactiveCommand DxfPageCommand { get; }
        public ReactiveCommand LogPageCommand { get; }

        public IReadOnlyBindableReactiveProperty<ViewModelBase?> CurrentPage { get; }
    }
}

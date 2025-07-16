using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Cryptography.X509Certificates;

namespace KWRP.Avalonia.Frontend.Services
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationStore _navigationStore;
        private readonly NavigationPageStore _navigationPageStore;
        private readonly NavigationTwoPanelStore _navigationTwoPanelStore;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(
            NavigationStore navigationStore, 
            IServiceProvider serviceProvider, 
            NavigationPageStore navigationPageStore, 
            NavigationTwoPanelStore navigationTwoPanelStore)
        {
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
            _navigationPageStore = navigationPageStore;
            _navigationTwoPanelStore = navigationTwoPanelStore;
        }

        public void NavigateTo<TViewModel>()
            where TViewModel : ViewModelBase
        {
            _navigationStore.SetViewModel(_serviceProvider.GetRequiredService<TViewModel>());
        }

        public void NavigateTo<LayoutTwoViewModel, LeftViewModel, RightViewModel>()
            where LayoutTwoViewModel : ViewModelBase
            where LeftViewModel : ViewModelBase
            where RightViewModel : ViewModelBase
        {
            _navigationStore.SetViewModel(_serviceProvider.GetRequiredService<LayoutTwoViewModel>());
            _navigationTwoPanelStore.SetLeftViewModel(_serviceProvider.GetRequiredService<LeftViewModel>());
            _navigationTwoPanelStore.SetRightViewModel(_serviceProvider.GetRequiredService<RightViewModel>());
        }

        public void SwitchPageTo<TViewModel>() where TViewModel : ViewModelBase
        {
            _navigationPageStore.SetViewModel(_serviceProvider.GetRequiredService<TViewModel>());
        }
    }
}

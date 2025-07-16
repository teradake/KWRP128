using CommunityToolkit.Mvvm.ComponentModel;
using KWRP.Avalonia.Backend.Constants;
using R3;
using System;
using System.ComponentModel;

namespace KWRP.Avalonia.Frontend.ViewModels
{
    public class ViewModelBase : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsDevMode
        {
            get => KWRPConfigs.IsDevMode;
        }


        protected CompositeDisposable Disposables = new();
        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}

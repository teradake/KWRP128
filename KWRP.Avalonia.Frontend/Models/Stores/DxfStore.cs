using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using ObservableCollections;
using R3;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class DxfStore
    {
        public DxfStore()
        {
        }

        public ObservableList<DxfConverterOptionViewModel> Options { get; } = [];
        public ReactiveProperty<DxfConverterOptionViewModel?> SelectedOption { get; } = new();
    }
}

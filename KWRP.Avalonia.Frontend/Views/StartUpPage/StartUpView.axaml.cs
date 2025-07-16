using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KWRP.Avalonia.Frontend.Views.StartUpPage;

public partial class StartUpView : UserControl
{
    public StartUpView()
    {
        InitializeComponent();


        homeRadioButton.IsChecked = true;
    }
}
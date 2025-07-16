using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KWRP.Avalonia.Frontend.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBarPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // êÿÇËë÷Ç¶
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                // í èÌÇÃÉhÉâÉbÉOà⁄ìÆ
                BeginMoveDrag(e);
            }
        }

        private void MinimizeClick(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ToggleClick(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        }

        private void CloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
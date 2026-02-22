using Avalonia.Controls;
using Password_Manager.ViewModels;

namespace Password_Manager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DoubleClicked(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                (DataContext as MainWindowViewModel)?.SideMenuResizeCommand.Execute(null);
            }
        }
    }
}
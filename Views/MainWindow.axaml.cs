using Avalonia.Controls;
using Avalonia.Input;
using Vaultify.ViewModels;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Interactivity;

namespace Vaultify.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
        
            // Target your TextBox directly by its x:Name
            LoginBox.Focus();
        }

        private void DoubleClicked(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                (DataContext as MainWindowViewModel)?.SideMenuResizeCommand.Execute(null);
            }
        }
        private void TextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var current = sender as Control;
                if (current == null) return;

                var parent = current.GetVisualParent();
                if (parent == null) return;
                var textboxes = parent.GetVisualDescendants().OfType<TextBox>().ToList();

                int index = textboxes.IndexOf((TextBox)current);

                int nextindex = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? index - 1 : index + 1;

                if (nextindex != -1 && (nextindex >= 0 && nextindex < textboxes.Count))
                {
                    textboxes[nextindex].Focus();
                }
            }
        }
    }
}
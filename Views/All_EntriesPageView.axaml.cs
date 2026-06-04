using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Vaultify.Views;

public partial class All_EntriesPageView : UserControl
{
    public All_EntriesPageView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
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
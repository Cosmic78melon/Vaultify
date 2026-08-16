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
        if(e.Key != Key.Enter)
        return;

        if (e.Source is not Control current)
            return;

        if (current is not TextBox && current is not AutoCompleteBox)
            return;

        var controls = this.GetVisualDescendants()
            .Where(x => x is TextBox || x is AutoCompleteBox)
            .Cast<Control>()
            .ToList();

        int index = controls.IndexOf(current);

        if (index == -1)
            return;

        int nextIndex = e.KeyModifiers.HasFlag(KeyModifiers.Shift)
            ? index - 1
            : index + 1;

        if (nextIndex >= 0 && nextIndex < controls.Count)
        {
            e.Handled = true;
            controls[nextIndex].Focus();
        }
    }
}
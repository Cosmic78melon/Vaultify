using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Password_Manager.ViewModels;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Password_Manager
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal).Replace(".Views.", ".", StringComparison.InvariantCultureIgnoreCase);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name + "Type is: "+ type };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}

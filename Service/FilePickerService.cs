using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Vaultify.Service;

public class FilePickerService(Func<TopLevel?> toplevel)
{
    public async Task<string?> SaveFile(
        string masterPass)
    {
        var topLevel = toplevel();

        if (topLevel == null)
            return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select a Folder"
        });
        var path = folders.FirstOrDefault();
        if (path ==  null) return null;
        return path.TryGetLocalPath();
    }
}
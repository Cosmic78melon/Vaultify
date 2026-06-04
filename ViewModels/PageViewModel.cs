using CommunityToolkit.Mvvm.ComponentModel;
using Vaultify.Models;

namespace Vaultify.ViewModels
{
    public partial class PageViewModel: ViewModelBase
    {
        [ObservableProperty]
        public PageViewData _pageNames;

    }
}

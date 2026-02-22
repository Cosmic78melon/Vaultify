using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.ViewModels
{
    public partial class PageViewModel: ViewModelBase
    {
        [ObservableProperty]
        public PageViewData _pageNames;
    }
}

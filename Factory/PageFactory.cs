using Password_Manager.Models;
using Password_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Factory
{
    public partial class PageFactory(Func<PageViewData, PageViewModel> factory)
    {
        public  PageViewModel GetPageViewModel(PageViewData pagenames) =>  factory.Invoke(pagenames);
    }
}

using Password_Manager.Models;
using Password_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Factory
{
    public class PageFactory(Func<Type, PageViewModel> factory)
    {
        public PageViewModel GetPageViewModel<T>(Action<T>? Item = null)
            where T : PageViewModel
        {
            var viewmodel = factory(typeof(T));

            Item?.Invoke((T)viewmodel);

            return viewmodel;
        }
    }
}

using Vaultify.ViewModels;
using System;

namespace Vaultify.Factory
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

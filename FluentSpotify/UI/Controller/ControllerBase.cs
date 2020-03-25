using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentSpotify.UI.Controller
{
    internal abstract class ControllerBase<T> where T : DependencyObject
    {
        protected Page parent;
        protected T control;

        internal ControllerBase(Page parent, T control)
        {
            this.parent = parent;
            this.control = control;
        }

    }
}

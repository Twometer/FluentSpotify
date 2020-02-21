using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Util
{
    public interface IScrollNotify
    {

        void OnScroll(double current, double max);

    }
}

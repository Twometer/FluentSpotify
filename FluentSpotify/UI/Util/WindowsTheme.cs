using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.UI
{
    public class WindowsTheme
    {

        public static bool IsDarkMode()
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            return color.R == 0 && color.B == 0 && color.G == 0;
        }

    }
}

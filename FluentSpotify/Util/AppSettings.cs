using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentSpotify.Util
{
    public class AppSettings
    {
        private static ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;

        public static Theme AppTheme
        {
            get
            {
                if (!(Settings.Values["theme"] is string theme))
                    return Theme.Windows;
                return Enum.Parse<Theme>(theme);
            }
            set
            {
                Settings.Values["theme"] = value.ToString();
            }
        }

        public enum Theme
        {
            Light,
            Dark,
            Windows
        }

    }
}

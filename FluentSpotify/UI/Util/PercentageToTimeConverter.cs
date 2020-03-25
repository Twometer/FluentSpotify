using FluentSpotify.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace FluentSpotify.UI
{
    public class PercentageToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(string))
            {
                var fac = (double)value / 100.0d;
                var duration = Spotify.Playback.CurrentPlayer.CurrentTrack.Duration.TotalMilliseconds;
                return TimeSpan.FromMilliseconds(duration * fac).ToString(@"m\:ss");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}

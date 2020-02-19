using FluentSpotify.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FluentSpotify.Util
{
    public static class Extensions
    {

        public static ImageInfo FindByResolution(this IEnumerable<ImageInfo> infos, int res)
        {
            foreach (var info in infos)
                if (info.Height == res && info.Width == res)
                    return info;

            return infos.FirstOrDefault();
        }

        public static T ValueOrDefault<T>(this JToken token, string name, T fallback)
        {
            var val = token[name];
            if (val.Type == JTokenType.Null)
                return fallback;
            else
                return val.ToObject<T>();
        }

        public static async Task<string> RunScript(this WebView webView, string js)
        {
            return await webView.InvokeScriptAsync("eval", new string[] { js });
        }

    }
}

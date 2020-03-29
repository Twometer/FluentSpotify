using FluentSpotify.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        public static async void PrintStackTrace(this WebException e, string body = null, string header = "Failed to communicate with the server")
        {
            Log.Error("=== " + header + " ===");
            if (!string.IsNullOrEmpty(body))
                Log.Error(" Body: " + body);
            Log.Error(" Exception: ");
            Log.Error(e.ToString());
            Log.Error(" Server Response: ");
            using (var reader = new StreamReader(e.Response.GetResponseStream()))
            {
                Log.Error(await reader.ReadToEndAsync());
            }
        }

    }
}

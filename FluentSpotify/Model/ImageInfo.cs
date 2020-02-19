using FluentSpotify.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Model
{
    public class ImageInfo
    {
        public int Width { get; private set; }

        public int Height { get; private set; }

        public string Url { get; private set; }

        public static ImageInfo Parse(JObject obj)
        {
            return new ImageInfo()
            {
                Width = obj.ValueOrDefault("width", 0),
                Height = obj.ValueOrDefault("height", 0),
                Url = obj.Value<string>("url")
            };
        }

    }
}

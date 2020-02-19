using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Model
{
    public class Playlist
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Owner { get; private set; }

        public bool Collaborative { get; private set; }

        public bool Public { get; private set; }

        public int TrackCount { get; private set; }

        public IEnumerable<ImageInfo> Images { get; private set; }

        public static Playlist Parse(JObject json)
        {
            return new Playlist()
            {
                Id = json.Value<string>("id"),
                Name = json.Value<string>("name"),
                Description = WebUtility.HtmlDecode(json.Value<string>("description")),
                Owner = json["owner"].Value<string>("display_name"),
                Collaborative = json.Value<bool>("collaborative"),
                Public = json.Value<bool>("public"),
                TrackCount = json["tracks"].Value<int>("total"),
                Images = (json["images"] as JArray)?.Select(obj => ImageInfo.Parse((JObject)obj))
            };
        }
    }
}


using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Model
{
    public class Track
    {
        public string Id { get; private set; }

        public string Name { get; private set; }

        public TimeSpan Duration { get; private set; }

        public string DurationString => Duration.ToString(@"m\:ss");

        public DateTime AddedAt { get; private set; }

        public string AddedAtString => AddedAt.ToShortDateString();

        public string AddedBy { get; private set; }

        public bool Local { get; private set; }

        public bool Explicit { get; private set; }

        public int Popularity { get; private set; }

        public string[] Artists { get; private set; }

        public string ArtistsString => string.Join(", ", Artists);

        public string[] AvailableMarkets { get; private set; }

        public static Track Parse(JObject obj)
        {
            var trackObj = obj["track"];
            var result = new Track
            {
                Id = trackObj.Value<string>("id"),
                Name = trackObj.Value<string>("name"),
                Duration = TimeSpan.FromMilliseconds(trackObj.Value<int>("duration_ms")),
                AddedAt = DateTime.Parse(obj.Value<string>("added_at"), CultureInfo.InvariantCulture),
                AddedBy = obj["added_by"].Value<string>("id"),
                Local = trackObj.Value<bool>("is_local"),
                Explicit = trackObj.Value<bool>("explicit"),
                Popularity = trackObj.Value<int>("popularity"),
                Artists = (trackObj["artists"] as JArray)?.Select(artist => artist.Value<string>("name")).ToArray(),
                AvailableMarkets = (trackObj["available_markets"] as JArray)?.ToObject<string[]>()
            };
            return result;
        }

        public static Track ParseMinimal(JObject obj)
        {
            var result = new Track
            {
                Id = obj.Value<string>("id"),
                Name = obj.Value<string>("name"),
                Duration = TimeSpan.FromMilliseconds(obj.Value<int>("duration_ms")),
                Artists = (obj["artists"] as JArray)?.Select(artist => artist.Value<string>("name")).ToArray()
            };
            return result;
        }

    }
}

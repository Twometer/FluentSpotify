using FluentSpotify.Model;
using FluentSpotify.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class SearchApi
    {
        public async Task<IEnumerable<Track>> Search(string query, int limit = 20)
        {
            var obj = await Request.New("https://api.spotify.com/v1/search")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("q", query)
                .AddParameter("type", "track")
                .AddParameter("limit", limit.ToString())
                .Get();

            var array = JObject.Parse(obj)["tracks"]["items"] as JArray;
            return array?.Select(t => Track.ParseMinimal((JObject)t));
        }


    }
}

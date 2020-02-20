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
    public class PlaylistApi
    {

        public async Task<bool> IsFollowing(Playlist playlist)
        {
            var result = await Request.New($"https://api.spotify.com/v1/playlists/{playlist.Id}/followers/contains")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("ids", Spotify.Account.CurrentAccount.Id)
                .Get();

            return JArray.Parse(result).First.ToObject<bool>();
        }

    }
}

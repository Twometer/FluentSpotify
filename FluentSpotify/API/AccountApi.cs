using FluentSpotify.Model;
using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class AccountApi
    {
        public async Task<Account> GetAccount()
        {
            await Spotify.Auth.RefreshToken();

            var data = await Request.New("https://api.spotify.com/v1/me")
                .Authenticate("Bearer", Spotify.AccessToken)
                .Get();
            return Account.Parse(data);
        }

        public async Task<IEnumerable<Playlist>> GetPlaylists()
        {
            await Spotify.Auth.RefreshToken();

            var data = await Request.New("https://api.spotify.com/v1/me/playlists")
                .Authenticate("Bearer", Spotify.AccessToken)
                .GetPaged(obj => Playlist.Parse(obj))
                .All();

            return data;
        }

    }
}

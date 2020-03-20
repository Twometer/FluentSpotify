using FluentSpotify.Model;
using FluentSpotify.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class AccountApi
    {
        public Account CurrentAccount { get; private set; }

        public async Task<Account> GetAccount()
        {
            await Spotify.Auth.RefreshToken();

            var data = await Request.New("https://api.spotify.com/v1/me")
                .Authenticate("Bearer", Spotify.AccessToken)
                .Get();
            
            CurrentAccount = Account.Parse(data);
            return CurrentAccount;
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

        public async Task<IEnumerable<Device>> GetDevices()
        {
            await Spotify.Auth.RefreshToken();

            var data = await Request.New("https://api.spotify.com/v1/me/player/devices")
                .Authenticate("Bearer", Spotify.AccessToken)
                .Get();

            var devices = new List<Device>();
            var jArray = JObject.Parse(data)["devices"] as JArray;
            
            foreach (var item in jArray)
                devices.Add(Device.Parse((JObject) item));

            return devices;
        }

    }
}

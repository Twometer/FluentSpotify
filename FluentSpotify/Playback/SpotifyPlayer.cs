using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FluentSpotify.Playback
{
    public class SpotifyPlayer
    {
        private WebView container;

        private string playerId;

        public SpotifyPlayer(WebView container)
        {
            this.container = container;
        }

        public async Task Initialize()
        {
            await container.RunScript($"SetToken('{Spotify.AccessToken}');");
            await container.RunScript("Connect();");
            playerId = await container.RunScript("GetPlayerId();");
        }

        public async Task Pause()
        {
            await container.RunScript("window.player.pause();");
        }

        public async Task Play()
        {
            await container.RunScript("window.player.resume();");
        }

        public Task PlayTrack(Track track)
        {
            return PlayUri("spotify:track:" + track.Id);
        }

        public Task PlayPlaylist(Playlist playlist)
        {
            return PlayUri("spotify:playlist:" + playlist.Id);
        }

        public async Task Previous()
        {
            await container.RunScript("window.player.previousTrack();");
        }

        public async Task Next()
        {
            await container.RunScript("window.player.nextTrack();");
        }

        public async Task SetVolume(double vol)
        {
            await container.RunScript($"window.player.setVolume({vol.ToString(CultureInfo.InvariantCulture)});");
        }


        private async Task PlayUri(string uri)
        {
            var body = $"{{ \"uris\": [ \"{uri}\" ] }}";

            var request = WebRequest.CreateHttp("https://api.spotify.com/v1/me/player/play?device_id=" + playerId);
            request.Method = "PUT";
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + Spotify.AccessToken);
            request.ContentLength = Encoding.UTF8.GetByteCount(body);
            request.ContentType = "application/json";

            var reqStream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(reqStream))
            {
                writer.Write(body);
            }
            await request.GetResponseAsync();
        }

    }
}

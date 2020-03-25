using FluentSpotify.Playback;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class PlaybackApi
    {
        private ISpotifyPlayer _currentPlayer;
        public ISpotifyPlayer CurrentPlayer
        {
            get
            {
                return _currentPlayer;
            }
            set
            {
                UpdatePlayer(value);
                _currentPlayer = value;
            }
        }

        public LocalPlayer LocalPlayer { get; set; }

        public event EventHandler PlaybackStateChanged;

        public event EventHandler TrackPositionChanged;

        public event EventHandler PlayerChanged;

        public async Task TransferPlayback(string playerId)
        {
            await Spotify.Auth.RefreshToken();
            await SendTransferRequest(new
            {
                device_ids = new[] {playerId}
            });

            if (playerId == LocalPlayer.PlayerId)
                CurrentPlayer = LocalPlayer;
            else
                CurrentPlayer = new RemotePlayer(playerId);
        }

        private void UpdatePlayer(ISpotifyPlayer player)
        {
            if (CurrentPlayer is LocalPlayer oldLocalPlayer)
            {
                oldLocalPlayer.PlaybackStateChanged -= Handle_PlaybackStateChanged;
                oldLocalPlayer.TrackPositionChanged -= Handle_TrackPositionChanged;
            }
            if (player is LocalPlayer newLocalPlayer)
            {
                newLocalPlayer.PlaybackStateChanged += Handle_PlaybackStateChanged;
                newLocalPlayer.TrackPositionChanged += Handle_TrackPositionChanged;
            }
            PlayerChanged?.Invoke(this, new EventArgs());
        }

        private void Handle_TrackPositionChanged(object sender, EventArgs e)
        {
            TrackPositionChanged?.Invoke(sender, e);
        }

        private void Handle_PlaybackStateChanged(object sender, EventArgs e)
        {
            PlaybackStateChanged?.Invoke(sender, e);
        }

        private async Task SendTransferRequest(object obj)
        {
            var body = JsonConvert.SerializeObject(obj);
            try
            {
                var request = WebRequest.CreateHttp("https://api.spotify.com/v1/me/player");
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
            catch (WebException e)
            {
                e.PrintStackTrace("Transfer failed", body);
            }
        }
    }
}

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
using System.Threading;
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

        public PlaybackApi()
        {
            var updateThread = new Thread(UpdateWorker);
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        /// <summary>
        /// Asks the Spotify Web API to transfer playback to a different player
        /// </summary>
        /// <param name="playerId">The player/device id to transfer to</param>
        public async Task TransferPlayback(string playerId)
        {
            Log.Info($"Transferring playback to {playerId}...");
            await Spotify.Auth.RefreshToken();
            await SendTransferRequest(new
            {
                device_ids = new[] { playerId }
            });

            if (playerId == LocalPlayer.PlayerId)
                CurrentPlayer = LocalPlayer;
            else
                CurrentPlayer = new RemotePlayer(playerId);
            await CurrentPlayer.Initialize();
        }

        /// <summary>
        /// Updates the current player to a new player. This mainly updates the event handlers
        /// to the correct player.
        /// 
        /// Note: Must be called BEFORE updating
        /// </summary>
        /// <param name="player">The new player.</param>
        private void UpdatePlayer(ISpotifyPlayer player)
        {
            Log.Info($"Updating player to {player.GetType().Name}");
            if (CurrentPlayer != null)
            {
                CurrentPlayer.PlaybackStateChanged -= Handle_PlaybackStateChanged;
                CurrentPlayer.TrackPositionChanged -= Handle_TrackPositionChanged;
            }
            player.PlaybackStateChanged += Handle_PlaybackStateChanged;
            player.TrackPositionChanged += Handle_TrackPositionChanged;
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

        private void UpdateWorker()
        {
            while (true)
            {
                CurrentPlayer?.Update();
                Thread.Sleep(1000);
            }
        }
    }
}

using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Playback
{
    public class RemotePlayer : ISpotifyPlayer
    {
        public bool IsPlaying { get; private set; }

        public Track CurrentTrack { get; private set; }

        public int Position { get; private set; }

        public event EventHandler PlaybackStateChanged;

        public event EventHandler TrackPositionChanged;

        private readonly string playerId;

        private int updateCounter;

        public RemotePlayer(string playerId)
        {
            this.playerId = playerId;
        }

        public async Task Initialize()
        {
            await ReloadPlaybackState();
        }

        public async Task Update()
        {
            if (IsPlaying)
            {
                Position += 1000;
                TrackPositionChanged?.Invoke(this, new EventArgs());
            }

            var pollIntervalElapsed = updateCounter % 15 == 0; // Poll every 15 seconds
            if (Position >= CurrentTrack.Duration.TotalMilliseconds || pollIntervalElapsed)
            {
                await ReloadPlaybackState();
            }

            updateCounter++;
        }

        public async Task Next()
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/next")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task Pause()
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/pause")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
            IsPlaying = false;
            TrackPositionChanged?.Invoke(this, new EventArgs());
        }

        public async Task Play()
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/play")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
            IsPlaying = true;
            TrackPositionChanged?.Invoke(this, new EventArgs());
        }

        public Task PlayPlaylist(Playlist playlist)
        {
            return SendPlayRequest(new PlayListRequest(playlist.Uri));
        }

        public Task PlayTrack(Track track)
        {
            return SendPlayRequest(new PlayTrackRequest(new List<string>() { track.Uri }));
        }

        public Task PlayTrack(Playlist context, int trackIdx)
        {
            return SendPlayRequest(new PlayListItemRequest(context.Uri, new OffsetWrapper(trackIdx)));
        }

        public async Task Previous()
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/previous")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task Seek(int positionMs)
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/next")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("position_ms", positionMs.ToString())
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetRepeat(RepeatMode mode)
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/repeat")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("state", mode.ToString().ToLower())
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetShuffle(bool shuffle)
        {
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/shuffle")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("state", shuffle ? "true" : "false")
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetVolume(double vol)
        {
            var volPercent = vol * 100.0d;
            await Spotify.Auth.RefreshToken();
            await Request.New("https://api.spotify.com/v1/me/player/pause")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("volume_percent", volPercent.ToString())
                .AddParameter("device_id", playerId)
                .Put();
        }

        public Task TogglePlayback()
        {
            if (IsPlaying)
                return Pause();
            else return Play();
        }

        private async Task ReloadPlaybackState()
        {
            await Spotify.Auth.RefreshToken();
            var state = await Request.New("https://api.spotify.com/v1/me/player")
                .Authenticate("Bearer", Spotify.AccessToken)
                .Get();

            var obj = JObject.Parse(state);
            var item = obj["item"] as JObject;
            CurrentTrack = Track.ParseFree(item);
            IsPlaying = obj.Value<bool>("is_playing");
            Position = obj.Value<int>("progress_ms");
            PlaybackStateChanged?.Invoke(this, new EventArgs());
            TrackPositionChanged?.Invoke(this, new EventArgs());
        }

        /* Playback requests */

        private async Task SendPlayRequest(object obj)
        {
            await Spotify.Auth.RefreshToken();
            var body = JsonConvert.SerializeObject(obj);
            try
            {
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
            catch (WebException e)
            {
                e.PrintStackTrace("Playback failed", body);
            }
        }



        private class PlayListRequest
        {
            [JsonProperty("context_uri")]
            public string ContextUri { get; set; }

            public PlayListRequest(string contextUri)
            {
                ContextUri = contextUri;
            }
        }

        private class PlayListItemRequest : PlayListRequest
        {
            [JsonProperty("offset")]
            public OffsetWrapper Offset { get; set; }

            public PlayListItemRequest(string contextUri, OffsetWrapper offset) : base(contextUri)
            {
                Offset = offset;
            }
        }

        private class PlayTrackRequest
        {
            [JsonProperty("uris")]
            public List<string> Uris { get; }

            public PlayTrackRequest(List<string> uris)
            {
                Uris = uris;
            }
        }

        private class OffsetWrapper
        {
            [JsonProperty("position")]
            public int Position { get; set; }

            public OffsetWrapper(int position)
            {
                Position = position;
            }
        }
    }
}

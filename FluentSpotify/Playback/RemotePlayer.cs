using FluentSpotify.API;
using FluentSpotify.Model;
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

namespace FluentSpotify.Playback
{
    public class RemotePlayer : ISpotifyPlayer
    {
        public bool IsPlaying => throw new NotImplementedException();

        public Track CurrentTrack => throw new NotImplementedException();

        public int Position => throw new NotImplementedException();

        private readonly string playerId;

        public RemotePlayer(string playerId)
        {
            this.playerId = playerId;
        }

        public Task Initialize()
        {
            // Remote players don't need initialization
            return Task.CompletedTask;
        }

        public async Task Next()
        {
            await Request.New("https://api.spotify.com/v1/me/player/next")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task Pause()
        {
            await Request.New("https://api.spotify.com/v1/me/player/pause")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task Play()
        {
            await Request.New("https://api.spotify.com/v1/me/player/play")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
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
            await Request.New("https://api.spotify.com/v1/me/player/previous")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task Seek(int positionMs)
        {
            await Request.New("https://api.spotify.com/v1/me/player/next")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("position_ms", positionMs.ToString())
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetRepeat(RepeatMode mode)
        {
            await Request.New("https://api.spotify.com/v1/me/player/repeat")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("state", mode.ToString().ToLower())
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetShuffle(bool shuffle)
        {
            await Request.New("https://api.spotify.com/v1/me/player/shuffle")
                .Authenticate("Bearer", Spotify.AccessToken)
                .AddParameter("state", shuffle ? "true" : "false")
                .AddParameter("device_id", playerId)
                .Put();
        }

        public async Task SetVolume(double vol)
        {
            var volPercent = vol * 100.0d;
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

        /* Playback requests */

        private async Task SendPlayRequest(object obj)
        {
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

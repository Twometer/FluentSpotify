using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public bool IsPlaying { get; private set; }

        public Track CurrentTrack { get; private set; }

        public int Position { get; private set; }

        public event EventHandler PlaybackStateChanged;

        public event EventHandler TrackPositionChanged;


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
            return SendPlayRequest(new PlayTrackRequest(new List<string>() { track.Uri }));
        }

        public Task PlayTrack(Playlist context, int trackIdx)
        {
            return SendPlayRequest(new PlayListItemRequest(context.Uri, new OffsetWrapper(trackIdx)));
        }

        public Task PlayPlaylist(Playlist playlist)
        {
            return SendPlayRequest(new PlayListRequest(playlist.Uri));
        }

        public async Task Previous()
        {
            await container.RunScript("window.player.previousTrack();");
        }

        public async Task Next()
        {
            await container.RunScript("window.player.nextTrack();");
        }

        public async Task TogglePlayback()
        {
            await container.RunScript("window.player.togglePlay();");
            PlaybackStateChanged.Invoke(this, new EventArgs());
        }

        public async Task SetVolume(double vol)
        {
            await container.RunScript($"window.player.setVolume({vol.ToString(CultureInfo.InvariantCulture)});");
        }

        public void HandleEvent(JObject eventObj)
        {
            var eventType = eventObj.Value<string>("eventType");
            var eventData = eventObj["eventData"] as JObject;

            switch (eventType)
            {
                case "state_change":
                    HandleStateChange(eventData);
                    break;
                case "position_change":
                    HandlePositionChange(eventData);
                    break;
            }
        }

        private void HandlePositionChange(JObject data)
        {
            Position = data.Value<int>("position");
            TrackPositionChanged.Invoke(this, new EventArgs());
        }

        private void HandleStateChange(JObject data)
        {
            if (data.Type == JTokenType.Null)
            {
                IsPlaying = false;
                CurrentTrack = null;
            }
            else
            {
                IsPlaying = true;
                CurrentTrack = Track.ParseMinimal(data);
            }

            PlaybackStateChanged.Invoke(this, new EventArgs());
        }

        private async Task SendPlayRequest(object obj)
        {
            var body = JsonConvert.SerializeObject(obj);
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

using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool IsPlaying => CurrentTrack != null && !isSoftwarePause;

        public Track CurrentTrack { get; private set; }

        public int Position { get; private set; }

        public event EventHandler PlaybackStateChanged;

        public event EventHandler TrackPositionChanged;

        private bool isSoftwarePause;

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

        public async Task Seek(int positionMs)
        {
            await container.RunScript($"window.player.seek({positionMs});");
        }

        /* public Task PlayCollection()
        {
            return SendPlayRequest(new PlayListRequest($"spotify:user:{Spotify.Account.CurrentAccount.Id}:collection"));
        }

        public Task PlayCollectionTrack(int trackId)
        {
            return SendPlayRequest(new PlayListItemRequest($"spotify:user:{Spotify.Account.CurrentAccount.Id}:collection", new OffsetWrapper(trackId)));
        } */

        public async Task PlayCollection()
        {
            // TODO For the internal API we require an open.spotify.com AccessToken
            try
            {
                var url = $"https://gew-spclient.spotify.com/connect-state/v1/player/command/from/{playerId}/to/{playerId}";
                var accountId = Spotify.Account.CurrentAccount.Id;
                var payload = "{\"command\":{\"context\":{\"uri\":\"spotify:user:" + accountId + ":collection\",\"url\":\"context://spotify:user:" + accountId + ":collection\",\"metadata\":{}},\"play_origin\":{\"feature_identifier\":\"harmony\",\"feature_version\":\"3.23.0-a0f8ef4\"},\"options\":{\"skip_to\":{\"track_index\":0},\"license\":\"premium\",\"player_options_override\":{\"repeating_track\":false,\"repeating_context\":false}},\"endpoint\":\"play\"}}";

                var request = WebRequest.CreateHttp(url);
                request.Method = "POST";
                request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + Spotify.AccessToken);
                request.ContentLength = Encoding.UTF8.GetByteCount(payload);
                request.ContentType = "application/json";
                request.Headers.Add("Origin", "https://open.spotify.com");
                request.Referer = "https://open.spotify.com/collection/tracks";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36";

                var reqStream = await request.GetRequestStreamAsync();
                using (var writer = new StreamWriter(reqStream))
                {
                    writer.Write(payload);
                }
                var resp = await request.GetResponseAsync();

                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    Debug.WriteLine(await reader.ReadToEndAsync());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                throw e;
            }
        }

        public async Task SetShuffle(bool shuffle)
        {
            await Request.New("https://api.spotify.com/v1/me/player/shuffle")
                .AddParameter("state", shuffle ? "true" : "false")
                .Put();
        }

        public async Task SetRepeat(RepeatMode mode)
        {
            await Request.New("https://api.spotify.com/v1/me/player/repeat")
                .AddParameter("state", mode.ToString().ToLower())
                .Put();
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
            await container.RunScript("TogglePlay();");
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
                case "status_update":
                    HandleStatusUpdate(eventData);
                    break;
            }
        }

        private void HandleStatusUpdate(JObject data)
        {
            Position = data.Value<int>("position");
            isSoftwarePause = data.Value<bool>("paused");
            TrackPositionChanged.Invoke(this, new EventArgs());
        }

        private void HandleStateChange(JObject data)
        {
            if (data.Type == JTokenType.Null)
            {
                CurrentTrack = null;
            }
            else
            {
                CurrentTrack = Track.ParseMinimal(data);
            }

            PlaybackStateChanged.Invoke(this, new EventArgs());
        }

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

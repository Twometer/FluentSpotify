using FluentSpotify.Playback;
using FluentSpotify.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public static class Spotify
    {
        public static AuthApi Auth { get; } = new AuthApi();

        public static AccountApi Account { get; } = new AccountApi();

        public static TracksApi Tracks { get; } = new TracksApi();

        public static LocalPlayer Playback { get; set; }

        public static SearchApi Search { get; } = new SearchApi();

        public static PlaylistApi Playlist { get; } = new PlaylistApi();

        public static string AccessToken => Auth.KeyStore.AccessToken;


        public static void Initialize()
        {
            Auth.Initialize();
        }

    }
}

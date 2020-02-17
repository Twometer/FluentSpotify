using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class Spotify
    {
        public const string ClientId = "21fc82995c544b219555fca9002378f9";

        public const string ClientSecret = "6fb896615e4d4eab85e75edae2e4a00d";

        public const string CallbackUrl = "x-fluent-sp://oauth_callback";

        public static Spotify Instance { get; } = new Spotify();


        public string BuildAuthUrl()
        {
            // Request access to almost all scopes so that we can provide
            // a fully featured Spotify experience
            var scopes = string.Join(' ',
                "streaming",
                "ugc-image-upload",
                "user-read-playback-state",
                "playlist-read-collaborative",
                "user-modify-playback-state",
                "playlist-modify-public",
                "user-library-modify",
                "user-top-read",
                "user-read-currently-playing",
                "playlist-read-private",
                "user-follow-read",
                "user-read-recently-played",
                "playlist-modify-private",
                "user-follow-modify",
                "user-library-read");

            return Request.New("https://accounts.spotify.com/authorize")
                .AddParameter("client_id", ClientId)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", CallbackUrl)
                .AddParameter("scope", scopes)
                .Build();
        }

    }
}

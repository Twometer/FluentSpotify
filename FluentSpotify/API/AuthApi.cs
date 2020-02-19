using FluentSpotify.Util;
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
    public class AuthApi
    {
        public const string ClientId = "21fc82995c544b219555fca9002378f9";

        public const string ClientSecret = "6fb896615e4d4eab85e75edae2e4a00d";

        public const string CallbackUrl = "http://x-fluent-sp/oauth_callback/";

        public KeyStore KeyStore { get; private set; } = new KeyStore();

        public string BeginAuth()
        {
            // Request access to almost all scopes so that we can provide
            // a fully featured Spotify experience
            var scopes = string.Join(' ', 
                "streaming", 
                "user-top-read",
                "user-read-email", 
                "user-read-private",
                "user-read-currently-playing",
                "user-read-recently-played",
                "user-read-playback-state", 
                "user-modify-playback-state",
                "user-library-read",
                "user-library-modify", 
                "user-follow-modify", 
                "user-follow-read",
                "playlist-read-private",
                "playlist-read-collaborative", 
                "playlist-modify-public",
                "playlist-modify-private", 
                "ugc-image-upload"
            );

            return Request.New("https://accounts.spotify.com/authorize")
                .AddParameter("client_id", ClientId)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", CallbackUrl)
                .AddParameter("scope", scopes)
                .ToUrl();
        }

        public async Task FinishAuth(Uri callbackUri)
        {
            if (callbackUri == null)
                throw new ArgumentNullException($"{nameof(callbackUri)} cannot be null");

            var query = Query.Parse(callbackUri.Query);
            KeyStore.AuthorizationCode = query["code"];

            var authData = await Request.New("https://accounts.spotify.com/api/token")
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("code", KeyStore.AuthorizationCode)
                .AddParameter("redirect_uri", CallbackUrl)
                .Authenticate("Basic", $"{ClientId}:{ClientSecret}".ToBase64())
                .Post();

            var obj = JObject.Parse(authData);

            KeyStore.AccessToken = obj.Value<string>("access_token");
            KeyStore.RefreshToken = obj.Value<string>("refresh_token");

            var expiresIn = obj.Value<int>("expires_in");
            KeyStore.AccessTokenExpiry = DateTime.Now.AddSeconds(expiresIn);
            SaveKeystore();
        }

        public async Task RefreshToken()
        {
            if (!KeyStore.Expired)
                return;

            var authData = await Request.New("https://accounts.spotify.com/api/token")
                .AddParameter("grant_type", "refresh_token")
                .AddParameter("refresh_token", KeyStore.RefreshToken)
                .AddParameter("redirect_uri", CallbackUrl)
                .Authenticate("Basic", $"{ClientId}:{ClientSecret}".ToBase64())
                .Post();

            var obj = JObject.Parse(authData);

            KeyStore.AccessToken = obj.Value<string>("access_token");

            var expiresIn = obj.Value<int>("expires_in");
            KeyStore.AccessTokenExpiry = DateTime.Now.AddSeconds(expiresIn);
            SaveKeystore();
        }

        private void SaveKeystore()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["keyStore"] = JsonConvert.SerializeObject(KeyStore);
        }

        public void Initialize()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = (string)settings.Values["keyStore"];
            if (value == null) return;

            KeyStore = JsonConvert.DeserializeObject<KeyStore>(value);
        }

        public void Logout()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["keyStore"] = null;
        }

    }
}

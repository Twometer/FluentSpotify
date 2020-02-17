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
    public class Spotify
    {
        public const string ClientId = "21fc82995c544b219555fca9002378f9";

        public const string ClientSecret = "6fb896615e4d4eab85e75edae2e4a00d";

        public const string CallbackUrl = "http://x-fluent-sp/oauth_callback/";

        public static Spotify Instance { get; } = new Spotify();

        public KeyStore KeyStore { get; private set; } = new KeyStore();

        public void Initialize()
        {
            LoadKeystore();
        }

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
                .ToUrl();
        }

        public async Task ContinueLogin(Uri uri)
        {
            var query = Query.Parse(uri.Query);
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

        public async Task<Account> GetAccountProperties()
        {
            await RefreshToken();

            var data = await Request.New("https://api.spotify.com/v1/me")
                .Authenticate("Bearer", KeyStore.AccessToken)
                .Get();
            return Account.Parse(data);
        }

        private void SaveKeystore()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            settings.Values["keyStore"] = JsonConvert.SerializeObject(KeyStore);
        }

        private void LoadKeystore()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var value = (string)settings.Values["keyStore"];
            if (value == null) return;

            KeyStore = JsonConvert.DeserializeObject<KeyStore>(value);
        }

    }
}

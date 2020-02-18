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
        public static AuthApi Auth { get; } = new AuthApi();

        public static AccountApi Account { get; } = new AccountApi();

        public static string AccessToken => Auth.KeyStore.AccessToken;


        public static void Initialize()
        {
            Auth.Initialize();
        }

    }
}

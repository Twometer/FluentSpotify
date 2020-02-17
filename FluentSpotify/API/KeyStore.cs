using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class KeyStore
    {
        public string AuthorizationCode { get; set; }

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiry { get; set; }

        public string RefreshToken { get; set; }

        public bool Expired => DateTime.Now > AccessTokenExpiry;

        public bool Authenticated => !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);
    }
}

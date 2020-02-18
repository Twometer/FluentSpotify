using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class Account
    {
        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public string ImageUrl { get; private set; }

        public int Followers { get; private set; }

        public static Account Parse(string json)
        {
            var jObject = JObject.Parse(json);
            var account = new Account
            {
                Id = jObject.Value<string>("id"),
                DisplayName = jObject.Value<string>("display_name"),
                ImageUrl = jObject["images"].First?.Value<string>("url"),
                Followers = jObject["followers"].Value<int>("total")
            };
            return account;
        }
    }
}

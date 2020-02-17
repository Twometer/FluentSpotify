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
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string ImageUrl { get; set; }

        public int Followers { get; set; }

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

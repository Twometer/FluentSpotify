using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Web
{
    public class Request
    {
        private string endpoint;

        private readonly IDictionary<string, string> queryParameters;

        private Request(string endpoint)
        {
            this.endpoint = endpoint;
            this.queryParameters = new Dictionary<string, string>();
        }

        public static Request New(string endpoint)
        {
            var normalized = new Uri(endpoint).AbsoluteUri.ToString().Trim('/');
            return new Request(normalized);
        }

        public Request AddParameter(string key, string val)
        {
            queryParameters.Add(key, val);
            return this;
        }

        public string Build()
        {
            if (queryParameters.Count > 0)
            {
                endpoint += "?";
                var first = true;
                foreach (var param in queryParameters)
                {
                    var key = WebUtility.UrlEncode(param.Key);
                    var val = WebUtility.UrlEncode(param.Value);

                    if (first) first = false;
                    else endpoint += "&";


                    endpoint += $"{key}={val}";
                }
            }
            return endpoint;
        }

        public async Task<string> Send()
        {
            Build();

            var request = WebRequest.CreateHttp(endpoint);
            var response = await request.GetResponseAsync();
            var stream = response.GetResponseStream();
            
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

    }
}

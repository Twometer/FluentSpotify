using Newtonsoft.Json.Linq;
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

        private string query;

        private string authorization;

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

        public Request Authenticate(string scheme, string val)
        {
            this.authorization = scheme + " " + val;
            return this;
        }

        public Request AddParameter(string key, string val)
        {
            queryParameters.Add(key, val);
            return this;
        }

        public string ToUrl()
        {
            BuildQuery();

            if (string.IsNullOrEmpty(query))
                return endpoint;
            else
                return endpoint + "?" + query;
        }

        public async Task<IEnumerable<T>> GetPaged<T>(Func<JObject, T> mapper)
        {
            var ret = new List<T>();
            while (true)
            {
                var obj = JObject.Parse(await Get());
                var arr = obj["items"] as JArray;

                foreach (var item in arr)
                    ret.Add(mapper(item as JObject));

                var next = obj.Value<string>("next");
                endpoint = next;
                if (string.IsNullOrEmpty(next))
                    break;
            }
            return ret;
        }

        public async Task<string> Get()
        {
            var request = BuildRequest(ToUrl());
            var response = await request.GetResponseAsync();
            var stream = response.GetResponseStream();

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<string> Post()
        {
            BuildQuery();

            var request = BuildRequest(endpoint);
            request.Method = "POST";
            request.ContentLength = Encoding.UTF8.GetByteCount(query);
            request.ContentType = "application/x-www-form-urlencoded";

            var reqStream = await request.GetRequestStreamAsync();
            using (var writer = new StreamWriter(reqStream))
            {
                writer.Write(query);
            }

            var response = await request.GetResponseAsync();
            var respStream = response.GetResponseStream();
            using (var reader = new StreamReader(respStream))
            {
                return reader.ReadToEnd();
            }
        }

        private HttpWebRequest BuildRequest(string url)
        {
            var req = WebRequest.CreateHttp(url);
            req.UserAgent = "FluentSpotify/1.00";
            if (!string.IsNullOrEmpty(authorization))
                req.Headers.Add(HttpRequestHeader.Authorization, authorization);
            return req;
        }

        private void BuildQuery()
        {
            var first = true;
            query = string.Empty;
            foreach (var param in queryParameters)
            {
                var key = WebUtility.UrlEncode(param.Key);
                var val = WebUtility.UrlEncode(param.Value);

                if (first) first = false;
                else query += "&";


                query += $"{key}={val}";
            }
        }
    }
}

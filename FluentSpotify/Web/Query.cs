using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Web
{
    public class Query
    {
        private IDictionary<string, string> values = new Dictionary<string, string>();

        private Query()
        {
        }

        public static Query Parse(string query)
        {
            var result = new Query();

            if (query.StartsWith("?"))
                query = query.Substring(1);

            foreach(var param in query.Split("&", StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = param.Split('=');
                var key = WebUtility.UrlDecode(parts[0]);
                var val = WebUtility.UrlDecode(parts[1]);

                result.values[key] = val;
            }
            return result;
        }

        public string this[string key]
        {
            get
            {
                return values[key];
            }
        }

    }
}

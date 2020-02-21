using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Web
{
    public class PagedRequest<T>
    {
        private string next;

        private string auth;

        private Func<JObject, T> mapper;

        public int Total { get; private set; }

        public PagedRequest(string endpoint, string auth, Func<JObject, T> mapper)
        {
            this.next = endpoint;
            this.auth = auth;
            this.mapper = mapper;
        }

        public bool HasNext => !string.IsNullOrEmpty(next);

        public async Task<IEnumerable<T>> Next()
        {
            var ret = new List<T>();
            var obj = JObject.Parse(await Get());
            var arr = obj["items"] as JArray;

            foreach (var item in arr)
                ret.Add(mapper(item as JObject));

            next = obj.Value<string>("next");
            Total = obj.Value<int>("total");
            return ret;
        }

        public async Task<IEnumerable<T>> All()
        {
            var ret = new List<T>();
            while (HasNext)
            {
                ret.AddRange(await Next());
            }
            return ret;
        }

        private Task<string> Get()
        {
            return Request.New(next)
                .Authenticate(auth)
                .Get();
        }

    }
}

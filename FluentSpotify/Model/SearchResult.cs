using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Model
{
    public class SearchResult
    {
        private object obj;

        public ObjectType Type { get; private set; }

        public SearchResult(object obj, ObjectType type)
        {
            this.obj = obj;
            Type = type;
        }

        public T Get<T>()
        {
            return (T)obj;
        }
    }
}

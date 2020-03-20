using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Model
{
    public class Device
    {
        public string Id { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsPrivateSession { get; private set; }

        public bool IsRestricted { get; private set; }

        public string Name { get; private set; }

        public DeviceType Type { get; private set; }

        public int Volume { get; private set; }

        public static Device Parse(JObject json)
        {
            return new Device()
            {
                Id = json.Value<string>("id"),
                IsActive = json.Value<bool>("is_active"),
                IsPrivateSession = json.Value<bool>("is_private_session"),
                IsRestricted = json.Value<bool>("is_restricted"),
                Name = json.Value<string>("name"),
                Type = Enum.Parse<DeviceType>(json.Value<string>("type")),
                Volume = json.Value<int>("volume_percent")
            };
        }

    }
}

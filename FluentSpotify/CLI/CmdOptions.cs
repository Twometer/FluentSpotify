using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.CLI
{
    public class CmdOptions
    {
        [Option('s', "server", Required = false)]
        public bool UseServer { get; set; }

        [Option('p', "playermode", Required = false)]
        public bool PlayerMode { get; set; }

        [Option('i', "playerid", Required = false)]
        public string PlayerId { get; set; }

    }
}

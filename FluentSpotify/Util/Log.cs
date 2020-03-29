using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Util
{
    public class Log
    {
        private static void WriteMessage(string tag, string msg)
        {
            var now = DateTime.Now;
            var message = $"[{now.ToShortDateString()} {now.ToShortTimeString()}] [{tag}] {msg}";
            if (Debugger.IsAttached)
                Debug.WriteLine(message);
            else
                Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            WriteMessage("ERROR", message);
        }

        public static void Error(string message, Exception e)
        {
            WriteMessage("ERROR", message + ": " + e.Message);
        }

        public static void Error(Exception e)
        {
            WriteMessage("ERROR", e.Message);
        }

        public static void Info(string message)
        {
            WriteMessage("INFO", message);
        }

        public static void Warn(string message)
        {
            WriteMessage("WARN", message);
        }

    }
}

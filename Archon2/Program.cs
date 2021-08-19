using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace Archon2
{
    public class Program
    {
        public static JObject Config { get; private set; }

        static void Main(string[] args)
        {
            if (args is null || args.Length == 0 || !Directory.Exists(args[0]))
            {
                Console.WriteLine("Invalid runtime path.");
                Environment.Exit(1);
            }

            Directory.SetCurrentDirectory(args[0]);
            try
            {
                Config = JObject.Parse(File.ReadAllText("config.json"));
            }
            catch
            {
                Console.WriteLine("Invalid config. Either the file doesn't exist, or the file isn't valid json.");
            }

            new BotManager().Run().GetAwaiter().GetResult();
        }
    }
}

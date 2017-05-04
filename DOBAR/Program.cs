using System;
using System.Linq;
using DOBAR.Core;

class Program
{
    static void Main(string[] args)
    {
        Configuration config = null;
        // first start up - configuration time?
        var configDb = BotCore.Instance.Database.GetCollection<Configuration>("configuration");
        var configs = configDb.FindAll();
        if (configs.Count() == 0)
        {
            Console.WriteLine("Couldn't find a valid configuration. Configuration time!");
            Console.WriteLine("Choose a service bot configuration name: ");
            var name = Console.ReadLine();

            Console.WriteLine("Enter the discord token the bot is listening to:");
            var token = Console.ReadLine();

            Console.WriteLine("Set a valid v5^dev API key:");
            var apiKey = Console.ReadLine();

            Console.Write("\r\nThat's all i need for now, writing configuration file..");

            config = new Configuration
            {
                Token = token,
                FriendlyName = name,
                V5ApiKey = apiKey
            };

            configDb.Insert(config);
            configDb.EnsureIndex(x => x.Token);

            Console.Write(". done!\r\n");
        } // do we have an existing configuration file already?
        else if (configs.Count() == 1)
        {
            config = configs.FirstOrDefault();
        }
        else //do we have multiple configurations? let the user choose
        {
            Console.WriteLine("Discovered " + configs.Count() + " configurations:");
            for (var i = 0; i < configs.Count(); i++)
                Console.WriteLine($"{i}:: { configs.ElementAt(i).FriendlyName}");

            Console.WriteLine("");
            Console.WriteLine("Please choose a configuration by number: ");
            var index = Convert.ToInt32(Console.ReadLine());
            config = configs.ElementAt(index);
        }


        // now start the bot
        BotCore.Instance.LaunchAsync(config);

        var exit = false;

        while (!exit)
        {
            var cmd = Console.ReadLine();

            switch (cmd)
            {
                case "stop":
                case "exit":
                    BotCore.Instance.DisconnectAsync();
                    exit = true;
                    break;
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
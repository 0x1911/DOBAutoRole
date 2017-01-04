﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DOB_AutoRole.Core;

class Program
{
    static void Main(string[] args)
    {
        // doing the configuration stuff
        Configuration config = null;

        var configDb = BotCore.Instance.Database.GetCollection<Configuration>("configuration");
        var configs = configDb.FindAll();

        if (configs.Count() == 0)
        {
            Console.WriteLine("Seems like this is your first start up. Let us configure your bot.");
            Console.WriteLine("Please give your bot configuration a name: ");
            var name = Console.ReadLine();

            Console.WriteLine("Ok, now lets get serious. On which discord bot token does your nep listen? ");
            var token = Console.ReadLine();

            Console.WriteLine("Ok, I got it.");

            config = new Configuration
            {
                Token = token,
                FriendlyName = name
            };

            configDb.Insert(config);
            configDb.EnsureIndex(x => x.Token);
        }
        else if (configs.Count() == 1)
        {
            config = configs.FirstOrDefault();
        }
        else
        {
            Console.WriteLine("Multiple configurations found.");
            for (var i = 0; i < configs.Count(); i++)
                Console.WriteLine($"{i}: { configs.ElementAt(i).FriendlyName}");

            Console.WriteLine("");
            Console.WriteLine("Please choose a configuration by number: ");
            var index = Convert.ToInt32(Console.ReadLine());
            config = configs.ElementAt(index);
        }


        // now start the bot
        BotCore.Instance.LaunchAsync(config.Token);

        var exit = false;

        while (!exit)
        {
            var cmd = Console.ReadLine();

            switch (cmd)
            {
                case "stop":
                case "exit":
                    Task.Run(() => BotCore.Instance.DisconnectAsync()).ContinueWith(_ => exit = true);
                    break;
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchLib;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Api.Models.v5.Users;

namespace TwitchBot
{
    class Program
    {
        static void Main(string[] args)
        {
            string shutdown = "";

            ChatBot bot = new ChatBot();
            bot.Connect();

            do
            {
                shutdown = Console.ReadLine();

                if (bot.GetClient().IsConnected == false)
                {
                    bot.Connect();
                }

            } while (shutdown != "exit" || shutdown != "Exit" || shutdown != "quit" || shutdown != "Quit");

            Console.WriteLine("You shouldn't be here. Something broke");

            bot.Disconnect();
        }
    }
}

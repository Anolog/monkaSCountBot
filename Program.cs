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
            ChatBot bot = new ChatBot();
            bot.Connect();

            while (true)
            {
                Console.ReadLine();
                if (bot.GetClient().IsConnected == false)
                {
                    bot.Connect();
                }

            }
            
            bot.Disconnect();
        }
    }
}

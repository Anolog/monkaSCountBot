using System;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Api.Models.v5.Users;
using System.Globalization;

namespace TwitchBot
{
    internal class ChatBot
    {
        readonly ConnectionCredentials m_Credentials = new ConnectionCredentials(TwitchInfo.BotUsername, TwitchInfo.BotToken);
        TwitchClient m_Client;

        static float m_CurrentMonkaCount;
        int m_MaxMonkaS = 10;
        int m_MaxTriHard = 10;
        float m_MonkaWorth = 1.0f;
        float m_TriHardWorth = 3.0f;

        public ChatBot()
        {
            m_CurrentMonkaCount = 250;
        }

        internal void Connect()
        {
            Console.WriteLine("Connecting");

            m_Client = new TwitchClient();
            m_Client.Initialize(m_Credentials, TwitchInfo.ChannelName);
            m_Client.ChatThrottler = new TwitchLib.Client.Services.MessageThrottler(m_Client, 100, TimeSpan.FromSeconds(30));
            m_Client.ChatThrottler.ApplyThrottlingToRawMessages = true;
            m_Client.ChatThrottler.StartQueue();

            m_Client.OnLog += Client_OnLog;
            m_Client.OnConnected += Client_OnConnect;
            m_Client.OnConnectionError += Client_OnConnectionError;
            m_Client.OnMessageReceived += Client_OnMessageRecieved;

            m_Client.Connect();

        }

        private void Client_OnConnect(object sender, OnConnectedArgs e)
        {
            Console.WriteLine("Started Bot");

            //m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Enabled! There are 250 monkaS remaining. DON'T OVERUSE! monkaS = -1, monkaOMEGA = -2, TriHard = +1, triGOLD = +3");
            m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Enabled! There are 250 monkaS remaining. DON'T OVERUSE! monkaS = -1, TriHard = +3");

        }

        private void Client_OnMessageRecieved(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine("Debug: Throttle: " + m_Client.ChatThrottler.PendingSendCount);

            if (m_Client.ChatThrottler.PendingSendCount > 5 && m_CurrentMonkaCount > 0)
            {
                m_Client.ChatThrottler.Clear();
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaMonkaSWorth") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaMonkaSWorth"))
            {
                //val = (float)Convert.ToDouble(chatMessage[1]);
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    float val;

                    val = (float)Convert.ToDouble(chatMessage[1]);

                    if (val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to change worth to negative");
                        m_Client.SendWhisper(e.ChatMessage.Username, "You cannot use a negative number.");
                        return;
                    }

                    if (val > Int32.MaxValue)
                    {
                        Console.Write("Moderator " + e.ChatMessage.Username + " tried to give a value higher than max int");
                        m_Client.SendWhisper(e.ChatMessage.Username, "Total monkaS value exceeds ~2.1 billion, integer error.");
                        return;
                    }

                    Console.WriteLine("monkaS are now worth " + val);
                    m_MonkaWorth = val;

                    //m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has changed monkaS to be worth " + m_MonkaWorth + "!");
                    return;

                }
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaTriHardWorth") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaTriHardWorth"))
            {
                //val = (float)Convert.ToDouble(chatMessage[1]);
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    float val;

                    val = (float)Convert.ToDouble(chatMessage[1]);

                    if (val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to change worth to negative");
                        m_Client.SendWhisper(e.ChatMessage.Username, "You cannot use a negative number.");
                        return;
                    }

                    if (val > Int32.MaxValue)
                    {
                        Console.Write("Moderator " + e.ChatMessage.Username + " tried to give a value higher than max int");
                        m_Client.SendWhisper(e.ChatMessage.Username, "Total monkaS value exceeds ~2.1 billion, integer error.");
                        return;
                    }

                    Console.WriteLine("TriHard are now worth " + val);
                    m_TriHardWorth = val;

                    //m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has changed TriHard to be worth " + m_TriHardWorth + "!");
                    return;

                }
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaTriHardAmount") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaTriHardAmount"))
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    int val;

                    if (Int32.TryParse(chatMessage[1], out val))
                    {
                        if (val < 0)
                        {
                            Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to set the spam amount to below 0");
                            m_Client.SendWhisper(e.ChatMessage.Username, "You cannot set the limit below 0. cmonBruh");
                            return;
                        }

                        if (val > Int32.MaxValue)
                        {
                            Console.Write("Moderator " + e.ChatMessage.Username + " tried to give a value higher than max int");
                            m_Client.SendWhisper(e.ChatMessage.Username, "value exceeds ~2.1 billion, integer error.");
                            return;
                        }

                        Console.WriteLine("TriHard amount increased by mod: " + e.ChatMessage.Username + " to value: " + val);
                        m_MaxTriHard = val;

                        //m_CurrentMonkaCount--;
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has increased the TriHard spam limit to " + m_MaxTriHard);
                        return;
                    }

                    else
                    {
                        Console.WriteLine("Error with amount to increase: " + chatMessage[1]);
                        return;
                    }
                }
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaMonkaSAmount") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaMonkaSAmount"))
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    int val;

                    if (Int32.TryParse(chatMessage[1], out val))
                    {
                        if (val < 0)
                        {
                            Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to set the spam amount to below 0");
                            m_Client.SendWhisper(e.ChatMessage.Username, "You cannot set the limit below 0. cmonBruh");
                            return;
                        }

                        if (val > Int32.MaxValue)
                        {
                            Console.Write("Moderator " + e.ChatMessage.Username + " tried to give a value higher than max int");
                            m_Client.SendWhisper(e.ChatMessage.Username, "value exceeds ~2.1 billion, integer error.");
                            return;
                        }

                        Console.WriteLine("monkaS amount increased by mod: " + e.ChatMessage.Username + " to value: " + val);
                        m_MaxMonkaS = val;

                        //m_CurrentMonkaCount--;
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has increased the monkaS spam limit to " + m_MaxMonkaS);
                        return;
                    }

                    else
                    {
                        Console.WriteLine("Error with amount to increase: " + chatMessage[1]);
                        return;
                    }
                }
            }

            //Check the current amount of monkaS
            if (e.ChatMessage.Message.StartsWith("!MonkaCount"))
            {
                m_Client.SendMessage(TwitchInfo.ChannelName, "Available MonkaS: " + m_CurrentMonkaCount);
                return;
            }

            //This code is for adding more monkaS stock
            if (e.ChatMessage.Message.StartsWith("!MonkaAdd") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaAdd"))
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    float val;

                    //if (Int32.TryParse(chatMessage[1], out val))
                    //if (float.TryParse(Convert.ToFloat, val));
                    val = (float)Convert.ToDouble(chatMessage[1]);

                    if (val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to add monkaS below 0");
                        m_Client.SendWhisper(e.ChatMessage.Username, "You cannot add a negative number.");
                        return;
                    }

                    if (val > Int32.MaxValue)
                    {
                        Console.Write("Moderator " + e.ChatMessage.Username + " tried to give a value higher than max int");
                        m_Client.SendWhisper(e.ChatMessage.Username, "Total monkaS value exceeds ~2.1 billion, integer error.");
                        return;
                    }

                    Console.WriteLine("monkaS Val increased by mod: " + e.ChatMessage.Username + " by value: " + val);
                    m_CurrentMonkaCount += val;

                    //m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has increased the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                    return;

                }
            }

            //Get rid of monkaS
            if (e.ChatMessage.Message.StartsWith("!MonkaRemove") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaRemove"))
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    float val;

                    val = (float)Convert.ToDouble(chatMessage[1]);
                    

                    if (val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to remove a negative number");
                        m_Client.SendWhisper(e.ChatMessage.Username, "You cannot remove a negative number.");
                        return;
                    }

                    if (m_CurrentMonkaCount - val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to make monka count as negative");
                        m_Client.SendWhisper(e.ChatMessage.Username, "This operation causes a negative number. Try again.");
                        return;
                    }

                    Console.WriteLine("monkaS Val decreased by mod: " + e.ChatMessage.Username + " by value: " + val);
                    m_CurrentMonkaCount -= val;
                    Math.Floor(m_CurrentMonkaCount);

                    if (m_CurrentMonkaCount <= 0)
                    {
                        //1 for the bot to use
                        //m_CurrentMonkaCount = 1;
                    }

                    //m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has decreased the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                    return;
                }
            }

            //Let mods set the amount of monkaS
            if (e.ChatMessage.Message.StartsWith("!MonkaChange") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaChange"))
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                if (chatMessage.Length < 1)
                {
                    Console.WriteLine("Command did not recieve a value");
                    return;
                }

                else
                {
                    if (chatMessage[1] == null)
                    {
                        Console.WriteLine("Null value hit in chatMessage[1]");
                        return;
                    }

                    float val;

                    val = (float)Convert.ToDouble(chatMessage[1]);

                    if (val < 0)
                    {
                        Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to change monka to negative");
                        m_Client.SendWhisper(e.ChatMessage.Username, "Sorry but you can't put the chat in debt... cmonBruh");
                        return;
                    }

                    Console.WriteLine("monkaS Val changed by mod: " + e.ChatMessage.Username + " by value: " + val);
                    m_CurrentMonkaCount = val;

                    if (m_CurrentMonkaCount <= 0)
                    {
                        //1 for the bot to use
                        //m_CurrentMonkaCount = 1;
                    }

                    //m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has changed the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                    return;
                }
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaCommands") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaCommands"))
            {
                Console.WriteLine("Moderator: " + e.ChatMessage.Username + " asked what the commands were");
                //TODO: SPLIT INTO 2 PARTS

                m_Client.SendWhisper(e.ChatMessage.Username, e.ChatMessage.Username + " the commands are: !MonkaCount to view the current monka left, !MonkaAdd [number] to add more monka, !MonkaRemove [number] to remove that amount of monka, !MonkaChange [number] to change the amount of monka left,"
                + " !MonkaMonkaSAmount [number] to increase the limit on monkaS Spam, !MonkaTriHardAmount [number] to increase the limit on TriHard Spam, !MonkaMonkaSWorth [number] to change the value of monkaS, and !MonkaTriHardWorth [number] to change the trihard worth."
                + " Feel free to PM Piemeup on Twitch or Anolog#6680 on Discord for any questions!");

                return;
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaCommands") && e.ChatMessage.IsModerator == false || e.ChatMessage.IsBroadcaster == false && e.ChatMessage.Message.StartsWith("!MonkaCommands"))
            {
                Console.WriteLine("Non mod: " + e.ChatMessage.Username + " asked what the commands were");

                m_Client.SendWhisper(e.ChatMessage.Username, e.ChatMessage.Username + " the commands for non mods are: !MonkaCount to view the current monka left."
                + " Feel free to PM Piemeup on Twitch or Anolog#6680 on Discord for any questions!");

                return;
            }

            else
            {
                string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                Console.WriteLine("monkaS Amount: " + m_CurrentMonkaCount);

                int triHardTrack = 0;
                int monkaSTrack = 0;

                //Check for multiple emotes
                for (int i = 0; i < chatMessage.Length; i++)
                {
                    if (chatMessage[i] == "monkaS")
                    {
                        monkaSTrack++;
                    }

                    if (chatMessage[i] == "TriHard")
                    {
                        triHardTrack++;
                    }

                    if ((m_MaxMonkaS + 1) == monkaSTrack)
                    {
                        m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(5));
                        m_Client.SendWhisper(e.ChatMessage.Username, "Automaded Message: The emote limit for monkaS is currently " + m_MaxMonkaS + ". Please don't spam more than that. <3");
                        return;
                    }

                    if ((m_MaxTriHard + 1) == triHardTrack)
                    {
                        m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(5));
                        m_Client.SendWhisper(e.ChatMessage.Username, "Automaded Message: The emote limit for TriHard is currently " + m_MaxTriHard + ". Please don't spam more than that. <3");
                        return;
                    }
                }

                for (int i = 0; i < chatMessage.Length; i++)
                {
                    if (chatMessage[i] == "monkaS" || chatMessage[i] == "monkaOMEGA")
                    {
                        //m_Client.SendMessage(TwitchInfo.ChannelName, "Current MonkaS Count: " + m_CurrentMonkaCount);

                        if (m_CurrentMonkaCount > 0.0f)
                        {
                            if (chatMessage[i] == "monkaS")
                            {
                                m_CurrentMonkaCount -= m_MonkaWorth;
                            }

                            /*
                            else if (chatMessage[i] == "monkaOMEGA")
                            {
                                m_CurrentMonkaCount -= 2;
                            }
                            */

                        }

                        if (m_CurrentMonkaCount <= 0.0f)
                        {
                            //double check
                            m_CurrentMonkaCount = 0;
                            //m_Client.SendMessage(TwitchInfo.ChannelName, "WE ARE OUT OF MONKAS/MONKAOMEGA, SPAM MORE TriHard or triGOLD !");
                            m_Client.SendMessage(TwitchInfo.ChannelName, "WE ARE OUT OF monkaS , SPAM MORE TriHard !");

                            m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(1));
                            return;
                        }
                    }

                    else if (chatMessage[i] == "TriHard")
                    {
                        m_CurrentMonkaCount += m_TriHardWorth;
                    }
                    /*
                    else if (chatMessage[i] == "triGOLD")
                    {
                        m_CurrentMonkaCount += 3;
                    }
                    */
                }
            }

            if (m_CurrentMonkaCount == 101)
            {
                //Take away for user message
                m_CurrentMonkaCount--;

                m_Client.SendMessage(TwitchInfo.ChannelName, "100 monkaS left! Don't overuse!");

                //Take away for the bot
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 151)
            {
                //Take away for user message
                m_CurrentMonkaCount--;

                m_Client.SendMessage(TwitchInfo.ChannelName, "150 monkaS left! Don't overuse!");

                //Take away for the bot
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 201)
            {
                //Take away for user message
                m_CurrentMonkaCount--;

                m_Client.SendMessage(TwitchInfo.ChannelName, "200 monkaS left! Don't overuse!");

                //Take away for the bot
                // m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 301)
            {
                //Take away for user message
                m_CurrentMonkaCount--;

                m_Client.SendMessage(TwitchInfo.ChannelName, "300 monkaS left! Don't overuse!");

                //Take away for the bot
                //m_CurrentMonkaCount--;
            }


            else if (m_CurrentMonkaCount == 501)
            {
                //Take away for user message
                 m_CurrentMonkaCount--;

                m_Client.SendMessage(TwitchInfo.ChannelName, "500 monkaS left! Don't overuse!");

                //Take away for the bot
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 51)
            {
                m_CurrentMonkaCount--;
                m_Client.SendMessage(TwitchInfo.ChannelName, "50 monkaS left! Don't overuse!");
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 26)
            {
                m_CurrentMonkaCount--;
                m_Client.SendMessage(TwitchInfo.ChannelName, "25 monkaS left! Don't overuse!");
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 11)
            {
                m_CurrentMonkaCount--;
                m_Client.SendMessage(TwitchInfo.ChannelName, "10 monkaS left! Don't overuse!");
                //m_CurrentMonkaCount--;
            }

            else if (m_CurrentMonkaCount == 6)
            {
                m_CurrentMonkaCount--;
                m_Client.SendMessage(TwitchInfo.ChannelName, "5 monkaS left! Don't overuse!");
                // m_CurrentMonkaCount--;
            }
        }

        private void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            Console.WriteLine($"Error! {e.Error}");
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            //Comment to get rid of debug.
            Console.WriteLine(e.Data);
        }

        internal void Disconnect()
        {
            Console.WriteLine("Ending Bot");

            m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Disabled!");
        }
    }
}
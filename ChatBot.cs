using System;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Api.Models.v5.Users;

namespace TwitchBot
{
    internal class ChatBot
    {
        readonly ConnectionCredentials m_Credentials = new ConnectionCredentials(TwitchInfo.BotUsername, TwitchInfo.BotToken);
        TwitchClient m_Client;

        static int m_CurrentMonkaCount;

        public ChatBot()
        {
            m_CurrentMonkaCount = 20;
        }

        internal void Connect()
        {
            Console.WriteLine("Connecting");

            m_Client = new TwitchClient();
            m_Client.Initialize(m_Credentials, TwitchInfo.ChannelName);
            m_Client.ChatThrottler = new TwitchLib.Client.Services.MessageThrottler(m_Client, 20, TimeSpan.FromSeconds(30));
            m_Client.ChatThrottler.ApplyThrottlingToRawMessages = true;
            m_Client.ChatThrottler.StartQueue();

            m_Client.OnLog += Client_OnLog;
            m_Client.OnConnectionError += Client_OnConnectionError;
            m_Client.OnMessageReceived += Client_OnMessageRecieved;

            m_Client.Connect();

        }

        private void Client_OnMessageRecieved(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine("Debug: Throttle: " + m_Client.ChatThrottler.PendingSendCount);

            if (m_Client.ChatThrottler.PendingSendCount > 5 && m_CurrentMonkaCount > 0)
            {
                m_Client.ChatThrottler.Clear();
            }

            //Check the current amount of monkaS
            if (e.ChatMessage.Message.StartsWith("!MonkaCount"))
            {
                m_Client.SendMessage(TwitchInfo.ChannelName, "Available MonkaS: " + m_CurrentMonkaCount);
                return;
            }

            //This code is for adding more monkaS stock
            if (e.ChatMessage.Message.StartsWith("!MonkaAdd") && e.ChatMessage.IsModerator == true|| e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaAdd"))
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
                        Console.WriteLine("monkaS Val increased by mod: " + e.ChatMessage.Username + " by value: " + val);
                        m_CurrentMonkaCount += val;

                        m_CurrentMonkaCount--;
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has increased the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                        return;
                    }

                    else
                    {
                        Console.WriteLine("Error with amount to increase: " + chatMessage[1]);
                        return;
                    }
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

                    int val;

                    if (Int32.TryParse(chatMessage[1], out val))
                    {
                        Console.WriteLine("monkaS Val decreased by mod: " + e.ChatMessage.Username + " by value: " + val);
                        m_CurrentMonkaCount -= val;

                        if (m_CurrentMonkaCount <= 0)
                        {
                            //1 for the bot to use
                            m_CurrentMonkaCount = 1;
                        }

                        m_CurrentMonkaCount--;
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has decreased the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                        return;
                    }

                    else
                    {
                        Console.WriteLine("Error with amount to decrease: " + chatMessage[1]);
                        return;
                    }
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

                    int val;

                    if (Int32.TryParse(chatMessage[1], out val))
                    {
                        Console.WriteLine("monkaS Val changed by mod: " + e.ChatMessage.Username + " by value: " + val);
                        m_CurrentMonkaCount = val;

                        if (m_CurrentMonkaCount <= 0)
                        {
                            //1 for the bot to use
                            m_CurrentMonkaCount = 1;
                        }

                        m_CurrentMonkaCount--;
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has changed the monkaS count! There are " + m_CurrentMonkaCount + " left!");
                        return;
                    }

                    else
                    {
                        Console.WriteLine("Error with amount to decrease: " + chatMessage[1]);
                        return;
                    }
                }
            }

            if (e.ChatMessage.Message.StartsWith("!MonkaCommands") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaCommands"))
            {
                Console.WriteLine("Moderator: " + e.ChatMessage.Username + " asked what the commands were");

                m_Client.SendWhisper(e.ChatMessage.Username, e.ChatMessage.Username + " the commands are: !MonkaCount to view the current monka left, !MonkaAdd [number] to add more monka, !MonkaRemove [number] to remove that amount of monka, and !MonkaChange [number] to change the amount of monka left."
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

                for (int i = 0; i < chatMessage.Length; i++)
                {
                    if (chatMessage[i] == "monkaS")
                    {
                        //m_Client.SendMessage(TwitchInfo.ChannelName, "Current MonkaS Count: " + m_CurrentMonkaCount);

                        if (m_CurrentMonkaCount > 0)
                        {
                            if (m_CurrentMonkaCount == 102)
                            {
                                //Take away for user message
                                m_CurrentMonkaCount--;

                                m_Client.SendMessage(TwitchInfo.ChannelName, "100 monkaS left! Don't overuse!");

                                //Take away for the bot
                                m_CurrentMonkaCount--;
                            }

                            else if (m_CurrentMonkaCount == 52)
                            {
                                m_CurrentMonkaCount--;
                                m_Client.SendMessage(TwitchInfo.ChannelName, "50 monkaS left! Don't overuse!");
                                m_CurrentMonkaCount--;
                            }

                            else if (m_CurrentMonkaCount == 27)
                            {
                                m_CurrentMonkaCount--;
                                m_Client.SendMessage(TwitchInfo.ChannelName, "25 monkaS left! Don't overuse!");
                                m_CurrentMonkaCount--;
                            }

                            else if (m_CurrentMonkaCount == 12)
                            {
                                m_CurrentMonkaCount--;
                                m_Client.SendMessage(TwitchInfo.ChannelName, "10 monkaS left! Don't overuse!");
                                m_CurrentMonkaCount--;
                            }

                            else if (m_CurrentMonkaCount == 7)
                            {
                                m_CurrentMonkaCount--;
                                m_Client.SendMessage(TwitchInfo.ChannelName, "5 monkaS left! Don't overuse!");
                                m_CurrentMonkaCount--;
                            }

                            else
                            {
                                m_CurrentMonkaCount--;
                            }
                        }

                        else if (m_CurrentMonkaCount == 0)
                        {
                            //double check
                            m_CurrentMonkaCount = 0;
                            m_Client.SendMessage(TwitchInfo.ChannelName, "WE ARE OUT OF MONKAS, SPAM MORE TriHard !");
                            m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(1));
                        }
                    }

                    else if (chatMessage[i] == "TriHard")
                    {
                        m_CurrentMonkaCount += 5;
                    }
                }
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
            Console.WriteLine("Disconnecting");
        }
    }
}
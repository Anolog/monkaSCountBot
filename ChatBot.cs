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
            m_CurrentMonkaCount = 100;
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

            if (e.ChatMessage.Message.StartsWith("!monkaCount"))
            {
                m_Client.SendMessage(TwitchInfo.ChannelName, "Available MonkaS: " + m_CurrentMonkaCount);
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
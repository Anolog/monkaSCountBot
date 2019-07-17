using System;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Api.Models.v5.Users;
using System.Globalization;
using System.Collections.Generic;

namespace TwitchBot
{
    internal class ChatBot
    {
        readonly ConnectionCredentials m_Credentials = new ConnectionCredentials(TwitchInfo.BotUsername, TwitchInfo.BotToken);
        TwitchClient m_Client;

        string m_VersionNumber = "1.3";

        static float m_CurrentMonkaCount;
        int m_MaxMonkaS = 10;
        int m_MaxTriHard = 10;
        float m_MonkaWorth = 1.0f;
        float m_TriHardWorth = 3.0f;

        //Emote key, string for if monka or tri
        Dictionary<string, string> m_EmoteList = new Dictionary<string, string>();

        const int m_FinalMessageAmount = 5;
        int m_CurrentMessageCount = 0;

        DateTime m_TimeCanSteal;
        int m_TimerSecondsReset = 60;

        bool m_Level5Trigger = false;
        bool m_Level10Trigger = false;
        bool m_Level15Trigger = false;
        bool m_Level25Trigger = false;
        bool m_Level50Trigger = false;
        bool m_Level75Trigger = false;
        bool m_Level100Trigger = false;

        bool m_BotEnabled = false;
        bool m_FirstTimeConnecting = true;
        bool m_InDebt = false;

        int m_CaveInCounter = 0;
        float m_CaveInChance = 0.01f;

        public ChatBot()
        {
            ResetAllValues();
            ResetEmotesBeingUsed();
        }

        private void ResetAllValues()
        {
            m_MaxMonkaS = 10;
            m_MaxTriHard = 10;
            m_MonkaWorth = 1.0f;
            m_TriHardWorth = 3.0f;
            m_CurrentMonkaCount = 250;
            m_TimerSecondsReset = 60;

            m_CaveInChance = 0.01f;
            m_CaveInCounter = 0;

            m_CurrentMessageCount = 0;

            m_Level5Trigger = false;
            m_Level10Trigger = false;
            m_Level15Trigger = false;
            m_Level25Trigger = false;
            m_Level50Trigger = false;
            m_Level75Trigger = false;
            m_Level100Trigger = false;

            m_InDebt = false;

            ResetStealTimer();
        }

        private void ResetStealTimer()
        {
            m_TimeCanSteal = DateTime.Now;
            m_TimeCanSteal = m_TimeCanSteal.AddSeconds(m_TimerSecondsReset);
            Console.WriteLine("Time Can Steal Reset To " + m_TimeCanSteal);
        }

        public void SetStealTimer(DateTime aDateTime)
        {
            m_TimeCanSteal = aDateTime;
        }

        public void SetStealTimer(int aSecondsAhead)
        {
            //m_TimeCanSteal = DateTime.Now;
            //m_TimeCanSteal = m_TimeCanSteal.AddSeconds(Convert.ToDouble(aSecondsAhead));
            m_TimerSecondsReset = aSecondsAhead;
            ResetStealTimer();
        }

        public void ResetEmotesBeingUsed()
        {
            m_EmoteList.Clear();
            m_EmoteList.Add("monkaS", "monkaS");
            m_EmoteList.Add("TriHard", "TriHard");
        }

        public TwitchClient GetClient()
        {
            return m_Client;
        }

        public void SetEnabled(bool aEnabled)
        {
            m_BotEnabled = aEnabled;
        }

        public bool GetEnabled()
        {
            return m_BotEnabled;
        }

        public void AddMonkaEmote(string aEmoteAdding)
        {
            if (!CheckIfEmoteExists(aEmoteAdding))
            {
                m_EmoteList.Add(aEmoteAdding, "monkaS");
            }
        }
        public void AddTriHardEmote(string aEmoteAdding)
        {
            if (!CheckIfEmoteExists(aEmoteAdding))
            {
                m_EmoteList.Add(aEmoteAdding, "TriHard");
            }
        }

        public List<string> GetListOfEmotes()
        {
            List<string> emoteList = new List<string>();

            foreach (string emoteName in m_EmoteList.Keys)
            {
                emoteList.Add(emoteName);
            }

            return emoteList;
        }

        public bool CheckIfEmoteExists(string aEmoteToCheck)
        {
            return m_EmoteList.ContainsKey(aEmoteToCheck);
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
            m_Client.OnDisconnected += Client_OnDisconnect;

            m_Client.Connect();
            //m_Client.OnJoinedChannel += Client_OnJoinedChannel;

        }

        private void Client_OnDisconnect(object sender, OnDisconnectedArgs e)
        {
            Console.WriteLine("Disconnected from chat.");
            m_Client.Connect();
        }

        private void Client_OnConnect(object sender, OnConnectedArgs e)
        {
            Console.WriteLine("Started Bot");

            if (m_FirstTimeConnecting == true)
            {
                m_FirstTimeConnecting = false;
                m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot has joined the channel. The bot is currently disabled! If you are a moderator, type !MonkaEnable to start.");
            }

            else if (m_FirstTimeConnecting == false)
            {
                Console.WriteLine("Bot Reconnecting due to error");
            }
        }

        private void Client_OnMessageRecieved(object sender, OnMessageReceivedArgs e)
        {
            if (m_BotEnabled == false)
            {
                if (e.ChatMessage.Message.StartsWith("!MonkaEnable") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaEnable"))
                {
                    Console.WriteLine("Bot Enabled By: " + e.ChatMessage.Username);
                    m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Enabled! Version " + m_VersionNumber + " There are " + m_CurrentMonkaCount + " monkaS remaining. DON'T OVERUSE! monkaS = -" + m_MonkaWorth + ", TriHard = +" + m_TriHardWorth + "!");
                    m_BotEnabled = true;
                }
            }
            //Have this for seperating, could throw it down below, but I want it here for organization, even though this is already spaghett
            if (m_BotEnabled == true)
            {
                if (e.ChatMessage.Message.StartsWith("!MonkaDisable") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaDisable"))
                {
                    Console.WriteLine("Bot Disabled By: " + e.ChatMessage.Username);
                    m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Disabled! monkaS now has unlimited supplies monkaS");
                    m_BotEnabled = false;
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaReset") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaReset"))
                {
                    Console.WriteLine("Bot has been reset to default");
                    m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot has been reset. Values have all reset to default values.");
                    ResetAllValues();

                }
            }

            //These commands are for if the bot is enabled. So they can't do anything with it to reduce spam further, even though it's a mod
            if (m_BotEnabled == true)
            {
                Console.WriteLine("Debug: Throttle: " + m_Client.ChatThrottler.PendingSendCount);

                if (m_Client.ChatThrottler.PendingSendCount > 5 && m_CurrentMonkaCount > 0)
                {
                    m_Client.ChatThrottler.Clear();
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaValues"))
                {
                    Console.WriteLine(e.ChatMessage.Username + " asked for the values");
                    m_Client.SendMessage(TwitchInfo.ChannelName, "The current values are: monkaS = -" + m_MonkaWorth +
                        ". TriHard = +" + m_TriHardWorth + ". You can currently spam monkaS " + m_MaxMonkaS +
                        " times. You can currently spam TriHard " + m_MaxTriHard + " times.");
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
                    //Show a rounded value
                    float temp = m_CurrentMonkaCount;
                    temp *= 2;
                    temp = (float)Math.Round(temp, MidpointRounding.AwayFromZero);
                    temp /= 2;

                    m_Client.SendMessage(TwitchInfo.ChannelName, "Available monkaS : " + temp);
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

                        float val = 0;

                        if (chatMessage[1].GetType() != val.GetType())
                        {
                            Console.WriteLine("Error, type not accepted");
                            return;
                        }

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

                        float temp = m_CurrentMonkaCount;
                        temp *= 2;
                        temp = (float)Math.Round(temp, MidpointRounding.AwayFromZero);
                        temp /= 2;

                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has decreased the monkaS count! There are " + temp + " left!");
                        return;
                    }
                }

                //Debt cause why not? 
                if (e.ChatMessage.Message.StartsWith("!MonkaDebt") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaDebt"))
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

                        if (val >= 0)
                        {
                            Console.WriteLine("Moderator " + e.ChatMessage.Username + " tried to give debt in positive value");
                            m_Client.SendWhisper(e.ChatMessage.Username, "The value must be a negative number i.e., -200");
                            return;
                        }

                        Console.WriteLine(e.ChatMessage.Username + " has put the chat in debt.");
                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has put the chat in debt. You owe " + val + ". :rage: PAY UP :rage: ");

                        m_CurrentMonkaCount = val;

                        m_InDebt = true;

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
                        m_Level5Trigger = false;
                        m_Level10Trigger = false;
                        m_Level15Trigger = false;
                        m_Level25Trigger = false;
                        m_Level50Trigger = false;
                        m_Level75Trigger = false;
                        m_Level100Trigger = false;

                        return;
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaCommands") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaCommands"))
                {
                    Console.WriteLine("Moderator: " + e.ChatMessage.Username + " asked what the commands were");
                    m_Client.SendWhisper(e.ChatMessage.Username, "Hi " + e.ChatMessage.Username + ", Please visit twitch.tv/monkascountbot to see a list of commands." + "Feel free to PM Piemeup on Twitch or Anolog#6680 on Discord for any questions!");
                    return;
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaCommands") && e.ChatMessage.IsModerator == false || e.ChatMessage.IsBroadcaster == false && e.ChatMessage.Message.StartsWith("!MonkaCommands"))
                {
                    Console.WriteLine("Non mod: " + e.ChatMessage.Username + " asked what the commands were");

                    m_Client.SendWhisper(e.ChatMessage.Username, "Hi " + e.ChatMessage.Username + ", the commands are available at twitch.tv/monkascountbot !"
                    + " Feel free to PM Piemeup on Twitch or Anolog#6680 on Discord for any questions!");

                    return;
                }

                //More manual type of adding emotes
                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteAdd") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaEmoteAdd"))
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 2)
                    {
                        string emoteToAdd = chatMessage[1].ToString();
                        string emoteListType = chatMessage[2].ToString();
                        if (!String.IsNullOrEmpty(emoteToAdd))
                        {
                            if (!String.IsNullOrEmpty(emoteListType))
                            {
                                if (!CheckIfEmoteExists(emoteToAdd))
                                {
                                    if (emoteListType == "monkaS")
                                    {
                                        AddMonkaEmote(emoteToAdd);
                                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has added the emote " + emoteToAdd + " to be tracked.");
                                    }
                                    else if (emoteListType == "TriHard")
                                    {
                                        AddTriHardEmote(emoteToAdd);
                                        m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has added the emote " + emoteToAdd + " to be tracked.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error adding emote to list: " + emoteToAdd + " to " + emoteListType);
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        Console.WriteLine("Error: Could not add emote, message not long enough");
                    }
                }

                //Easier way
                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteAddMonka") && e.ChatMessage.IsModerator == true || e.ChatMessage.Message.StartsWith("!MonkaEmoteAddMonka") && e.ChatMessage.IsBroadcaster == true)
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 1)
                    {
                        string emoteToAdd = chatMessage[1].ToString();
                        if (!String.IsNullOrEmpty(emoteToAdd))
                        {
                            if (!CheckIfEmoteExists(emoteToAdd))
                            {
                                AddMonkaEmote(emoteToAdd);
                                m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has added the emote " + emoteToAdd + " to be tracked.");
                            }
                        }
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteAddTriHard") && e.ChatMessage.IsModerator == true || e.ChatMessage.Message.StartsWith("!MonkaEmoteAddTriHard") && e.ChatMessage.IsBroadcaster == true)
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 1)
                    {
                        string emoteToAdd = chatMessage[1].ToString();
                        if (!String.IsNullOrEmpty(emoteToAdd))
                        {
                            if (!CheckIfEmoteExists(emoteToAdd))
                            {
                                AddTriHardEmote(emoteToAdd);
                                m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has added the emote " + emoteToAdd + " to be tracked.");
                            }
                        }
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteRemove") && e.ChatMessage.IsModerator == true || e.ChatMessage.Message.StartsWith("!MonkaEmoteRemove") && e.ChatMessage.IsBroadcaster == true)
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');
                    if (chatMessage.Length > 1)
                    {
                        string emoteToRemove = chatMessage[1].ToString();
                        if (!String.IsNullOrEmpty(emoteToRemove))
                        {
                            if (CheckIfEmoteExists(emoteToRemove))
                            {
                                //Move this to a function you fuckwad
                                m_EmoteList.Remove(emoteToRemove);
                                m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has removed the emote " + emoteToRemove + " from being tracked.");
                            }
                        }
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteListReset") && e.ChatMessage.IsModerator == true || e.ChatMessage.Message.StartsWith("!MonkaEmoteListReset") && e.ChatMessage.IsBroadcaster == true)
                {
                    ResetEmotesBeingUsed();
                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has reset the emotes being tracked.");
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaEmoteList"))
                {
                    m_Client.SendWhisper(e.ChatMessage.Username, "These are the current emotes being tracked: ");

                    string whisper = "";
                    for (int i = 0; i < GetListOfEmotes().Count; i++)
                    {
                        if (whisper.Length + GetListOfEmotes()[i].Length >= 500)
                        {
                            m_Client.SendWhisper(e.ChatMessage.Username, whisper);
                            whisper = "";
                        }
                        else
                        {
                            whisper += (" " + GetListOfEmotes()[i]);
                        }
                    }

                    m_Client.SendWhisper(e.ChatMessage.Username, whisper);

                }

                if (e.ChatMessage.Message.StartsWith("!MonkaTimer") && e.ChatMessage.IsModerator == true || e.ChatMessage.Message.StartsWith("!MonkaTimer") && e.ChatMessage.IsBroadcaster == true)
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 1)
                    {
                        int amountToChange;
                        bool parsed = int.TryParse(chatMessage[1].ToString(), out amountToChange);

                        if (parsed)
                        {
                            SetStealTimer(amountToChange);
                            m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has set the steal timer to " + amountToChange + " seconds!");
                        }
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaSteal"))
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 1)
                    {
                        int amountToSteal;
                        bool parsed = int.TryParse(chatMessage[1].ToString(), out amountToSteal);

                        if (parsed)
                        {
                            if (amountToSteal > 0)
                            {
                                if (amountToSteal > m_CurrentMonkaCount)
                                {
                                    m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + ", you cannot steal more than " + m_CurrentMonkaCount);
                                }
                                else
                                {
                                    if (DateTime.Now < m_TimeCanSteal)
                                    {
                                        TimeSpan timeLeft = m_TimeCanSteal - DateTime.Now;
                                        m_Client.SendMessage(TwitchInfo.ChannelName, "Cops are all over the place! You can steal again in " + Convert.ToInt32(timeLeft.TotalSeconds) + " seconds!");
                                    }
                                    else
                                    {
                                        Random randNum = new Random();
                                        float stealingPercentMessageOutput = amountToSteal / m_CurrentMonkaCount;
                                        float stealingPercent = amountToSteal / m_CurrentMonkaCount;
                                        stealingPercent = 1.0f - stealingPercent;

                                        if (stealingPercent <= 0.1f)
                                        {
                                            if (randNum.Next(10) == 1)
                                            {
                                                stealingPercent = 0.25f;
                                            }
                                        }

                                        stealingPercent *= 100;

                                        int numGen = randNum.Next(101);

                                        if (numGen <= stealingPercent)
                                        {
                                            //Steal successful
                                            m_CurrentMonkaCount -= amountToSteal;
                                            m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " HAS STOLEN " + amountToSteal + " monkaS with a " + Convert.ToInt32(Math.Floor(stealingPercent)) + "% chance to steal!");
                                            m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS :point_right: :chart_with_downwards_trend: " + m_CurrentMonkaCount + "LEFT!");
                                        }
                                        else
                                        {
                                            int timeoutAmount;
                                            if (amountToSteal >= 100)
                                            {
                                                timeoutAmount = 100;
                                            }
                                            else
                                            {
                                                timeoutAmount = amountToSteal;
                                            }
                                            m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has been caught trying to steal! They have been jailed for " + Convert.ToInt32(timeoutAmount) + " seconds!");
                                            m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(Convert.ToInt32(timeoutAmount)), "Timed out for failed stealing.");
                                        }

                                        if (stealingPercent >= 85f)
                                        {
                                            SetStealTimer(30);
                                        }
                                        else
                                        {
                                            SetStealTimer(Convert.ToInt32(stealingPercent) + randNum.Next(300));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (e.ChatMessage.Message.StartsWith("!MonkaCaveInPercent") && e.ChatMessage.IsModerator == true || e.ChatMessage.IsBroadcaster == true && e.ChatMessage.Message.StartsWith("!MonkaCaveInPercent"))
                {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    if (chatMessage.Length > 1)
                    {
                        float percentage;
                        bool parsed = float.TryParse(chatMessage[1].ToString(), out percentage);

                        if (parsed)
                        {
                            if (percentage > -1 && percentage < 101)
                            {
                                m_CaveInChance = percentage / 100;

                                m_Client.SendMessage(TwitchInfo.ChannelName, e.ChatMessage.Username + " has set the cave in chance to " + (m_CaveInChance * 100) + "%");
                            }
                        }
                    }
                }

                else
                        {
                    string[] chatMessage = e.ChatMessage.Message.Split(' ', '\t');

                    Console.WriteLine("monkaS Amount: " + m_CurrentMonkaCount);

                    int triHardTrack = 0;
                    int monkaSTrack = 0;

                    if (chatMessage.Length == 0)
                    {
                        return;
                    }

                    else if (chatMessage.Length < 0)
                    {
                        return;
                    }

                    //Check for multiple emotes
                    for (int i = 0; i < chatMessage.Length; i++)
                    {

                        if (CheckIfEmoteExists(chatMessage[i]))
                        {
                            if (m_EmoteList[chatMessage[i]] == "monkaS")
                            {
                                monkaSTrack++;
                            }
                            else if (m_EmoteList[chatMessage[i]] == "TriHard")
                            {
                                triHardTrack++;
                            }
                        }

                        if ((m_MaxMonkaS + 1) == monkaSTrack)
                        {
                            m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(5));
                            m_Client.SendWhisper(e.ChatMessage.Username, "Automated Message: The emote limit for monka emotes is currently " + m_MaxMonkaS + ". Please don't spam more than that. <3");
                            return;
                        }

                        if ((m_MaxTriHard + 1) == triHardTrack)
                        {
                            m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(5));
                            m_Client.SendWhisper(e.ChatMessage.Username, "Automated Message: The emote limit for Trihard emotes is currently " + m_MaxTriHard + ". Please don't spam more than that. <3");
                            return;
                        }
                    }

                    for (int i = 0; i < chatMessage.Length; i++)
                    {
                        if (CheckIfEmoteExists(chatMessage[i]))
                        {
                            if (m_EmoteList[chatMessage[i]] == "monkaS")
                            {
                                if (m_CurrentMonkaCount > 0.0f)
                                {
                                    m_CurrentMonkaCount -= m_MonkaWorth;
                                }
                                else if (m_CurrentMonkaCount <= 0.0f && m_InDebt == false)
                                {
                                    m_CurrentMonkaCount = 0;

                                    if (m_CurrentMessageCount != m_FinalMessageAmount)
                                    {
                                        m_CurrentMessageCount++;
                                        m_Client.SendMessage(TwitchInfo.ChannelName, "WE ARE OUT OF monkaS , SPAM MORE TriHard !");
                                    }

                                    m_Client.TimeoutUser(e.ChatMessage.Username, TimeSpan.FromSeconds(1));
                                    return;

                                }
                            }
                            else if (m_EmoteList[chatMessage[i]] == "TriHard")
                            {
                                if (m_CurrentMonkaCount >= 0 && m_InDebt == true)
                                {
                                    m_InDebt = false;

                                    m_Client.SendMessage(TwitchInfo.ChannelName, "WE ARE DEBT FREE TriHard 7");
                                }

                                m_CurrentMonkaCount += m_TriHardWorth;
                            }
                        }
                    }

                    if (chatMessage.Length > 1)
                    {
                        if (chatMessage[0] == "TriHard" && chatMessage[1] == ":pick:" || chatMessage[0] == "TriHard" && chatMessage[1] == "⛏")
                        {
                            m_CaveInCounter++;

                            Random rand = new Random();

                            //float chance = (m_CaveInChance * 100f) * (0.10f * m_CaveInCounter);

                            //m_CaveInChance += m_CaveInChance;


                            if (rand.Next(101) <= m_CaveInChance * 100)
                            {
                                m_Client.SendMessage(TwitchInfo.ChannelName, "CAVE IN AT THE TRIHARD MINES!!! WE LOST 35% OF OUR MONKAS!");

                                m_CaveInCounter = 0;

                                int amountToRemove;
                                amountToRemove = Convert.ToInt32(Math.Floor(m_CurrentMonkaCount * 0.35f));
                                m_CurrentMonkaCount -= amountToRemove;
                            }
                        }
                    }
                }

                if ((int)m_CurrentMonkaCount == 101 && m_Level100Trigger == false)
                {
                    //Take away for user message
                    m_CurrentMonkaCount--;

                    m_Client.SendMessage(TwitchInfo.ChannelName, "100 monkaS left! Don't overuse!");

                    m_Level5Trigger = false;
                    m_Level10Trigger = false;
                    m_Level15Trigger = false;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = true;

                    //Take away for the bot
                    //m_CurrentMonkaCount--;
                }

                else if ((int)m_CurrentMonkaCount == 51 && m_Level50Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "50 monkaS left! Don't overuse!");
                    //m_CurrentMonkaCount--;

                    m_Level5Trigger = false;
                    m_Level10Trigger = false;
                    m_Level15Trigger = false;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = false;
                }

                else if ((int)m_CurrentMonkaCount == 76 && m_Level75Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "75 monkaS left! Don't overuse!");
                    //m_CurrentMonkaCount--;

                    m_Level5Trigger = false;
                    m_Level10Trigger = false;
                    m_Level15Trigger = false;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = true;
                    m_Level100Trigger = false;
                }

                else if ((int)m_CurrentMonkaCount == 26 && m_Level25Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "25 monkaS left! Don't overuse!");
                    //m_CurrentMonkaCount--;

                    m_Level5Trigger = false;
                    m_Level10Trigger = false;
                    m_Level15Trigger = false;
                    m_Level25Trigger = true;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = false;
                }

                else if ((int)m_CurrentMonkaCount == 16 && m_Level15Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "15 monkaS left! Don't overuse!");
                    //m_CurrentMonkaCount--;

                    m_CurrentMessageCount = 0;

                    m_Level5Trigger = false;
                    m_Level10Trigger = false;
                    m_Level15Trigger = true;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = false;
                }

                else if ((int)m_CurrentMonkaCount == 11 && m_Level10Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "10 monkaS left! Don't overuse!");
                    //m_CurrentMonkaCount--;

                    m_CurrentMessageCount = 0;

                    m_Level5Trigger = false;
                    m_Level10Trigger = true;
                    m_Level15Trigger = true;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = false;
                }

                else if ((int)m_CurrentMonkaCount == 6 && m_Level5Trigger == false)
                {
                    m_CurrentMonkaCount--;
                    m_Client.SendMessage(TwitchInfo.ChannelName, "5 monkaS left! Don't overuse!");

                    m_Level5Trigger = true;
                    m_Level10Trigger = true;
                    m_Level15Trigger = false;
                    m_Level25Trigger = false;
                    m_Level50Trigger = false;
                    m_Level75Trigger = false;
                    m_Level100Trigger = false;

                    // m_CurrentMonkaCount--;
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
            //Console.WriteLine("In debt: " + m_InDebt);
        }

        internal void Disconnect()
        {
            Console.WriteLine("Ending Bot");

            m_Client.SendMessage(TwitchInfo.ChannelName, "monkaS Bot Disabled!");
        }
    }
}
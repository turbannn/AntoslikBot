using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntoslikBot.BotHandlers;
using AntoslikBot.Interfaces;
using AntoslikBot.Services;
using Discord.WebSocket;


namespace AntoslikBot.Modes;

internal class ManualMode : IBotMode
{
    public static Dictionary<String, Func<Task>> Commands { get; private set; } = null!;
    public string InputCommand { get; set; } = null!;
    public bool isSet { get; private set; } = false;

    private ManualModeService _manualModService { get; set; }
    private MessageHandler _messageHandler { get; set; } = null!;

    public ManualMode(MessageHandler msgHandler, ManualModeService manualModService)
    {
        _messageHandler = msgHandler;
        _manualModService = manualModService;
        Set();
    }

    public void Set()
    {
        Commands = new Dictionary<string, Func<Task>>
            {
                { "text",  _manualModService.Text},
                { "statvoices",  _manualModService.SeeUsersInVoiceChannels},
                { "allroles",  _manualModService.SeeAllRoles},
                { "changeroles",  _manualModService.ChangeRoles}
            };
        InputCommand = string.Empty;
        isSet = true;
    }
    public void ChangeSubscriptions(DiscordSocketClient client)
    {
        client.MessageReceived -= _messageHandler.MessagesHandler;
        client.UserVoiceStateUpdated -= VoiceStateHandler.VoiceStateUpdateHandler;
    }
    public async Task Run()
    {

        if (!isSet)
        {
            Console.WriteLine("Bot is not set");
        }

        while (isSet)
        {
            Console.WriteLine(ToString());
            Console.WriteLine("readline: ");

            try
            {
#pragma warning disable
                InputCommand = Console.ReadLine();
                if (!InputCommand.Equals("exit"))
                {
                    await Commands[InputCommand]();
                }
                else
                {
                    isSet = false;
                }
#pragma warning restore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n Wrong input command[ManualMode]");
            }
        }
    }
    public override string ToString()
    {
        string str = "[manualmod]";
        foreach (var key in Commands.Keys)
        {
            str += $"'{key}' | ";
        }
        str += "'exit'[manualmod]";
        return str;
    }
}
using Discord.WebSocket;
using AntoslikBot.Services;
using AntoslikBot.Interfaces;
using AntoslikBot.BotHandlers;

namespace AntoslikBot.Modes;

internal class AutoMode : IBotMode
{
    internal static Dictionary<String, Func<SocketMessage, Task>> Commands { get; private set; } = null!;
    public string InputCommand { get; set; } = null!;
    public bool isSet { get; private set; } = false;

    private MessageHandler _messageHandler;

    private AutoModeService _autoModeService;

    public AutoMode(MessageHandler msgHandler, AutoModeService autoModeService)
    {
        _messageHandler = msgHandler;
        _autoModeService = autoModeService;
        Set();
    }

    public void Set()
    {
        Commands = new Dictionary<string, Func<SocketMessage, Task>>
        {
            { "help",  _autoModeService.getHelp }, //automode command module.
            { "mute", _autoModeService.MuteWholeChannelExceptInvoker },
            { "find", _autoModeService.MarkFittingMessages }
        };
        InputCommand = string.Empty;
        isSet = true;
    }
    public void ChangeSubscriptions(DiscordSocketClient client)
    {
        client.MessageReceived += _messageHandler.MessagesHandler;
        client.UserVoiceStateUpdated += VoiceStateHandler.VoiceStateUpdateHandler;
    }
    public Task Run()
    {

        if (!isSet)
        {
            Console.WriteLine("Bot is not set");
            return Task.CompletedTask;
        }

        while (isSet)
        {
            Console.WriteLine(ToString());
            Console.WriteLine("readline: ");
#pragma warning disable
            InputCommand = Console.ReadLine();
            if (InputCommand.Equals("exit"))
            {
                isSet = false;
            }
#pragma warning restore
        }
        return Task.CompletedTask;
    }
    public override string ToString()
    {
        return "[automod]'exit'[automod]";
    }
}
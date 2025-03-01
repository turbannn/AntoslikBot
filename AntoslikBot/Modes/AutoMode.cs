using Discord.WebSocket;
using AntoslikBot.Services;
using AntoslikBot.Interfaces;
using AntoslikBot.BotHandlers;

namespace AntoslikBot.Modes;

internal class AutoMode : IBotMode
{
    public static Dictionary<String, Func<SocketMessage, Task>> Commands { get; private set; } = null!;
    public string InputCommand { get; set; } = null!;
    public bool isSet { get; private set; } = false;

    private AutoModeService _autoModeService { get; set; }
    private MessageHandler _messageHandler { get; set; } = null!;

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
            { "find", _autoModeService.RespondToTextMessagesThatMatchThePrompt }
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
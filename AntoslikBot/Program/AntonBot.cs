using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using AntoslikBot.JsonSettings;
using AntoslikBot.Modes;
using AntoslikBot.Interfaces;
using AntoslikBot.BotHandlers;
using AntoslikBot.AIFeatures;
using AntoslikBot.Services;


namespace AntoslikBot.Program
{
    internal sealed class AntonBot
    {
        private readonly ServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly JSONReader _jsonReader;

        private IBotMode _botMod = null!;

        private static bool _isBotOn = true;
        private static string? _lineToWorkWith = "";

        public AntonBot()
        {
            _services = ConfigureServices();

            _jsonReader = _services.GetRequiredService<JSONReader>();
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _client.Log += Log;
        }

        public async Task RunBotAsync(string[] args)
        {
            await _client.LoginAsync(TokenType.Bot, _jsonReader.jSON.Token);
            await _client.StartAsync();

            await Task.Delay(2000);

            while (_isBotOn)
            {
                Console.WriteLine("[AntonBot]'setManual' | 'setAuto' | 'off'[AntonBot]");
                Console.WriteLine("readline: ");
                _lineToWorkWith = Console.ReadLine();
                switch (_lineToWorkWith)
                {
                    case "setAuto":
                        _botMod = _services.GetRequiredService<AutoMode>();
                        _botMod.ChangeSubscriptions(_client);
                        await _botMod.Run();
                        break;
                    case "setManual":
                        _botMod = _services.GetRequiredService<ManualMode>();
                        _botMod.ChangeSubscriptions(_client);
                        await _botMod.Run();
                        break;
                    case "off":
                        _isBotOn = false;
                        break;
                }
            }
        }
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<JSONReader>()
                .AddTransient<ManualMode>()
                .AddTransient<AutoMode>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<ManualModeService>()
                .AddSingleton<AutoModeService>()
                .AddSingleton<AIMessageResponser>()
                .AddSingleton(provider =>
                {
                    return new DiscordSocketClient(new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.Guilds |
                                        GatewayIntents.GuildMessages |
                                        GatewayIntents.GuildMessageReactions |
                                        GatewayIntents.MessageContent |
                                        GatewayIntents.GuildVoiceStates
                    });
                })
                .AddLogging(builder => { builder.AddConsole(); })
                .BuildServiceProvider();
        }
        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}

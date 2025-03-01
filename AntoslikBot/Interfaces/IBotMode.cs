using Discord.WebSocket;

namespace AntoslikBot.Interfaces;

internal interface IBotMode
{
    static Dictionary<String, Func<Task>> Commands { get; } = null!;
    string InputCommand { get; set; }
    bool isSet { get; } //is Set and therefore can run

    void ChangeSubscriptions(DiscordSocketClient client);
    void Set();
    Task Run();
}
using Discord.WebSocket;
using AntoslikBot.Modes;

namespace AntoslikBot.BotHandlers
{
    internal abstract class CommandHandler
    {
        public static async Task HandleTheCommandAsync(string pref, SocketMessage msg)
        {
            string cmd = msg.Content.Split()[0].Substring(pref.Length);
            try
            {
                await AutoMode.Commands[cmd](msg);
            }
            catch (Exception ex)
            {
                await msg.Channel.SendMessageAsync("хуйня комманда бро");
                Console.WriteLine($"{ex.Message}\n[AutoMode]Input command error[AutoMode]");
            }
        }
    }
}

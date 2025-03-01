using Discord.WebSocket;
using Discord;

namespace AntoslikBot.BotHandlers;

internal abstract class VoiceStateHandler
{
    public static async Task VoiceStateUpdateHandler(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
    {
        IReadOnlyCollection<IGuildChannel>? guildChannels = null;
        IGuild? guild = null;
        IGuildChannel? textChannel = null;
        if (oldState.VoiceChannel == null && newState.VoiceChannel != null && user.Id == 332450176335609859)
        {
            guild = newState.VoiceChannel.Guild;
            guildChannels = await guild.GetChannelsAsync();
            foreach (var channel in guildChannels)
            {
                if (channel is ITextChannel && (channel.Name.Contains("основ") || channel.Name.Contains("gener") || channel.Name.Contains("щее") || channel.Name.Contains("щий")))
                {
                    textChannel = channel;
                }
            }
            if (textChannel != null)
            {
                await ((ITextChannel)textChannel).SendMessageAsync($"{user.Mention} ОСЛИК ИАААААААА");
            }
        }
    }
}
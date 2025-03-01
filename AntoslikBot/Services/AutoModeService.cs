using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntoslikBot.Modes;
using Discord;
using System.Data;

namespace AntoslikBot.Services;

internal class AutoModeService
{
    public AutoModeService()
    {

    }

    public async Task getHelp(SocketMessage msg)
    {
        StringBuilder help = new StringBuilder();
        foreach (var item in AutoMode.Commands)
        {
            help.Append("ia!")
                .Append(item.Key)
                .Append("\n");
        }
        await msg.Channel.SendMessageAsync(embed: new EmbedBuilder()
            .WithTitle("All Availiable commands: ")
            .WithDescription(help.ToString())
            .Build());
    }
    /// <summary>
    /// Finds all the messages that  match given prompt
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task MarkFittingMessages(SocketMessage msg)
    {
        //parsing
        string[] splittedCommand = msg.Content.Split(" ");

        //validation
        if (splittedCommand.Length < 2)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Argument error")
                .WithDescription("Invalid argument count <2")
                .WithDescription("ia!find 'limit' 'prompt' [messages author]")
                .Build());
            return;
        }
        if (splittedCommand.Length > 4)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Argument error")
                .WithDescription("Invalid argument count >4")
                .Build());
            return;
        }

        //extracting the args
        int.TryParse(splittedCommand[1], out int msglimit);
        string prompt = splittedCommand[2];

        //msgs content
        IEnumerable<IMessage> allMessages = await msg.Channel.GetMessagesAsync(limit: msglimit).FlattenAsync();
        IEnumerable<IMessage>? messagesToSend = null;

        SocketUser? user = null;

        if (msg.MentionedUsers.Any())
        {
            user = msg.MentionedUsers.First();
        }

        if (user != null)
            messagesToSend = allMessages.Where(m => m.Content.Contains(prompt) && m.Author.Id == user.Id);
        else
            messagesToSend = allMessages.Where(m => m.Content.Contains(prompt));


        await PointTheMessages(messagesToSend, msg.Channel, prompt);
    }
    private async Task PointTheMessages(IEnumerable<IMessage> messagesToSend, ISocketMessageChannel channel, string prompt)
    {
        foreach (var message in messagesToSend)
        {
            foreach (var m in messagesToSend)
            {
                await channel.SendMessageAsync(embed:
                    new EmbedBuilder()
                    .WithTitle(ChangeOffsetToCest(m.CreatedAt).ToString())
                    .WithDescription("Prompt: " + prompt)
                    .Build(),
                    messageReference: new MessageReference(m.Id));
            }
        }

    }
    private static DateTimeOffset ChangeOffsetToCest(DateTimeOffset original)
    {
        TimeZoneInfo cetInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        DateTimeOffset cetTime = TimeZoneInfo.ConvertTime(original, cetInfo);
        return original
            .Subtract(cetTime.Offset)
            .ToOffset(cetTime.Offset).AddHours(1);
    }

    /// <summary>
    /// Mutes whole voice channel except the Invoker of the Command
    /// </summary>
    /// <param name="msg"></param>
    /// <returns>
    /// 
    /// </returns>
    public async Task MuteWholeChannelExceptInvoker(SocketMessage msg)
    {
        int.TryParse(msg.Content.Split(" ")[1], out int seconds);

        if (seconds < 1 || seconds > 48)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Timespan error")
                .WithDescription("Invalid entered time")
                .Build());
            return;
        }

        SocketGuildUser? invoker = null;

        try
        {
            invoker = (SocketGuildUser)msg.Author;
        }
        catch
        {
            Console.WriteLine("Author cast error");
            return;
        }

        if (invoker.VoiceChannel == null)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("User error")
                .WithDescription("User is not in VC")
                .Build());
            return;
        }

        if (invoker.Roles.FirstOrDefault(r => r.Name.Equals("AB_Mute_Permission")) == null)
        {
            await msg.Channel.SendMessageAsync(embed:
                new EmbedBuilder()
                .WithTitle("Permission error")
                .WithDescription("No permission role found")
                .Build());
            return;
        }

        await MuteWholeVoiceChannel(invoker, true);

        await Task.Delay(seconds * 1000);

        await MuteWholeVoiceChannel(invoker, false);
    }
    private async Task MuteUser(ulong userId, SocketGuild guild, bool isMuted)
    {
        var user = guild.GetUser(userId);

        if (user == null)
        {
            return;
        }
        await user.ModifyAsync(properties =>
        {
            properties.Mute = isMuted;
        });
    }
    private async Task MuteWholeVoiceChannel(SocketGuildUser invoker, bool mute)
    {
        var voiceChannel = invoker.Guild.GetVoiceChannel(invoker.VoiceChannel.Id);

        var usersInVC = invoker.Guild.Users.Where(u => u.VoiceChannel != null //snachala naiti potom yusat
        && u.VoiceChannel.Id == voiceChannel.Id
        && u.Id != invoker.Id
        && !u.IsBot).ToList();

        for (int i = 0; i < usersInVC.Count; i++)
        {
            await MuteUser(usersInVC[i].Id, invoker.Guild, mute);
        }
    }
}

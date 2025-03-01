using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AntoslikBot.Services
{
    internal class ManualModeService
    {
        private readonly DiscordSocketClient _client;
        public ManualModeService(DiscordSocketClient client)
        {
            _client = client;
        }
        public async Task Text()
        {
            IGuild guild;
            string? message = string.Empty;

            guild = SelectGuild();
            ITextChannel? guildTextChannel = await SelectTextChannel(guild);
            while (message != "quit")
            {
                if (guildTextChannel != null)
                {
                    Console.WriteLine("message readline: | '@' | 'quit'");
                    message = Console.ReadLine();
                    if (message != null && message != "quit" && !message.Contains("@"))
                        await guildTextChannel.SendMessageAsync(message);
                    if (message != null && message != "quit" && message.Contains("@"))
                        await Mention(guild.GetUsersAsync().Result, guildTextChannel);
                }
            }
        }
        public IGuild SelectGuild()
        {
            IGuild? guild = null;
            string[] guildNames = new string[_client.Guilds.Count];

            Console.WriteLine("Select server:");
            int i = 0;
            foreach (var g in _client.Guilds)
            {
                guildNames[i] = $"{i}.{g.Name}.";
                Console.WriteLine($"{i}. {g.Name}.");

                i++;
            }

            while (guild == null)
            {
                Console.Write("guildName index readline: ");
                int.TryParse(Console.ReadLine(), out int res);

                guild = _client.Guilds.FirstOrDefault(g => g.Name.Contains(guildNames[res].Split(".")[1]));

                if (guild == null)
                    Console.WriteLine("GUILD NOT FOUND, index issue");
            }

            return guild;
        }

        public async Task<ITextChannel?> SelectTextChannel(IGuild guild)
        {
            IReadOnlyCollection<IGuildChannel> GuildChannels = await guild.GetChannelsAsync();
            ITextChannel? returnTextChannel = null;
            string? channelName = string.Empty;

            foreach (var ch in GuildChannels)
            {
                if (ch is ITextChannel)
                    Console.WriteLine(ch.Name);
            }
            while (returnTextChannel == null && channelName != "quit")
            {
                Console.WriteLine("readline textchannel: ");
                channelName = Console.ReadLine();
                if (channelName != null)
                {
                    returnTextChannel = GuildChannels.FirstOrDefault(c => c.Name.Contains(channelName)) as ITextChannel;
                    if (returnTextChannel == null)
                        Console.WriteLine("TEXT CHANNEL NOT FOUND | 'quit'");
                }
            }
            Console.WriteLine("Channel found");
            return returnTextChannel;
        }
        public async Task Mention(IReadOnlyCollection<IGuildUser> GuildUsers, ITextChannel guildTextChannel)
        {
            string? Message = string.Empty;

            await SeeUsers(GuildUsers);
            await Task.Run(async () =>
            {
                IGuildUser? user = SelectUser(GuildUsers);
                Console.WriteLine("readline message: ");
                Message = Console.ReadLine();
                await guildTextChannel.SendMessageAsync($"{user.Mention} {Message}");
            });
        }
        public Task SeeUsersInVoiceChannels()
        {
            IGuild guild = SelectGuild();
            foreach (IGuildChannel ch in guild.GetChannelsAsync().Result)
            {
                if (ch is SocketVoiceChannel voiceCh)
                {
                    if (voiceCh.ConnectedUsers.Count == 0)
                    {
                        Console.WriteLine($"{voiceCh.Name}: No users");
                    }
                    else
                    {
                        Console.WriteLine($"[{voiceCh.Name}]: ");
                        foreach (var user in voiceCh.ConnectedUsers)
                        {
                            Console.WriteLine(user.DisplayName + "(" + user.Username + ")");
                        }
                        Console.WriteLine();
                    }
                }
            }
            return Task.CompletedTask;
        }
        public Task SeeUsers(IReadOnlyCollection<IGuildUser> GuildUsers)
        {
            string? command = string.Empty;
            while (command != "online" && command != "offline" && command != "all")
            {
                Console.WriteLine("online(ne rabotaet) | offline(ne rabotaet) | all");
                Task.Delay(1000);
                command = Console.ReadLine();
                switch (command)
                {
                    case "online":
                        Console.WriteLine("Online users: ");
                        Task.Delay(1000);
                        foreach (var us in GuildUsers)
                        {
                            if (!us.IsBot && us.Status == UserStatus.Online)
                                Console.WriteLine(us.Username);
                        }
                        break;
                    case "offline":
                        Console.WriteLine("Offline users: ");
                        Task.Delay(1000);
                        foreach (var us in GuildUsers)
                        {
                            if (!us.IsBot && us.Status == UserStatus.Offline)
                                Console.WriteLine(us.Username);
                        }
                        break;
                    case "all":
                        Console.WriteLine("All users: ");
                        Task.Delay(1000);
                        foreach (var us in GuildUsers)
                        {
                            if (!us.IsBot)
                                Console.WriteLine(us.Username);
                        }
                        break;
                }
            }
            return Task.CompletedTask;
        }
        public IGuildUser SelectUser(IReadOnlyCollection<IGuildUser> GuildUsers)
        {
            string? mention = string.Empty;
            IGuildUser? user = null;
            Console.WriteLine("readline username: ");
            mention = Console.ReadLine();
            if (mention != null)
                user = GuildUsers.FirstOrDefault(u => u.Username.Contains(mention));

            while (user == null)
            {
                Console.WriteLine("USER NOT FOUND: ");
                mention = Console.ReadLine();
                if (mention != null)
                    user = GuildUsers.FirstOrDefault(u => u.Username.Contains(mention));
            }
            Console.WriteLine("User found");
            return user;
        }
        public Task SeeAllRoles()
        {
            IGuild guild = SelectGuild();
            Console.WriteLine("All roles: ");
            foreach (var role in guild.Roles)
            {
                Console.WriteLine(role.Name);
            }

            return Task.CompletedTask;
        }
        public Task SeeAllRoles(IGuild guild)
        {
            Console.WriteLine("All roles: ");
            foreach (var role in guild.Roles)
            {
                Console.WriteLine(role.Name);
            }

            return Task.CompletedTask;
        }
        public Task SeeUserRoles(IGuildUser user)
        {
            Console.WriteLine("All user roles:");
            if (user is SocketGuildUser socketUser)
            {
                foreach (var r in socketUser.Roles)
                {
                    Console.WriteLine(r.Name);
                }
            }
            return Task.CompletedTask;
        }
        public async Task ChangeRoles()
        {
            string? command = string.Empty;
            string? roleName = string.Empty;
            IGuild guild = SelectGuild();
            IReadOnlyCollection<IGuildUser> guildUsers = await guild.GetUsersAsync();
            IGuildUser? user = null;
            IRole? role;

            Console.WriteLine("Users: ");
            await SeeUsers(guildUsers);

            while (command != "add" && command != "remove")
            {
                Console.WriteLine("add | remove");
                command = Console.ReadLine();
                switch (command)
                {
                    case "add":
                        role = null;
                        user = SelectUser(guildUsers);
                        await SeeAllRoles(guild);

                        while (role == null)
                        {
                            Console.WriteLine("rolename readline: ");
                            roleName = Console.ReadLine();
                            if (roleName != null)
                            {
                                role = guild.Roles.FirstOrDefault(r => r.Name.Contains(roleName));
                            }
                        }
                        try
                        {
                            await user.AddRoleAsync(role);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        break;
                    case "remove":
                        role = null;

                        user = SelectUser(guildUsers);
                        await Task.Delay(1500);

                        await SeeUserRoles(user);
                        await Task.Delay(1000);

                        while (role == null)
                        {
                            Console.WriteLine("rolename readline: ");
                            roleName = Console.ReadLine();
                            if (roleName != null)
                            {
                                role = guild.Roles.FirstOrDefault(r => r.Name.Contains(roleName));
                            }
                        }
                        try
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        break;
                }
            }
        }
    }
}

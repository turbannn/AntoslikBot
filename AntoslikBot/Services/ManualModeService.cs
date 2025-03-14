using Discord;
using Discord.WebSocket;
using AntoslikBot.Interfaces;

namespace AntoslikBot.Services
{
    //need to check for concurency issues and overall usage
    internal class ManualModeService : IManualModeService
    {
        private readonly DiscordSocketClient _client;

        public string? Readline { get; private set; }

        public ManualModeService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task Text()
        {
            IGuild guild;
            Readline = string.Empty;

            guild = SelectGuild();
            ITextChannel? guildTextChannel = await SelectTextChannel(guild);
            while (Readline != "quit")
            {
                if (guildTextChannel != null)
                {
                    Console.WriteLine("message readline: | '@' | 'quit'");
                    Readline = Console.ReadLine();
                    if (Readline != null && Readline != "quit" && !Readline.Contains("@"))
                        await guildTextChannel.SendMessageAsync(Readline);
                    if (Readline != null && Readline != "quit" && Readline.Contains("@"))
                        await Mention(guild.GetUsersAsync().Result, guildTextChannel);
                }
            }
        }
        private IGuild SelectGuild()
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

        private async Task<ITextChannel?> SelectTextChannel(IGuild guild)
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
        private async Task Mention(IReadOnlyCollection<IGuildUser> GuildUsers, ITextChannel guildTextChannel)
        {
            Readline = string.Empty;

            await SeeUsers(GuildUsers);
            await Task.Run(async () =>
            {
                IGuildUser? user = SelectUser(GuildUsers);
                Console.WriteLine("readline message: ");
                Readline = Console.ReadLine();
                await guildTextChannel.SendMessageAsync($"{user.Mention} {Readline}");
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
        private Task SeeUsers(IReadOnlyCollection<IGuildUser> GuildUsers)
        {
            Readline = string.Empty;

            while (Readline != "online" && Readline != "offline" && Readline != "all")
            {
                Console.WriteLine("online(ne rabotaet) | offline(ne rabotaet) | all");
                Task.Delay(1000);
                Readline = Console.ReadLine();
                switch (Readline)
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
        private IGuildUser SelectUser(IReadOnlyCollection<IGuildUser> GuildUsers)
        {
            Readline = string.Empty;
            IGuildUser? user = null;

            while (user == null)
            {
                Console.WriteLine("readline username: ");
                Readline = Console.ReadLine();
                if (Readline != null)
                    user = GuildUsers.FirstOrDefault(u => u.Username.Contains(Readline));
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
        private Task SeeUserRoles(IGuildUser user)
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
            Readline = string.Empty;
            string? roleName = string.Empty;
            
            IGuildUser? user = null;
            IRole? role;

            IGuild guild = SelectGuild();
            IReadOnlyCollection<IGuildUser> guildUsers = await guild.GetUsersAsync();

            Console.WriteLine("Users: ");
            await SeeUsers(guildUsers);

            while (Readline != "add" && Readline != "remove")
            {
                Console.WriteLine("add | remove");
                Readline = Console.ReadLine();
                switch (Readline)
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

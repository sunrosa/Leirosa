namespace Leirosa
{
    public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private async Task<Discord.Color> GetUserColor(Discord.WebSocket.SocketGuildUser user)
        {
            _log.Debug("A color to be used with an embed targeting a user has been requested.");

            var discord_color = new Discord.Color(0, 0, 0);

            if (bool.Parse(Program.Config["embed_color_from_user_avatar"]))
            {
                _log.Debug("Config opted to calculate embed color via user avatar.");

                // Some bullshit to get the average color of the user icon
                using (var client = new HttpClient())
                {
                    _log.Debug("Retrieving user icon...");
                    var response = await client.GetAsync(user.GetAvatarUrl());
                    _log.Debug("Converting HTTP response to image...");
                    var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                    using (var image = new ImageMagick.MagickImage(ms))
                    {
                        image.Resize(1, 1);
                        var color = image.GetPixels().First().ToColor();
                        discord_color = new Discord.Color(color.R, color.G, color.B);
                    }
                }
            }
            else
            {
                _log.Debug("Config opted to calculate embed color via user's top role.");

                _log.Debug("Sorting roles...");
                var roles_sorted = user.Roles.OrderBy(x => x.Position).Reverse(); // Roles sorted by position in server

                _log.Debug("Stepping through roles from top to bottom in order to find a color for the embed...");
                foreach (var role in roles_sorted)
                {
                    if (!(role.Color.R == 0 && role.Color.G == 0 && role.Color.B == 0))
                    {
                        _log.Debug("Found a role color for the embed.");
                        discord_color = role.Color;
                        break;
                    }
                }

                if (discord_color.R == 0 && discord_color.G == 0 && discord_color.B == 0)
                {
                    _log.Debug("No role color was found for the embed. Setting the color to 200, 200, 200...");
                    discord_color = new Discord.Color(200, 200, 200);
                }
            }

            return discord_color;
        }

        [Discord.Commands.Command("help")]
        [Discord.Commands.Summary("Provides command info.")]
        public async Task HelpAsync()
        {
            _log.Debug("\"help\" was called!");

            // One in a hundred chance the command replies with "help!" and then does nothing else
            var random = new Random();
            if (random.Next(0, 101) == 0)
            {
                _log.Debug("Random check passed! Replying only with \"help!\"");
                await ReplyAsync("help!");
                return;
            }

            _log.Debug("Obtaining list of all commands...");
            var commands = Program.Commands.Commands.ToList();
            _log.Debug("Formatting as help text...");
            var output = "```\n";
            foreach (var command in commands)
            {
                output += $"{command.Name} [{command.Module.Name.Remove(command.Module.Name.Length - 6)}]: {command.Summary}\n";
            }
            output += "\n```";

            _log.Debug("Replying...");
            await ReplyAsync(output);
        }

        [Discord.Commands.Command("source")]
        [Discord.Commands.Summary("Prints my source code.")]
        public async Task SourceAsync()
        {
            _log.Debug("\"source\" was called!");
            await ReplyAsync(Program.Config["source"]);
        }

        [Discord.Commands.Command("invite")]
        [Discord.Commands.Summary("Prints invite URL to invite me into other servers.")]
        public async Task InviteAsync()
        {
            _log.Debug("\"invite\" was called!");
            await ReplyAsync(Program.Config["invite"]);
        }

        [Discord.Commands.Command("userinfo")]
        [Discord.Commands.Alias("whois")]
        [Discord.Commands.Summary("[user (optional)] Prints data about you or somebody else.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)] // Could be made workable in DM.
        public async Task WhoisAsync(Discord.WebSocket.SocketGuildUser user = null) // May fail when pinging other users.
        {
            _log.Debug("\"whois\" was called!");

            if (user == null)
            {
                _log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            // Is the target user the bot itself?
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                _log.Debug("\"whois\" was called targeting the bot itself. Sending a friendly message...");
                await ReplyAsync("That's me!");
            }

            _log.Debug("Sorting roles...");
            var roles_sorted = user.Roles.OrderBy(x => x.Position).Reverse(); // Roles sorted by position in server

            var role_list = "";
            var client_list = "";
            var custom_status = "";

            foreach (var role in roles_sorted)
            {
                role_list += role.Name + "\n";
            }
            foreach (var client in user.ActiveClients)
            {
                client_list += client.ToString() + "\n";
            }

            try
            {
                _log.Debug("Attempting to fetch user's custom status...");
                custom_status = $" ({(user.Activities.First() as Discord.CustomStatusGame).State})";
            }
            catch
            {
                _log.Debug("User has no custom status. Continuing...");
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField("Name", $"{user.Username}#{user.Discriminator}", true)
            .AddField("Nickname", user.Nickname ?? "None", true)
            .AddField("Id", user.Id, true)
            .AddField("Account created", user.CreatedAt, true)
            .AddField("Joined", user.JoinedAt, true)
            .AddField("Status", $"{user.Status.ToString()}{custom_status}", true)
            .AddField("Active clients", client_list != "" ? client_list : "None", true)
            .AddField($"Roles ({roles_sorted.Count()})", role_list, true);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("serverinfo")]
        [Discord.Commands.Summary("Prints general server info.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task ServerInfoAsync()
        {
            _log.Debug("\"serverinfo\" was called!");

            var discord_color = new Discord.Color(0, 0, 0);

            // Some bullshit to get the average color of the server icon
            using (var client = new HttpClient())
            {
                _log.Debug("Retrieving server icon...");
                var response = await client.GetAsync(Context.Guild.IconUrl);
                _log.Debug("Converting HTTP response to image...");
                var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                using (var image = new ImageMagick.MagickImage(ms))
                {
                    image.Resize(1, 1);
                    var color = image.GetPixels().First().ToColor();
                    discord_color = new Discord.Color(color.R, color.G, color.B);
                }
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(discord_color)
            .AddField("Name", Context.Guild.Name, true)
            .AddField("Id", Context.Guild.Id, true)
            .AddField("Members", Context.Guild.MemberCount, true)
            .AddField("Owner", await Context.Client.GetUserAsync(Context.Guild.OwnerId), true) /* For some fucking reason, Context.Guild.Owner.Username is null, so we do this instead. */
            .AddField("Created", Context.Guild.CreatedAt, true)
            .AddField("Verification", Context.Guild.VerificationLevel, true)
            .AddField("Roles", Context.Guild.Roles.Count, true)
            .AddField("Text channels", Context.Guild.TextChannels.Count, true)
            .AddField("Voice channels", Context.Guild.VoiceChannels.Count, true)
            .AddField("Active threads", Context.Guild.ThreadChannels.Count, true)
            .AddField("Boosts", Context.Guild.PremiumSubscriptionCount, true);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("permissions")]
        [Discord.Commands.Summary("[user (optional)] Prints a user's guild permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task PermissionsAsync(Discord.WebSocket.SocketGuildUser user = null)
        {
            _log.Debug("\"permissions\" was called!");

            if (user == null)
            {
                _log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            _log.Debug("Creating permissions list...");
            var permissions_string = user.GuildPermissions.ToList().Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField("Permissions", permissions_string);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("cpermissions")]
        [Discord.Commands.Summary("[user (optional), channel (optional)] Prints a user's channel permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task CPermissionsAsync(Discord.WebSocket.SocketGuildUser user = null, Discord.WebSocket.SocketGuildChannel channel = null)
        {
            _log.Debug("\"cpermissions\" was called!");

            if (user == null)
            {
                _log.Debug("No user target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            if (channel == null)
            {
                _log.Debug("No channel target. Setting channel target to context.");
                channel = Context.Channel as Discord.WebSocket.SocketGuildChannel;
            }

            var permissions_string = "";

            _log.Debug("Creating permissions list...");
            try
            {
                permissions_string = user.GetPermissions(channel).ToList().Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);
            }
            catch (System.InvalidOperationException)
            {
                _log.Debug("No permissions found. Returning...");
                await ReplyAsync("User is not a member of channel.");
                return;
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField($"Permissions in {channel.Name}", permissions_string);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("suggest")]
        [Discord.Commands.Summary("[suggestion (remainder)] Where you can suggest features.")]
        public async Task SuggestAsync([Discord.Commands.Remainder]string suggestion)
        {
            _log.Debug("\"suggest\" was called!");

            using (var writer = File.AppendText(Program.Config["suggestions_path"]))
            {
                writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {suggestion}");
            }

            await ReplyAsync("Thank you for your suggestion!");
        }

        [Discord.Commands.Command("report")]
        [Discord.Commands.Summary("[report (remainder)] Where you can report bugs.")]
        public async Task ReportAsync([Discord.Commands.Remainder]string report)
        {
            _log.Debug("\"report\" was called!");

            using (var writer = File.AppendText(Program.Config["reports_path"]))
            {
                writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {report}");
            }

            await ReplyAsync("Thank you for your error report!");
        }

        [Discord.Commands.Command("maintenance")]
        [Discord.Commands.Summary("Maintenance function that solves some problems.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task MaintenanceAsync()
        {
            _log.Debug("\"maintenance\" was called!");

            _log.Debug($"Downloading users in guild {Context.Guild.Name} ({Context.Guild.Id}).");
            await Context.Guild.DownloadUsersAsync();

            _log.Debug("Collecting garbage...");
            System.GC.Collect();

            await ReplyAsync("Maintenance complete!");
        }

        [Discord.Commands.Command("guilds")]
        [Discord.Commands.Summary("Prints a list of guilds the bot is in.")]
        public async Task GuildsAsync()
        {
            _log.Debug("\"guilds\" was called!");

            _log.Debug("Replying...");
            await ReplyAsync($"```\n{Context.Client.Guilds.Select(guild => guild.Name).Aggregate((a, b) => a + "\n" + b)}\n```");
        }

        [Discord.Commands.Command("channels")]
        [Discord.Commands.Summary("Prints a list of channels in the guild.")]
        public async Task ChannelsAsync()
        {
            _log.Debug("\"channels\" was called!");

            _log.Debug("Replying...");
            await ReplyAsync($"```\n{Context.Guild.Channels.OrderBy(channel => channel.Name).Select(channel => channel.Name).Aggregate((a, b) => a + "\n" + b)}\n```");
        }

        [Discord.Commands.Command("shutdown")]
        [Discord.Commands.Summary("(Developer only) Shuts the bot down.")]
        public async Task ShutdownAsync()
        {
            _log.Debug("\"shutdown\" was called!");

            if (Context.User.Id == Convert.ToUInt64(Program.Config["developer_id"]))
            {
                _log.Info("Calling Program.Shutdown()...");
                await ReplyAsync("Goodbye.");
                Program.Shutdown();
            }
            else
            {
                _log.Debug("Caller is not listed as developer. Doing nothing...");
                await ReplyAsync("Insufficient permissions.");
            }
        }

        [Discord.Commands.Command("runtime")]
        [Discord.Commands.Summary("Fetches bot runtime info.")]
        public async Task RuntimeAsync()
        {
            _log.Debug("\"runtime\" was called!");

            _log.Debug("Fetching build configuration...");
            var build_configuration = "UNKNOWN";
#if DEBUG
            build_configuration = "DEBUG";
#elif RELEASE
            build_configuration = "RELEASE";
#endif

            var commit_sha = "";

            _log.Debug("Fetching git HEAD sha...");
            try
            {
                _log.Debug("Constructing process...");
                using (var p = new System.Diagnostics.Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "git";
                    p.StartInfo.Arguments = "rev-parse HEAD";

                    _log.Debug("Starting process...");
                    p.Start();

                    _log.Debug("Reading from process...");
                    commit_sha = p.StandardOutput.ReadToEnd();

                    _log.Debug("Awaiting process exit...");
                    p.WaitForExit();
                    _log.Debug("Successfully exited process.");
                }
            }
            catch
            {
                _log.Error("Git is not installed. Command will not have full output. Replying with request to install git...");
                await ReplyAsync("Please install git on the host server for full runtime output.");
            }

            _log.Debug("Replying...");
            await ReplyAsync($"Running {Program.Config["name"]}{(commit_sha != "" ? $" {commit_sha[0..7]}" : "")} on .NET {Environment.Version} on {System.Net.Dns.GetHostName()} with build configuration {build_configuration}.");
        }
    }
}

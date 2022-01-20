namespace Leirosa.Modules
{
    public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        [Discord.Commands.Command("help")]
        [Discord.Commands.Summary("Provides command info.")]
        public async Task HelpAsync([Discord.Commands.Summary("The command to print help text for")]string command = "")
        {
            _log.Debug("\"help\" was called!");

            if (command == "")
            {
                _log.Debug("Command parameter is empty. Supplying general help...");

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
                foreach (var c in commands)
                {
                    var parameters = new List<string>();
                    foreach (var parameter in c.Parameters)
                    {
                        parameters.Add($"{parameter.Name}{(parameter.IsOptional ? " (optional)" : "")}{(parameter.IsRemainder ? " (remainder)" : "")}");
                    }

                    var parameters_str = "";
                    try
                    {
                        parameters_str = parameters.Aggregate((a, b) => a + ", " + b);
                    }
                    catch (InvalidOperationException)
                    {}

                    output += $"({c.Aliases.Aggregate((a, b) => a + ", " + b)}) ({parameters_str}) [{c.Module.Name.Remove(c.Module.Name.Length - 6)}]: {c.Summary}\n";
                }
                output += "\n```";

                _log.Debug("Replying...");
                await ReplyAsync(output);
            }
            else
            {
                _log.Debug($"Help was requested for {command}.");

                var commandInfo = Program.Commands.Commands.First(c => c.Name == command || c.Aliases.Contains(command));
                if (commandInfo == null)
                {
                    await ReplyAsync("Unknown command.");
                    return;
                }

                string? parameter_output = null;
                foreach (var parameter in commandInfo.Parameters)
                {
                    parameter_output += $"{parameter.Name}{(parameter.Summary != "" && parameter.Summary != null ? $": {parameter.Summary}" : "")}{(parameter.IsOptional ? " (optional)" : "")}{(parameter.IsRemainder ? " (remainder)" : "")}\n";
                }

                var embed = new Discord.EmbedBuilder()
                    .WithTitle(commandInfo.Name)
                    .AddField("Summary", commandInfo.Summary, true)
                    .AddField("Aliases", commandInfo.Aliases.Aggregate((a, b) => a + ", " + b), true)
                    .AddField("Module", commandInfo.Module.Name.Remove(commandInfo.Module.Name.Length - 6), true)
                    .AddField("Preconditions", commandInfo.Preconditions.Count != 0 ? commandInfo.Preconditions.Select(p => $"[{p.GetType().Name}, {p.Group ?? "None"}]").Aggregate((a, b) => a + ", " + b) : "None") /* TODO: Attribute arguments should be included here.*/
                    .AddField("Parameters", parameter_output ?? "None", false);

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Discord.Commands.Command("source")]
        [Discord.Commands.Summary("Prints my source code.")]
        public async Task SourceAsync()
        {
            _log.Debug("\"source\" was called!");
            await ReplyAsync(Program.Config.SourceURL);
        }

        [Discord.Commands.Command("invite")]
        [Discord.Commands.Summary("Prints invite URL to invite me into other servers.")]
        public async Task InviteAsync()
        {
            _log.Debug("\"invite\" was called!");
            await ReplyAsync(Program.Config.InviteURL);
        }

        [Discord.Commands.Command("userinfo")]
        [Discord.Commands.Alias("whois")]
        [Discord.Commands.Summary("Prints data about you or somebody else.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)] // Could be made workable in DM.
        public async Task WhoisAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null)
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
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
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
        [Discord.Commands.Summary("Prints a user's guild permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task PermissionsAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null)
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
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField("Permissions", permissions_string);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("cpermissions")]
        [Discord.Commands.Summary("Prints a user's channel permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task CPermissionsAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null, [Discord.Commands.Summary("Target channel")]Discord.WebSocket.SocketGuildChannel channel = null)
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
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField($"Permissions in {channel.Name}", permissions_string);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("suggest")]
        [Discord.Commands.Summary("Suggests features.")]
        public async Task SuggestAsync([Discord.Commands.Summary("Suggestion to be made")][Discord.Commands.Remainder]string suggestion)
        {
            _log.Debug("\"suggest\" was called!");

            using (var writer = File.AppendText(Program.Config.SuggestionsPath))
            {
                writer.WriteLine($"{DateTime.Now.ToString(Program.Config.DatetimeFormat)} - {Context.User.Username} [{Context.User.Id}] - {suggestion}");
            }

            await ReplyAsync("Thank you for your suggestion!");
        }

        [Discord.Commands.Command("report")]
        [Discord.Commands.Summary("Reports bugs.")]
        public async Task ReportAsync([Discord.Commands.Summary("Report to be made")][Discord.Commands.Remainder]string report)
        {
            _log.Debug("\"report\" was called!");

            using (var writer = File.AppendText(Program.Config.ReportsPath))
            {
                writer.WriteLine($"{DateTime.Now.ToString(Program.Config.DatetimeFormat)} - {Context.User.Username} [{Context.User.Id}] - {report}");
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

            if (Program.Config.DeveloperIds.Contains(Context.User.Id))
            {
                _log.Info("Calling Program.Shutdown()...");
                await ReplyAsync("Goodbye.");
                Program.Shutdown();
            }
            else
            {
                _log.Debug("Caller is not listed as a developer. Doing nothing...");
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
            await ReplyAsync($"Running {Program.Config.BotName}{(commit_sha != "" ? $" {commit_sha[0..7]}" : "")} on .NET {Environment.Version} on {System.Net.Dns.GetHostName()} with build configuration {build_configuration}.");
        }

        [Discord.Commands.Command("save")]
        [Discord.Commands.Summary("(Developer only) Saves memory objects to their destination files.")]
        public async Task SaveAsync()
        {
            _log.Debug("\"save\" was called!");

            if (Program.Config.DeveloperIds.Contains(Context.User.Id))
            {
                try
                {
                    Program.CommandTracker.Save();
                }
                catch (NullReferenceException)
                {
                    _log.Warn("Bot is not configured to track command invokations.");
                }

                await ReplyAsync("Everything has been saved.");
            }
            else
            {
                _log.Debug("Caller is not listed as a developer. Doing nothing...");
                await ReplyAsync("Insufficient permissions.");
            }
        }
    }
}

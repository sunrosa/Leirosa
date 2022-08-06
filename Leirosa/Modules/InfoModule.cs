namespace Leirosa.Modules
{
    public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        [Discord.Commands.Command("help")]
        [Discord.Commands.Summary("Provides command info.")]
        public async Task HelpAsync([Discord.Commands.Summary("The command to print help text for")]string command = "")
        {
            log.Debug("\"help\" was called!");

            var chunkSize = 15;

            if (command == "")
            {
                log.Debug("Command parameter is empty. Supplying general help...");

                // One in a hundred chance the command replies with "help!" and then does nothing else
                var random = new Random();
                if (random.Next(0, 101) == 0)
                {
                    log.Debug("Random check passed! Replying only with \"help!\"");
                    await ReplyAsync("help!");
                    return;
                }

                log.Debug("Obtaining list of all commands...");
                var commands = Program.Commands.Commands.ToList().Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / chunkSize).Select(x => x.Select(v => v.Value).ToList()).ToList();
                log.Debug("Formatting as help text...");
                foreach (var commandsChunk in commands)
                {
                    var output = "```\n";
                    foreach (var c in commandsChunk)
                    {
                        var parameters = new List<string>();
                        foreach (var parameter in c.Parameters)
                        {
                            parameters.Add($"{parameter.Name}{(parameter.IsOptional ? " (optional)" : "")}{(parameter.IsRemainder ? " (remainder)" : "")}");
                        }

                        var parametersStr = "";
                        try
                        {
                            parametersStr = parameters.Aggregate((a, b) => a + ", " + b);
                        }
                        catch (InvalidOperationException)
                        {}

                        output += $"({c.Aliases.Aggregate((a, b) => a + ", " + b)}) ({parametersStr}) [{c.Module.Name.Remove(c.Module.Name.Length - 6)}]: {c.Summary}\n";
                    }
                    output += "\n```";

                    log.Debug("Replying...");
                    await ReplyAsync(output);
                }
            }
            else
            {
                log.Debug($"Help was requested for {command}.");

                var commandInfo = Program.Commands.Commands.First(c => c.Name == command || c.Aliases.Contains(command));
                if (commandInfo == null)
                {
                    await ReplyAsync("Unknown command.");
                    return;
                }

                string? parameterOutput = null;
                foreach (var parameter in commandInfo.Parameters)
                {
                    parameterOutput += $"{parameter.Name}{(parameter.Summary != "" && parameter.Summary != null ? $": {parameter.Summary}" : "")}{(parameter.IsOptional ? " (optional)" : "")}{(parameter.IsRemainder ? " (remainder)" : "")}\n";
                }

                var embed = new Discord.EmbedBuilder()
                    .WithTitle(commandInfo.Name)
                    .AddField("Summary", commandInfo.Summary, true)
                    .AddField("Aliases", commandInfo.Aliases.Aggregate((a, b) => a + ", " + b), true)
                    .AddField("Module", commandInfo.Module.Name.Remove(commandInfo.Module.Name.Length - 6), true)
                    .AddField("Preconditions", commandInfo.Preconditions.Count != 0 ? commandInfo.Preconditions.Select(p => $"[{p.GetType().Name}, {p.Group ?? "None"}]").Aggregate((a, b) => a + ", " + b) : "None") /* TODO: Attribute arguments should be included here.*/
                    .AddField("Parameters", parameterOutput ?? "None", false);

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Discord.Commands.Command("source")]
        [Discord.Commands.Summary("Prints my source code.")]
        public async Task SourceAsync()
        {
            log.Debug("\"source\" was called!");
            await ReplyAsync(Program.Config.SourceURL);
        }

        [Discord.Commands.Command("invite")]
        [Discord.Commands.Summary("Prints invite URL to invite me into other servers.")]
        public async Task InviteAsync()
        {
            log.Debug("\"invite\" was called!");
            await ReplyAsync(Program.Config.InviteURL);
        }

        [Discord.Commands.Command("userinfo")]
        [Discord.Commands.Alias("whois")]
        [Discord.Commands.Summary("Prints data about you or somebody else.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)] // Could be made workable in DM.
        public async Task WhoisAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null)
        {
            log.Debug("\"whois\" was called!");

            if (user == null)
            {
                log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            // Is the target user the bot itself?
            if (user.Id == Context.Client.CurrentUser.Id)
            {
                log.Debug("\"whois\" was called targeting the bot itself. Sending a friendly message...");
                await ReplyAsync("That's me!");
            }

            log.Debug("Sorting roles...");
            var rolesSorted = user.Roles.OrderBy(x => x.Position).Reverse(); // Roles sorted by position in server

            var roleList = "";
            var clientList = "";
            var customStatus = "";

            foreach (var role in rolesSorted)
            {
                roleList += role.Name + "\n";
            }
            foreach (var client in user.ActiveClients)
            {
                clientList += client.ToString() + "\n";
            }

            try
            {
                log.Debug("Attempting to fetch user's custom status...");
                customStatus = $" ({(user.Activities.First() as Discord.CustomStatusGame).State})";
            }
            catch
            {
                log.Debug("User has no custom status. Continuing...");
            }

            log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField("Name", $"{user.Username}#{user.Discriminator}", true)
            .AddField("Nickname", user.Nickname ?? "None", true)
            .AddField("Id", user.Id, true)
            .AddField("Account created", user.CreatedAt, true)
            .AddField("Joined", user.JoinedAt, true)
            .AddField("Status", $"{user.Status.ToString()}{customStatus}", true)
            .AddField("Active clients", clientList != "" ? clientList : "None", true)
            .AddField($"Roles ({rolesSorted.Count()})", roleList, true);

            log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("serverinfo")]
        [Discord.Commands.Summary("Prints general server info.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task ServerInfoAsync()
        {
            log.Debug("\"serverinfo\" was called!");

            var discordColor = new Discord.Color(0, 0, 0);

            // Some bullshit to get the average color of the server icon
            using (var client = new HttpClient())
            {
                log.Debug("Retrieving server icon...");
                var response = await client.GetAsync(Context.Guild.IconUrl);
                log.Debug("Converting HTTP response to image...");
                var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                using (var image = new ImageMagick.MagickImage(ms))
                {
                    image.Resize(1, 1);
                    var color = image.GetPixels().First().ToColor();
                    discordColor = new Discord.Color(color.R, color.G, color.B);
                }
            }

            log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(discordColor)
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

            log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("permissions")]
        [Discord.Commands.Summary("Prints a user's guild permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task PermissionsAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null)
        {
            log.Debug("\"permissions\" was called!");

            if (user == null)
            {
                log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            log.Debug("Creating permissions list...");
            var permissionsString = user.GuildPermissions.ToList().Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);

            log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField("Permissions", permissionsString);

            log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("cpermissions")]
        [Discord.Commands.Summary("Prints a user's channel permissions.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task CPermissionsAsync([Discord.Commands.Summary("Target user")]Discord.WebSocket.SocketGuildUser user = null, [Discord.Commands.Summary("Target channel")]Discord.WebSocket.SocketGuildChannel channel = null)
        {
            log.Debug("\"cpermissions\" was called!");

            if (user == null)
            {
                log.Debug("No user target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            if (channel == null)
            {
                log.Debug("No channel target. Setting channel target to context.");
                channel = Context.Channel as Discord.WebSocket.SocketGuildChannel;
            }

            var permissionsString = "";

            log.Debug("Creating permissions list...");
            try
            {
                permissionsString = user.GetPermissions(channel).ToList().Select(x => x.ToString()).Aggregate((a, b) => a + "\n" + b);
            }
            catch (System.InvalidOperationException)
            {
                log.Debug("No permissions found. Returning...");
                await ReplyAsync("User is not a member of channel.");
                return;
            }

            log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(await ModuleHelpers.GetUserColor(user))
            .WithAuthor(new Discord.EmbedAuthorBuilder().WithName(user.Username).WithIconUrl(user.GetAvatarUrl()))
            .AddField($"Permissions in {channel.Name}", permissionsString);

            log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("suggest")]
        [Discord.Commands.Summary("Suggests features.")]
        public async Task SuggestAsync([Discord.Commands.Summary("Suggestion to be made")][Discord.Commands.Remainder]string suggestion)
        {
            log.Debug("\"suggest\" was called!");

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
            log.Debug("\"report\" was called!");

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
            log.Debug("\"maintenance\" was called!");

            log.Debug($"Downloading users in guild {Context.Guild.Name} ({Context.Guild.Id}).");
            await Context.Guild.DownloadUsersAsync();

            log.Debug("Collecting garbage...");
            System.GC.Collect();

            await ReplyAsync("Maintenance complete!");
        }

        [Discord.Commands.Command("guilds")]
        [Discord.Commands.Summary("Prints a list of guilds the bot is in.")]
        public async Task GuildsAsync()
        {
            log.Debug("\"guilds\" was called!");

            log.Debug("Replying...");
            await ReplyAsync($"```\n{Context.Client.Guilds.Select(guild => guild.Name).Aggregate((a, b) => a + "\n" + b)}\n```");
        }

        [Discord.Commands.Command("channels")]
        [Discord.Commands.Summary("Prints a list of channels in the guild.")]
        public async Task ChannelsAsync()
        {
            log.Debug("\"channels\" was called!");

            log.Debug("Replying...");
            await ReplyAsync($"```\n{Context.Guild.Channels.OrderBy(channel => channel.Name).Select(channel => channel.Name).Aggregate((a, b) => a + "\n" + b)}\n```");
        }

        [Discord.Commands.Command("shutdown")]
        [Discord.Commands.Summary("(Developer only) Shuts the bot down.")]
        public async Task ShutdownAsync()
        {
            log.Debug("\"shutdown\" was called!");

            if (Program.Config.DeveloperIds.Contains(Context.User.Id))
            {
                log.Info("Calling Program.Shutdown()...");
                await ReplyAsync("Goodbye.");
                Program.Shutdown();
            }
            else
            {
                log.Debug("Caller is not listed as a developer. Doing nothing...");
                await ReplyAsync("Insufficient permissions.");
            }
        }

        [Discord.Commands.Command("runtime")]
        [Discord.Commands.Summary("Fetches bot runtime info.")]
        public async Task RuntimeAsync()
        {
            log.Debug("\"runtime\" was called!");

            log.Debug("Fetching build configuration...");
            var buildConfiguration = "UNKNOWN";
#if DEBUG
            buildConfiguration = "DEBUG";
#elif RELEASE
            buildConfiguration = "RELEASE";
#endif

            var commitSha = "";

            log.Debug("Fetching git HEAD sha...");
            try
            {
                log.Debug("Constructing process...");
                using (var p = new System.Diagnostics.Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = "git";
                    p.StartInfo.Arguments = "rev-parse HEAD";

                    log.Debug("Starting process...");
                    p.Start();

                    log.Debug("Reading from process...");
                    commitSha = p.StandardOutput.ReadToEnd();

                    log.Debug("Awaiting process exit...");
                    p.WaitForExit();
                    log.Debug("Successfully exited process.");
                }
            }
            catch
            {
                log.Error("Git is not installed. Command will not have full output. Replying with request to install git...");
                await ReplyAsync("Please install git on the host server for full runtime output.");
            }

            log.Debug("Replying...");
            await ReplyAsync($"Running {Program.Config.BotName} v{Program.Version}{(commitSha != "" ? $" {commitSha[0..7]}" : "")} on .NET {Environment.Version} on {System.Net.Dns.GetHostName()} with build configuration {buildConfiguration}.");
        }

        [Discord.Commands.Command("save")]
        [Discord.Commands.Summary("(Developer only) Saves memory objects to their destination files.")]
        public async Task SaveAsync()
        {
            log.Debug("\"save\" was called!");

            if (Program.Config.DeveloperIds.Contains(Context.User.Id))
            {
                try
                {
                    Program.CommandTracker.Save();
                }
                catch (NullReferenceException)
                {
                    log.Warn("Bot is not configured to track command invokations.");
                }

                await ReplyAsync("Everything has been saved.");
            }
            else
            {
                log.Debug("Caller is not listed as a developer. Doing nothing...");
                await ReplyAsync("Insufficient permissions.");
            }
        }

        [Discord.Commands.Command("topcmd")]
        [Discord.Commands.Summary("Lists out most used commands.")]
        public async Task TopCmdAsync()
        {
            log.Debug("\"topcmd\" was called!");

            if (Program.Config.TrackInvokedCommands)
            {
                await ReplyAsync($"```{Program.CommandTracker.Invokations.Where(c => Program.Commands.Commands.Select(x => x.Name).Contains(c.Key)).OrderByDescending(c => c.Value).Select(c => $"[{c.Value}] {c.Key}").Aggregate((a, b) => a + "\n" + b)}```"); // C# baby
            }
            else
            {
                log.Debug($"{nameof(Program.Config.TrackInvokedCommands)} is not enabled. Returning...");
                await ReplyAsync("Command tracking is disabled in the bot config.");
            }
        }

        [Discord.Commands.Command("ping")]
        [Discord.Commands.Summary("Gets the bot's ping.")]
        public async Task PingAsync()
        {
            await ReplyAsync($"{Context.Client.Latency}ms");
        }
    }
}

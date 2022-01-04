namespace Leirosa
{
    public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private async Task<Discord.Color> GetUserColor(Discord.WebSocket.SocketGuildUser user)
        {
            _log.Debug("A color to be used with an embed targeting a user has been requested.");

            var color = new Discord.Color(0, 0, 0);

            if (bool.Parse(Program.config["embed_color_from_user_avatar"]))
            {
                _log.Debug("Config opted to calculate embed color via user avatar.");

                // Some bullshit to get the average color of the user icon
                using (var client = new HttpClient())
                {
                    _log.Debug("Retrieving user icon...");
                    var response = await client.GetAsync(user.GetAvatarUrl());
                    _log.Debug("Converting HTTP response to image...");
                    var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
                    var img = System.Drawing.Image.FromStream(ms);
                    var bmp = new System.Drawing.Bitmap(1, 1);

                    _log.Debug("Interpolating (averaging) image color...");
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img, new System.Drawing.Rectangle(0, 0, 1, 1));
                    }

                    var pixel = bmp.GetPixel(0, 0);
                    color = new Discord.Color(pixel.R, pixel.G, pixel.B);
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
                        color = role.Color;
                        break;
                    }
                }

                if (color.R == 0 && color.G == 0 && color.B == 0)
                {
                    _log.Debug("No role color was found for the embed. Setting the color to 200, 200, 200...");
                    color = new Discord.Color(200, 200, 200);
                }
            }

            return color;
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
            var commands = Program.commands.Commands.ToList();
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
            await ReplyAsync(Program.config["source"]);
        }

        [Discord.Commands.Command("invite")]
        [Discord.Commands.Summary("Prints invite URL to invite me into other servers.")]
        public async Task InviteAsync()
        {
            _log.Debug("\"invite\" was called!");
            await ReplyAsync(Program.config["invite"]);
        }

        [Discord.Commands.Command("userinfo")]
        [Discord.Commands.Alias("whois")]
        [Discord.Commands.Summary("[user (optional)] Prints data about you or somebody else.")]
        public async Task WhoisAsync(Discord.WebSocket.SocketGuildUser user = null) // May fail when pinging other users
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
            .AddField("Nickname", user.Nickname != null ? user.Nickname : "None", true)
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
        public async Task ServerInfoAsync()
        {
            _log.Debug("\"serverinfo\" was called!");

            var color = new Discord.Color(0, 0, 0);

            // Some bullshit to get the average color of the server icon
            using (var client = new HttpClient())
            {
                _log.Debug("Retrieving server icon...");
                var response = await client.GetAsync(Context.Guild.IconUrl);
                _log.Debug("Converting HTTP response to image...");
                var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
                var img = System.Drawing.Image.FromStream(ms);
                var bmp = new System.Drawing.Bitmap(1, 1);

                _log.Debug("Interpolating (averaging) image color...");
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, 1, 1));
                }

                var pixel = bmp.GetPixel(0, 0);
                color = new Discord.Color(pixel.R, pixel.G, pixel.B);
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(color)
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

            using (var writer = File.AppendText(Program.config["suggestions_path"]))
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

            using (var writer = File.AppendText(Program.config["reports_path"]))
            {
                writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {report}");
            }

            await ReplyAsync("Thank you for your error report!");
        }

        [Discord.Commands.Command("maintenance")]
        [Discord.Commands.Summary("Maintenance function that solves some problems.")]
        public async Task MaintenanceAsync()
        {
            _log.Debug("\"maintenance\" was called!");

            _log.Debug($"Downloading users in {Context.Guild.Id}.");
            await Context.Guild.DownloadUsersAsync();

            await ReplyAsync("Maintenance complete!");
        }

        [Discord.Commands.Command("echo")]
        [Discord.Commands.Summary("[text (remainder)] Echoes what you say.")]
        public async Task EchoAsync([Discord.Commands.Remainder]string text)
        {
            _log.Debug("\"echo\" was called!");

            _log.Debug("Deleting caller...");
            await Context.Message.DeleteAsync();

            _log.Debug("Replying...");
            await ReplyAsync(text);
        }
    }
}

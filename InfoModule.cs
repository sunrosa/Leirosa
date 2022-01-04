namespace Mailwash
{
    public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

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
            await ReplyAsync("https://github.com/sunrosa/Mailwash");
        }

        [Discord.Commands.Command("invite")]
        [Discord.Commands.Summary("Prints invite URL to invite me into other servers.")]
        public async Task InviteAsync()
        {
            _log.Debug("\"invite\" was called!");
            await ReplyAsync("https://discord.com/api/oauth2/authorize?client_id=927336569709424661&permissions=8&scope=bot");
        }

        [Discord.Commands.Command("userinfo")]
        [Discord.Commands.Alias("whois")]
        [Discord.Commands.Summary("Prints data about you or somebody else (somewhat broken).")]
        public async Task WhoisAsync(Discord.WebSocket.SocketGuildUser user = null) // May fail when pinging other users
        {
            _log.Debug("\"whois\" was called!");

            if (user == null)
            {
                _log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            var color = System.Drawing.Color.White;

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

                color = bmp.GetPixel(0, 0);
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(new Discord.Color(color.R, color.G, color.B))
            .AddField("Name", user.Username, true)
            .AddField("Nickname", user.Nickname != null ? user.Nickname : "None", true)
            .AddField("Id", user.Id, true)
            .AddField("Account created", user.CreatedAt, true)
            .AddField("Joined", user.JoinedAt, true);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("serverinfo")]
        [Discord.Commands.Summary("Prints general server info.")]
        public async Task ServerInfoAsync()
        {
            _log.Debug("\"serverinfo\" was called!");

            var color = System.Drawing.Color.White;

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

                color = bmp.GetPixel(0, 0);
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.WithColor(new Discord.Color(color.R, color.G, color.B))
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

        [Discord.Commands.Command("suggest")]
        [Discord.Commands.Summary("Where you can suggest features.")]
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
        [Discord.Commands.Summary("Where you can report bugs.")]
        public async Task ReportAsync([Discord.Commands.Remainder]string report)
        {
            _log.Debug("\"report\" was called!");

            using (var writer = File.AppendText(Program.config["reports_path"]))
            {
                writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {report}");
            }

            await ReplyAsync("Thank you for your error report!");
        }
    }
}

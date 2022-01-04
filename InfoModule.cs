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

        [Discord.Commands.Command("whois")]
        [Discord.Commands.Summary("Prints data about you or somebody else (somewhat broken).")]
        public async Task WhoisAsync(Discord.WebSocket.SocketGuildUser user = null) // May fail when pinging other users
        {
            _log.Debug("\"whois\" was called!");

            if (user == null)
            {
                _log.Debug("No target. Setting target user to self...");
                user = Context.User as Discord.WebSocket.SocketGuildUser;
            }

            _log.Debug("Building embed...");
            var embed = new Discord.EmbedBuilder();
            embed.AddField("Name", user.Username)
            .AddField("Nickname", user.Nickname != null ? user.Nickname : "None")
            .AddField("Id", user.Id)
            .AddField("Account created", user.CreatedAt)
            .AddField("Joined", user.JoinedAt);

            _log.Debug("Replying...");
            await ReplyAsync(embed: embed.Build());
        }

        [Discord.Commands.Command("serverinfo")]
        [Discord.Commands.Summary("Prints general server info.")]
        public async Task ServerInfoAsync()
        {
            var color = System.Drawing.Color.White;

            // Some bullshit to get the average color of the server icon
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(Context.Guild.IconUrl);
                var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
                var img = System.Drawing.Image.FromStream(ms);
                var bmp = new System.Drawing.Bitmap(1, 1);

                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, new System.Drawing.Rectangle(0, 0, 1, 1));
                }

                color = bmp.GetPixel(0, 0);
            }

            var embed = new Discord.EmbedBuilder();
            embed.WithColor(new Discord.Color(color.R, color.G, color.B))
            .AddField("Name", Context.Guild.Name)
            .AddField("Id", Context.Guild.Id)
            .AddField("Members", Context.Guild.MemberCount)
            .AddField("Owner", await Context.Client.GetUserAsync(Context.Guild.OwnerId)) /* For some fucking reason, Context.Guild.Owner.Username is null, so we do this instead. */
            .AddField("Created", Context.Guild.CreatedAt)
            .AddField("Verification", Context.Guild.VerificationLevel)
            .AddField("Roles", Context.Guild.Roles.Count)
            .AddField("Text channels", Context.Guild.TextChannels.Count)
            .AddField("Voice channels", Context.Guild.VoiceChannels.Count)
            .AddField("Active threads", Context.Guild.ThreadChannels.Count)
            .AddField("Boosts", Context.Guild.PremiumSubscriptionCount);

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

public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("help")]
    [Discord.Commands.Summary("Provides command info.")]
    public async Task HelpAsync()
    {
        var commands = Program.commands.Commands.ToList();
        var output = "```\n";
        foreach (var command in commands)
        {
            output += $"{command.Name}: {command.Summary}\n";
        }
        output += "\n```";

        await ReplyAsync(output);
    }

    [Discord.Commands.Command("invite")]
    [Discord.Commands.Summary("Prints invite URL to invite this bot into other servers.")]
    public async Task InviteAsync()
    {
        await ReplyAsync("https://discord.com/api/oauth2/authorize?client_id=927336569709424661&permissions=8&scope=bot");
    }

    [Discord.Commands.Command("whois")]
    [Discord.Commands.Summary("Prints data about you or somebody else.")]
    public async Task WhoisAsync(Discord.WebSocket.SocketGuildUser user = null) // May fail when pinging other users
    {
        if (user == null) user = Context.User as Discord.WebSocket.SocketGuildUser;

        var embed = new Discord.EmbedBuilder();
        embed.AddField("Name", user.Username)
        .AddField("Nickname", user.Nickname != null ? user.Nickname : "None")
        .AddField("Id", user.Id)
        .AddField("Account created", user.CreatedAt)
        .AddField("Joined", user.JoinedAt);

        await ReplyAsync(embed: embed.Build());
    }

    [Discord.Commands.Command("suggest")]
    [Discord.Commands.Summary("Where you can suggest features.")]
    public async Task SuggestAsync([Discord.Commands.Remainder]string suggestion)
    {
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
        using (var writer = File.AppendText(Program.config["reports_path"]))
        {
            writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {report}");
        }

        await ReplyAsync("Thank you for your error report!");
    }
}

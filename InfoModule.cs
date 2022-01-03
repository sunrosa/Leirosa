public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("help")]
    public async Task HelpAsync()
    {
        await ReplyAsync("help!");
    }

    [Discord.Commands.Command("invite")]
    public async Task InviteAsync()
    {
        await ReplyAsync("https://discord.com/api/oauth2/authorize?client_id=927336569709424661&permissions=8&scope=bot");
    }

    [Discord.Commands.Command("whois")]
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
    public async Task SuggestAsync([Discord.Commands.Remainder]string suggestion)
    {
        using (var writer = File.AppendText(Program.config["suggestions_path"]))
        {
            writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {suggestion}");
        }

        await ReplyAsync("Thank you for your suggestion!");
    }

    [Discord.Commands.Command("report")]
    public async Task ReportAsync([Discord.Commands.Remainder]string report)
    {
        using (var writer = File.AppendText(Program.config["reports_path"]))
        {
            writer.WriteLine($"{DateTime.Now} - {Context.User.Username} [{Context.User.Id}] - {report}");
        }

        await ReplyAsync("Thank you for your error report!");
    }
}

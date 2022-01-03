public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("help")]
    public async Task HelpAsync()
    {
        await Context.Channel.SendMessageAsync("help!");
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
}

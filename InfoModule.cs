public class InfoModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("help")]
    public async Task HelpAsync()
    {
        await Context.Channel.SendMessageAsync("help!");
    }
}

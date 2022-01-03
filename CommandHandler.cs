public class CommandHandler // The command handler as copied from the docs
{
    private readonly Discord.WebSocket.DiscordSocketClient _client;
    private readonly Discord.Commands.CommandService _commands;

    // Retrieve client and CommandService instance via ctor
    public CommandHandler(Discord.WebSocket.DiscordSocketClient client, Discord.Commands.CommandService commands)
    {
        _commands = commands;
        _client = client;
    }

    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        _client.MessageReceived += HandleCommandAsync;

        // Here we discover all of the command modules in the entry
        // assembly and load them. Starting from Discord.NET 2.0, a
        // service provider is required to be passed into the
        // module registration method to inject the
        // required dependencies.
        //
        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
        await _commands.AddModulesAsync(assembly: System.Reflection.Assembly.GetEntryAssembly(),
                                        services: null);
    }

    private async Task HandleCommandAsync(Discord.WebSocket.SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as Discord.WebSocket.SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins. If you set it to 0, the prefix will be included in the call.
        int argPos = 1;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!message.Content.StartsWith(".") || message.Author.IsBot) return;

        // Create a WebSocket-based command context based on the message
        var context = new Discord.Commands.SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        var result = await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: null);

        if (!result.IsSuccess)
        {
            switch(result.Error)
            {
                case Discord.Commands.CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync("That command does not exist.");
                    break;
                case Discord.Commands.CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync("Bad parameters. See help for instructions on how to use the command.");
                    break;
                default:
                    await context.Channel.SendMessageAsync(result.ToString());
                    break;
            }
        }
    }
}

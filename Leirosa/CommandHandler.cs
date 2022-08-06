namespace Leirosa
{
    public class CommandHandler // The command handler as copied from the docs
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private readonly Discord.WebSocket.DiscordSocketClient client;
        private readonly Discord.Commands.CommandService commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(Discord.WebSocket.DiscordSocketClient client, Discord.Commands.CommandService commands)
        {
            log.Debug("Constructing CommandHandler...");
            this.commands = commands;
            this.client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            log.Debug("Hooking Client.MessageReceived into HandleCommandAsync...");
            client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            log.Debug("Reflecting for commands...");
            await commands.AddModulesAsync(assembly: System.Reflection.Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(Discord.WebSocket.SocketMessage messageParam)
        {
            log.Debug("Message received!");

            // Don't process the command if it was a system message
            var message = messageParam as Discord.WebSocket.SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins. If you set it to 0, the prefix will be included in the call.
            var argPos = Program.Config.Prefix.Length;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.Content.StartsWith(Program.Config.Prefix) || message.Content.Length == 1 || message.Content[1] == Convert.ToChar(Program.Config.Prefix) || message.Author.IsBot)
            {
                log.Debug("Message was not bot command. Exiting command sequence...");
                return;
            }
            log.Debug("Message was bot command. Continuing command sequence...");

            // Create a WebSocket-based command context based on the message
            var context = new Discord.Commands.SocketCommandContext(client, message);
            if (!context.IsPrivate)
            {
                log.Debug($"Context created. Command was \"{context.Message.Content}\" sent by {context.User.Username} ({context.User.Id}) in channel {context.Channel.Id} in guild {context.Guild.Id}.");
            }
            else
            {
                log.Debug($"Context created. Command was \"{context.Message.Content}\" sent by {context.User.Username} ({context.User.Id}) in channel {context.Channel.Id} (DM).");
            }

            if (Program.Config.TrackInvokedCommands) Program.CommandTracker.TrackCommand(context, argPos);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            log.Debug("Executing command...");
            var result = await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

            if (!result.IsSuccess)
            {
                log.Debug($"Command had error \"{result.Error}: {result.ErrorReason}\"");
                switch(result.Error)
                {
                    case Discord.Commands.CommandError.UnknownCommand:
                        await context.Channel.SendMessageAsync("That command does not exist.");
                        break;
                    case Discord.Commands.CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("Bad parameters. See help for instructions on how to use the command.");
                        break;
                    case Discord.Commands.CommandError.ObjectNotFound:
                        await context.Channel.SendMessageAsync("User, role, or channel not found.");
                        break;
                    default:
                        log.Debug("No help text was available for the error at hand. Sending error itself as a message...");
                        await context.Channel.SendMessageAsync($"{result.Error.GetType()}: {result.ErrorReason}");
                        break;
                }
            }
        }
    }
}

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public static Discord.WebSocket.DiscordSocketClient client;
    public static Discord.Commands.CommandService commands;
    public static Dictionary<string, string> config;

    private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

    public async Task MainAsync()
    {
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName="log.log"};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
        }

        _log.Info("Setup logger. Beginning of MainAsync().");

        _log.Debug("Creating client...");
        client = new Discord.WebSocket.DiscordSocketClient();
        client.Log += Log;

        _log.Debug("Parsing workspace config...");
        config = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("config.json"));

        var token = config["token"];

        _log.Debug("Logging in...");
        await client.LoginAsync(Discord.TokenType.Bot, token);
        await client.StartAsync();

        client.Ready += Ready; // Call Ready() when the client is ready.

        await Task.Delay(-1); // Block the thread to prevent the program from closing (infinite wait)
    }

    private async Task Ready()
    {
        _log.Debug("Client ready. Initializing CommandService...");
        commands = new Discord.Commands.CommandService();
        _log.Debug("CommandService ready. Initializing CommandHandler...");
        var command_handler = new CommandHandler(client, commands);
        _log.Debug("Installing commands...");
        await command_handler.InstallCommandsAsync();
    }

    private Task Log(Discord.LogMessage msg)
    {
        _log.Info(msg.ToString());
        return Task.CompletedTask;
    }
}

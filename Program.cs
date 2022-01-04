namespace Mailwash
{
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
            client = new Discord.WebSocket.DiscordSocketClient(new Discord.WebSocket.DiscordSocketConfig(){
                LogLevel = Discord.LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                GatewayIntents = Discord.GatewayIntents.All, // VERY IMPORTANT. SHIT DOESN'T WORK WITHOUT THIS. The Discord API basically neglects to send us shit (most notably SocketGuildUsers), unless we have all intents SPECIFIED in our config.
                LogGatewayIntentWarnings = false
            });
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

            if (bool.Parse(config["use_custom_status"]))
            {
                _log.Debug("Setting custom status...");
                await client.SetActivityAsync(new Discord.Game(config["status"])); // Apparently you can't set custom statuses for bots, so this is the best we can do.
            }
        }

        private Task Log(Discord.LogMessage msg)
        {
            switch (msg.Severity)
            {
                case Discord.LogSeverity.Debug:
                case Discord.LogSeverity.Verbose:
                    _log.Debug(msg.ToString());
                break;
                case Discord.LogSeverity.Info:
                    _log.Info(msg.ToString());
                break;
                case Discord.LogSeverity.Warning:
                    _log.Warn(msg.ToString());
                break;
                case Discord.LogSeverity.Error:
                    _log.Error(msg.ToString());
                break;
                case Discord.LogSeverity.Critical:
                    _log.Fatal(msg.ToString());
                break;
                default:
                    _log.Info(msg.ToString());
                break;
            }

            return Task.CompletedTask;
        }
    }
}

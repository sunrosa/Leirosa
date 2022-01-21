// Warning: This bot is designed to be private, used only within one guild per runtime instance.
namespace Leirosa
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        public static Discord.WebSocket.DiscordSocketClient Client {get; set;}
        public static Discord.Commands.CommandService Commands {get; set;}
        public static Data.Config Config {get; private set;}
        public static CommandTracker? CommandTracker {get; set;}

        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private bool _isReadied = false;

        public async Task MainAsync()
        {
            {
                var config = new NLog.Config.LoggingConfiguration();
                var logfile = new NLog.Targets.FileTarget("logfile"){FileName="log.log"}; // TODO: Set layout property to include method names in log entries
                var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

#if RELEASE
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
#else
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
#endif

                NLog.LogManager.Configuration = config;
            }

            _log.Info("Setup logger. Beginning of MainAsync().");

#if RELEASE
            _log.Info("Running in RELEASE.");
            _log.Info("File log level is INFO. Console log level is INFO.");
#else
            _log.Info("Running in DEBUG.");
            _log.Info("File log level is DEBUG. Console log level is INFO.");
#endif

            _log.Debug("Creating client...");
            Client = new Discord.WebSocket.DiscordSocketClient(new Discord.WebSocket.DiscordSocketConfig(){
                LogLevel = Discord.LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                GatewayIntents = Discord.GatewayIntents.All, // VERY IMPORTANT. SHIT DOESN'T WORK WITHOUT THIS. The Discord API basically neglects to send us shit (most notably SocketGuildUsers), unless we have all intents SPECIFIED in our config.
                LogGatewayIntentWarnings = false
            });
            Client.Log += Log;

            _log.Debug("Parsing workspace config...");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.Config>(File.ReadAllText("config.json"));

            _log.Debug("Logging in...");
            await Client.LoginAsync(Discord.TokenType.Bot, Config.Token);
            await Client.StartAsync();

            Client.Ready += Ready; // Call Ready() when the client is ready.

            if (Config.TrackInvokedCommands) Program.CommandTracker = new CommandTracker(Config.CommandTrackerPath);

            await Task.Delay(-1); // Block the thread to prevent the program from closing (infinite wait)
        }

        public static void Shutdown()
        {
            _log.Info("Exiting cleanly...");
            Environment.Exit(0);
        }

        private async Task Ready()
        {
            if (_isReadied)
            {
                _log.Info("Already readied once. Returning out of Ready()...");
                return;
            }

            _log.Debug("Client ready. Initializing CommandService...");
            Commands = new Discord.Commands.CommandService();
            _log.Debug("CommandService ready. Initializing CommandHandler...");
            var commandHandler = new CommandHandler(Client, Commands);
            _log.Debug("Installing commands...");
            await commandHandler.InstallCommandsAsync();

            if (Config.UseCustomStatus)
            {
                _log.Debug("Setting custom status...");
                await Client.SetActivityAsync(new Discord.Game(Config.Status)); // Apparently you can't set custom statuses for bots, so this is the best we can do.
            }
            _isReadied = true;
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

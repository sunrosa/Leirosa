﻿// Warning: This bot is designed on a custom basis per guild (server). One runtime will work on ONLY ONE guild!!! This branch is custom-built for a server titled "Rabbit Island" with the ID 485003271468089365, owned by Mint S. Decot and Ibis Sunrosa (developer).

namespace Leirosa
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        public const string Version = "Rabbit-v1.0.0";

        public static string ExecutingPath {get; set;}
        public static string ConfigPath {get; set;} = "Config.json";

#if RELEASE
        public static bool Release {get;} = true;
#else
        public static bool Release {get;} = false;
#endif

        public static Discord.WebSocket.DiscordSocketClient Client {get; set;}
        public static Discord.Commands.CommandService Commands {get; set;}
        public static Data.Config Config {get; private set;}
        public static CommandTracker? CommandTracker {get; set;}

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private bool isReadied = false;

        public async Task MainAsync()
        {
            ExecutingPath = AppDomain.CurrentDomain.BaseDirectory;

            // Parse workspace config
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<Data.Config>(File.ReadAllText($"{ExecutingPath}/{ConfigPath}"));
            Config.ExecutingPath = ExecutingPath;
            ValidateConfig(Config);

            {
                var logConfig = new NLog.Config.LoggingConfiguration();
                var logFile = new NLog.Targets.FileTarget("logfile"){FileName=$"{Config.DataPath}/{Config.LogName}", Layout="${longdate}|${level}|${callsite}:${callsite-linenumber}|${threadid}|${message}"}; // TODO: Set layout property to include method names in log entries
                var logConsole = new NLog.Targets.ConsoleTarget("logconsole");

                if (Release)
                {
                    var logLevel = NLog.LogLevel.Info;
                    switch (Config.LogLevel ?? "")
                    {
                        case ("Debug"):
                            logLevel = NLog.LogLevel.Debug;
                        break;
                        case ("Info"):
                            logLevel = NLog.LogLevel.Info;
                        break;
                        case ("Warn"):
                            logLevel = NLog.LogLevel.Warn;
                        break;
                        case ("Error"):
                            logLevel = NLog.LogLevel.Error;
                        break;
                        case ("Fatal"):
                            logLevel = NLog.LogLevel.Fatal;
                        break;
                    }
                    logConfig.AddRule(logLevel, NLog.LogLevel.Fatal, logFile);
                    logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole);
                }
                else
                {
                    var logLevel = NLog.LogLevel.Debug;
                    switch (Config.LogLevel ?? "")
                    {
                        case ("Debug"):
                            logLevel = NLog.LogLevel.Debug;
                        break;
                        case ("Info"):
                            logLevel = NLog.LogLevel.Info;
                        break;
                        case ("Warn"):
                            logLevel = NLog.LogLevel.Warn;
                        break;
                        case ("Error"):
                            logLevel = NLog.LogLevel.Error;
                        break;
                        case ("Fatal"):
                            logLevel = NLog.LogLevel.Fatal;
                        break;
                    }
                    logConfig.AddRule(logLevel, NLog.LogLevel.Fatal, logFile);
                    logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logConsole);
                }

                NLog.LogManager.Configuration = logConfig;
            }

            log.Info("Logger setup.");

            if (Release)
            {
                log.Info("Running in RELEASE.");
            }
            else
            {
                log.Info("Running in DEBUG.");
            }

            log.Info("Checking (and creating if necessary) directory in which to store bot runtime data...");
            Directory.CreateDirectory($"{ExecutingPath}/{Config.DataPath}");

            log.Debug("Creating client...");
            Client = new Discord.WebSocket.DiscordSocketClient(new Discord.WebSocket.DiscordSocketConfig(){
                LogLevel = Discord.LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                GatewayIntents = Discord.GatewayIntents.All, // VERY IMPORTANT. SHIT DOESN'T WORK WITHOUT THIS. The Discord API basically neglects to send us shit (most notably SocketGuildUsers), unless we have all intents SPECIFIED in our config.
                LogGatewayIntentWarnings = false
            });
            Client.Log += ClientLog;

            log.Debug("Logging in...");
            await Client.LoginAsync(Discord.TokenType.Bot, Config.Token);
            await Client.StartAsync();

            Client.Ready += Ready; // Call Ready() when the client is ready.

            if (Config.TrackInvokedCommands) Program.CommandTracker = new CommandTracker(Program.Config.CommandTrackerPath);

            await Task.Delay(-1); // Block the thread to prevent the program from closing (infinite wait)
        }

        private async Task Ready()
        {
            if (isReadied)
            {
                log.Warn("Already readied once. Returning out of Ready()...");
                return;
            }

            log.Debug("Client ready. Initializing CommandService...");
            Commands = new Discord.Commands.CommandService();
            log.Debug("CommandService ready. Initializing CommandHandler...");
            var commandHandler = new CommandHandler(Client, Commands);
            log.Info("Installing commands...");
            await commandHandler.InstallCommandsAsync();

            if (Config.UseCustomStatus)
            {
                log.Debug("Setting custom status...");
                await Client.SetActivityAsync(new Discord.Game(Config.Status)); // Apparently you can't set custom statuses for bots, so this is the best we can do.
            }
            isReadied = true;
        }

        public static void Shutdown()
        {
            log.Info("Exiting cleanly...");
            Environment.Exit(0);
        }

        private Task ClientLog(Discord.LogMessage msg)
        {
            switch (msg.Severity)
            {
                case Discord.LogSeverity.Debug:
                case Discord.LogSeverity.Verbose:
                    log.Debug(msg.ToString());
                break;
                case Discord.LogSeverity.Info:
                    log.Info(msg.ToString());
                break;
                case Discord.LogSeverity.Warning:
                    log.Warn(msg.ToString());
                break;
                case Discord.LogSeverity.Error:
                    log.Error(msg.ToString());
                break;
                case Discord.LogSeverity.Critical:
                    log.Fatal(msg.ToString());
                break;
                default:
                    log.Info(msg.ToString());
                break;
            }

            return Task.CompletedTask;
        }

        public void ValidateConfig(Leirosa.Data.Config config)
        {
            if (config.DataPath == null || config.DataPath == "")
                throw new ConfigException("DataPath must be configured.");
            if (config.LogName == null || config.LogName == "")
                throw new ConfigException("LogName must be configured.");
            if (config.SuggestionsPath == null || config.SuggestionsPath == "")
                throw new ConfigException("SuggestionsPath must be configured.");
            if (config.ReportsPath == null || config.ReportsPath == "")
                throw new ConfigException("ReportsPath must be configured.");
            if (config.Prefix == null || config.Prefix == "")
                throw new ConfigException("Prefix must be configured.");
            if (config.UseCustomStatus && (config.Status == null || config.Status == ""))
                throw new ConfigException("Must configure Status if UseCustomStatus is true.");
            if (config.BotName == null || config.BotName == "")
                throw new ConfigException("BotName must be configured.");
            if (config.TrackInvokedCommands && (config.CommandTrackerPath == null || config.CommandTrackerPath == ""))
                throw new ConfigException("Must configure CommandTrackerPath if TrackInvokedCommands is true.");
        }
    }
}

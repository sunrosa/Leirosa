namespace Leirosa
{
    /// <summary>
    /// Tracks command invokation counts to a json file
    /// </summary>
    public class CommandTracker
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Timer that periodically calls <see cref="Save"/>
        /// </summary>
        private System.Timers.Timer saveTimer;

        /// <summary>
        ///
        /// </summary>
        /// <param name="filepath">Filepath to save invokation data to</param>
        /// <param name="saveInterval">Interval in milliseconds to automatically save the invokation data to file</param>
        public CommandTracker(string filepath, double saveInterval = 60 * 60 * 1000)
        {
            log.Debug($"Constructing a new {nameof(CommandTracker)}...");

            Filepath = filepath;

            try
            {
            log.Debug($"Deserializing json to {nameof(Invokations)}...");
            Invokations = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, ulong>>(File.ReadAllText(Filepath));
            }
            catch (FileNotFoundException)
            {
                Invokations = new Dictionary<string, ulong>();
            }

            saveTimer = new System.Timers.Timer(saveInterval);

            saveTimer.Elapsed += Save;
            AppDomain.CurrentDomain.ProcessExit += Save;

            saveTimer.Start();
        }

        /// <summary>
        /// Save <see cref="Invokations"/> to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Save(object? sender = null, EventArgs? e = null)
        {
            log.Info($"Serializing {nameof(Invokations)} and saving to {Filepath}...");
            File.WriteAllText(Filepath, Newtonsoft.Json.JsonConvert.SerializeObject(Invokations));
        }

        /// <summary>
        /// Filepath to track to
        /// </summary>
        /// <value></value>
        public string Filepath {get;}

        /// <summary>
        /// Commands and their invokation counts
        /// </summary>
        /// <value></value>
        public Dictionary<string, ulong> Invokations {get;}

        /// <summary>
        /// Tracks a command invokation
        /// </summary>
        /// <param name="context">Invokation context</param>
        /// <param name="argPos">Number of characters to remove from the beginning of the context message to trim off the prefix</param>
        public void TrackCommand(Discord.Commands.SocketCommandContext context, int argPos)
        {
            var command = context.Message.Content.Substring(argPos).Split(' ')[0];
            Invokations[command] =  Invokations.GetValueOrDefault(command, Convert.ToUInt64(0)) + 1;
        }
    }
}

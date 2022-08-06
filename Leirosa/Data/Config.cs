namespace Leirosa.Data
{
    /// <summary>
    /// Leirosa config.
    /// </summary>
    public class Config : IConfig
    {
        /// <summary>
        /// Path of the executing program.
        /// </summary>
        /// <value></value>
        public string ExecutingPath {private get; set;}

        /// <inheritdoc/>
        public string DataPath {get; set;}

        /// <inheritdoc/>
        public string LogName {get; set;}

        /// <inheritdoc/>
        public string Token {get; set;}

        /// <inheritdoc/>
        public string SuggestionsPath
        {
            get
            {
                return $"{ExecutingPath}/{DataPath}/{suggestionsPath}";
            }
            set
            {
                suggestionsPath = value;
            }
        }
        private string suggestionsPath;

        /// <inheritdoc/>
        public string ReportsPath
        {
            get
            {
                return $"{ExecutingPath}/{DataPath}/{reportsPath}";
            }
            set
            {
                reportsPath = value;
            }
        }
        private string reportsPath;

        /// <inheritdoc/>
        public string VRChatPath
        {
            get
            {
                return $"{ExecutingPath}/{DataPath}/{vrchatPath}";
            }
            set
            {
                vrchatPath = value;
            }
        }
        private string vrchatPath;

        /// <inheritdoc/>
        public ulong VRChatRoleId {get; set;}

        /// <inheritdoc/>
        public string DefaultGelbooruTags {get; set;}

        /// <inheritdoc/>
        public string Prefix {get; set;}

        /// <inheritdoc/>
        public bool EmbedColorFromUserAvatar {get; set;}

        /// <inheritdoc/>
        public string Status {get; set;}

        /// <inheritdoc/>
        public bool UseCustomStatus {get; set;}

        /// <inheritdoc/>
        public string InviteURL {get; set;}

        /// <inheritdoc/>
        public string SourceURL {get; set;}

        /// <inheritdoc/>
        public string BotName {get; set;}

        /// <inheritdoc/>
        public List<ulong> DeveloperIds {get; set;}

        /// <inheritdoc/>
        public string DatetimeFormat {get; set;}

        /// <inheritdoc/>
        public bool TrackInvokedCommands {get; set;}

        /// <inheritdoc/>
        public string CommandTrackerPath
        {
            get
            {
                return $"{ExecutingPath}/{DataPath}/{commandTrackerPath}";
            }
            set
            {
                commandTrackerPath = value;
            }
        }
        private string commandTrackerPath;

        /// <inheritdoc/>
        public string LogLevel {get; set;}

        /// <inheritdoc/>
        public ulong HornyJailRoleId {get; set;}

        /// <inheritdoc/>
        public bool ApplyHornyJail {get; set;}

        /// <inheritdoc/>
        public ulong FeetAppreciatorRoleId {get; set;}

        /// <inheritdoc/>
        public ulong FeetPicsChannelId {get; set;}

        /// <inheritdoc/>
        public bool ApplyFeetAppreciator {get; set;}
    }
}
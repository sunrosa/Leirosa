namespace Leirosa.Data
{
    /// <summary>
    /// Leirosa config model.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Discord API token used to log in to the bot.
        /// </summary>
        /// <value></value>
        public string Token {get; set;}

        /// <summary>
        /// Path in which to put files for Suggestions, Reports, etc...
        /// </summary>
        /// <value></value>
        public string DataPath {get; set;}

        /// <summary>
        /// Filename (not including path) to be used for the log.
        /// </summary>
        /// <value></value>
        public string LogName {get; set;}

        /// <summary>
        /// The filename (not including path) that user suggestions will be written to.
        /// </summary>
        /// <value></value>
        public string SuggestionsPath {get; set;}

        /// <summary>
        /// The filename (not including path) that user error reports will be written to.
        /// </summary>
        /// <value></value>
        public string ReportsPath {get; set;}

        /// <summary>
        /// The filename (not including path) that user VRChat login statuses will be written to.
        /// </summary>
        /// <value></value>
        public string VRChatPath {get; set;}

        /// <summary>
        /// The Discord role ID that will be applied when logging into VRChat and removed when logging out of VRChat.
        /// </summary>
        /// <value></value>
        public ulong VRChatRoleId {get; set;}

        /// <summary>
        /// Gelbooru tags that appended to every query.
        /// </summary>
        /// <value></value>
        public string DefaultGelbooruTags {get; set;}

        /// <summary>
        /// Bot command prefix (used before commands to specify that they're commands).
        /// </summary>
        /// <value></value>
        public string Prefix {get; set;}

        /// <summary>
        /// Whether or not to calculate user-targeted embed colors by their avatar (otherwise calculates by their top role).
        /// </summary>
        /// <value></value>
        public bool EmbedColorFromUserAvatar {get; set;}

        /// <summary>
        /// Bot status to be displayed on the Discord bot's profile if <see cref="UseCustomStatus"/> is true.
        /// </summary>
        /// <value></value>
        public string Status {get; set;}

        /// <summary>
        /// Whether or not to display <see cref="Status"/> on the bot's profile.
        /// </summary>
        /// <value></value>
        public bool UseCustomStatus {get; set;}

        /// <summary>
        /// OAuth2 URL to invite the bot into Discord guilds.
        /// </summary>
        /// <value></value>
        public string InviteURL {get; set;}

        /// <summary>
        /// URL to the bot's source code.
        /// </summary>
        /// <value></value>
        public string SourceURL {get; set;}

        /// <summary>
        /// The bot's display name.
        /// </summary>
        /// <value></value>
        public string BotName {get; set;}

        /// <summary>
        /// Discord IDs of this bot's developers.
        /// </summary>
        /// <value></value>
        public List<ulong> DeveloperIds {get; set;}

        /// <summary>
        /// Format string used when converting DateTime objects to strings.
        /// </summary>
        /// <value></value>
        public string DatetimeFormat {get; set;}

        /// <summary>
        /// Whether or not to count all command invokations and output them into a json file.
        /// </summary>
        /// <value></value>
        public bool TrackInvokedCommands {get; set;}

        /// <summary>
        /// FIlename (not including path) to track command invokations to.
        /// </summary>
        /// <value></value>
        public string CommandTrackerPath {get; set;}

        /// <summary>
        /// Log level to be used by the executing assembly (through NLog).
        /// </summary>
        /// <value></value>
        public string LogLevel {get; set;}

        /// <summary>
        /// ID of the horny jail role.
        /// </summary>
        /// <value></value>
        public ulong HornyJailRoleId {get; set;}

        /// <summary>
        /// Enable to apply the horny jail role to people when they call NSFW commands.
        /// </summary>
        /// <value></value>
        public bool ApplyHornyJail {get; set;}

        /// <summary>
        /// ID of the feet appreciator role.
        /// </summary>
        /// <value></value>
        public ulong FeetAppreciatorRoleId {get; set;}

        /// <summary>
        /// ID of the #feet-pics channel.
        /// </summary>
        /// <value></value>
        public ulong FeetPicsChannelId {get; set;}

        // I swear they made me do this. I didn't want to do this...
        /// <summary>
        /// Enable to apply the feet appreciator role to people who have sent messages in the #feet-pics channel.
        /// </summary>
        /// <value></value>
        public bool ApplyFeetAppreciator {get; set;}
    }
}

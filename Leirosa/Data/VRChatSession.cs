namespace Leirosa.Data
{
    /// <summary>
    /// Bot-tracked VRChat session data
    /// </summary>
    public class VRChatSession
    {
        /// <summary>
        /// User's own descriptor for what they are doing in VRChat
        /// </summary>
        /// <value></value>
        public string? Activity {get; set;} = null;

        /// <summary>
        /// When the user logged into VRChat
        /// </summary>
        /// <value></value>
        public DateTime StartTime {get; set;}

        /// <summary>
        /// When the user last updated their status
        /// </summary>
        /// <value></value>
        public DateTime UpdateTime {get; set;}

        /// <summary>
        /// When the user paused their VRChat session (if they did) to go AFK
        /// </summary>
        /// <value></value>
        public DateTime PauseTime {get; set;}

        /// <summary>
        /// When the user estimates they'll unpause their VRChat session (if they set one)
        /// </summary>
        /// <value></value>
        public DateTime UnpauseTime {get; set;}

        /// <summary>
        /// Whether the user updated their status since the start of the VRChat session
        /// </summary>
        /// <value></value>
        public bool IsUpdated {get; set;} = false;

        /// <summary>
        /// Whether the VRChat session is currently paused
        /// </summary>
        /// <value></value>
        public bool IsPaused {get; set;} = false;
    }
}

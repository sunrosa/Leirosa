namespace Leirosa
{
    public class VRChatSession
    {
        public string? Activity {get; set;} = null;
        public DateTime StartTime {get; set;}
        public DateTime UpdateTime {get; set;}
        public DateTime PauseTime {get; set;}
        public DateTime UnpauseTime {get; set;}
        public bool IsUpdated {get; set;} = false;
        public bool IsPaused {get; set;} = false;
    }
}

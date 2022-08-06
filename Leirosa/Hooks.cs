namespace Leirosa
{
    public class Hooks
    {
        public Hooks()
        {
            if (Program.Config.ApplyFeetAppreciator)
            {
                log.Debug("Hooking FeetAppreciator function to Client.MessageReceived event.");
                Program.Client.MessageReceived += FeetAppreciator;
            }
        }

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public async Task FeetAppreciator(Discord.WebSocket.SocketMessage message)
        {
            var author = (Discord.WebSocket.SocketGuildUser) message.Author;
            if (message.Channel.Id == Program.Config.FeetPicsChannelId && !author.Roles.Any(r => r.Id == Program.Config.FeetAppreciatorRoleId) && Program.Config.ApplyFeetAppreciator)
            {
                log.Debug($"Giving FeetAppreciator role to {author.Username} ({author.Id})...");
                await author.AddRoleAsync(Program.Config.FeetAppreciatorRoleId);
            }
        }
    }
}
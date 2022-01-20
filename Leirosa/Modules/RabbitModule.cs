// Module specific to the guild Rabbit Island (485003271468089365)
namespace Leirosa.Modules
{
    public class RabbitModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        public RabbitModule()
        {
            Program.Client.UserJoined += TiaraAsync;
        }

        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private static ulong _rabbit_island_id = 485003271468089365;
        private static ulong _tiara_id = 704497570579480746;
        private static ulong[] _tiara_role_ids = new ulong[]{800908171538333717, 677648907123032074, 805613829727059980, 923261797803388999};

        // Gives tiara her roles back
        public async Task TiaraAsync(Discord.WebSocket.SocketGuildUser user)
        {
            _log.Debug("A user joined a guild.");

            if (user.Id != _tiara_id || user.Guild.Id != _rabbit_island_id)
            {
                _log.Debug("The user was not Tiara, or the user was not joining Rabbit Island. Returning...");
                return;
            }

            _log.Debug("Giving Tiara her roles back...");
            foreach (var role in _tiara_role_ids)
            {
                await user.AddRoleAsync(role);
            }
        }
    }
}

namespace Leirosa
{
    public static class ModuleHelpers
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        public static async Task<Discord.Color> GetUserColor(Discord.WebSocket.SocketGuildUser user)
        {
            _log.Debug("A color to be used with an embed targeting a user has been requested.");

            var discord_color = new Discord.Color(0, 0, 0);

            if (Program.Config.EmbedColorFromUserAvatar)
            {
                _log.Debug("Config opted to calculate embed color via user avatar.");

                // Some bullshit to get the average color of the user icon
                using (var client = new HttpClient())
                {
                    _log.Debug("Retrieving user icon...");
                    var response = await client.GetAsync(user.GetAvatarUrl());
                    _log.Debug("Converting HTTP response to image...");
                    var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                    using (var image = new ImageMagick.MagickImage(ms))
                    {
                        image.Resize(1, 1);
                        var color = image.GetPixels().First().ToColor();
                        discord_color = new Discord.Color(color.R, color.G, color.B);
                    }
                }
            }
            else
            {
                _log.Debug("Config opted to calculate embed color via user's top role.");

                _log.Debug("Sorting roles...");
                var roles_sorted = user.Roles.OrderBy(x => x.Position).Reverse(); // Roles sorted by position in server

                _log.Debug("Stepping through roles from top to bottom in order to find a color for the embed...");
                foreach (var role in roles_sorted)
                {
                    if (!(role.Color.R == 0 && role.Color.G == 0 && role.Color.B == 0))
                    {
                        _log.Debug("Found a role color for the embed.");
                        discord_color = role.Color;
                        break;
                    }
                }

                if (discord_color.R == 0 && discord_color.G == 0 && discord_color.B == 0)
                {
                    _log.Debug("No role color was found for the embed. Setting the color to 200, 200, 200...");
                    discord_color = new Discord.Color(200, 200, 200);
                }
            }

            return discord_color;
        }

        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{Math.Floor(timeSpan.TotalHours)}:{timeSpan.ToString(@"mm\:ss")}";
        }
    }
}

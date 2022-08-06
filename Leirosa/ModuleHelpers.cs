namespace Leirosa
{
    public static class ModuleHelpers
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public static async Task<Discord.Color> GetUserColor(Discord.WebSocket.SocketGuildUser user)
        {
            log.Debug("A color to be used with an embed targeting a user has been requested.");

            var discordColor = new Discord.Color(0, 0, 0);

            if (Program.Config.EmbedColorFromUserAvatar)
            {
                log.Debug("Config opted to calculate embed color via user avatar.");

                // Some bullshit to get the average color of the user icon
                using (var client = new HttpClient())
                {
                    log.Debug("Retrieving user icon...");
                    var response = await client.GetAsync(user.GetAvatarUrl());
                    log.Debug("Converting HTTP response to image...");
                    var ms = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                    using (var image = new ImageMagick.MagickImage(ms))
                    {
                        image.Resize(1, 1);
                        var color = image.GetPixels().First().ToColor();
                        discordColor = new Discord.Color(color.R, color.G, color.B);
                    }
                }
            }
            else
            {
                log.Debug("Config opted to calculate embed color via user's top role.");

                log.Debug("Sorting roles...");
                var rolesSorted = user.Roles.OrderBy(x => x.Position).Reverse(); // Roles sorted by position in server

                log.Debug("Stepping through roles from top to bottom in order to find a color for the embed...");
                foreach (var role in rolesSorted)
                {
                    if (!(role.Color.R == 0 && role.Color.G == 0 && role.Color.B == 0))
                    {
                        log.Debug("Found a role color for the embed.");
                        discordColor = role.Color;
                        break;
                    }
                }

                if (discordColor.R == 0 && discordColor.G == 0 && discordColor.B == 0)
                {
                    log.Debug("No role color was found for the embed. Setting the color to 200, 200, 200...");
                    discordColor = new Discord.Color(200, 200, 200);
                }
            }

            return discordColor;
        }

        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{Math.Floor(timeSpan.TotalHours)}:{timeSpan.ToString(@"mm\:ss")}";
        }
    }
}

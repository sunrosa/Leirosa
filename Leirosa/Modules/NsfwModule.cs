namespace Leirosa.Modules
{
    public class NsfwModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        [Discord.Commands.Command("gelbooru")]
        [Discord.Commands.Summary("Gets a random file from Gelbooru.")]
        [Discord.Commands.RequireNsfw(Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")] // In an NSFW channel OR DM
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.DM, Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")]
        public async Task GelbooruAsync([Discord.Commands.Summary("Tags to be queried")][Discord.Commands.Name("tags")][Discord.Commands.Remainder]string tagsStr = "")
        {
            try // Bad practice to wrap everything in a try-catch
            {
                _log.Debug("\"gelbooru\" was called!");

                _log.Debug("Obtaining default tags...");
                var defaultTags = Program.Config.DefaultGelbooruTags;

                _log.Debug("Formatting requested tags...");
                tagsStr = tagsStr.Replace(" ", "+"); // Format tags for API request

                _log.Debug("Creating client for request...");
                var client = new HttpClient(); // Create client for request
                _log.Debug("Making request...");
                var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&json=1&tags=sort:random+{defaultTags}+{tagsStr}"); // Make request
                _log.Debug("Parsing request...");
                dynamic responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request
                _log.Debug("Replying with file...");
                await ReplyAsync(responseJson.post[0].file_url.ToString(), messageReference: new Discord.MessageReference(Context.Message.Id)); // Don't touch this sacred bullshit
            }
            catch
            {
                _log.Warn("An exception was thrown. Replying with \"Not found.\"");
                await ReplyAsync("Not found.");
            }
        }

        [Discord.Commands.Command("mgelbooru")]
        [Discord.Commands.Summary("Gets 5 random files from Gelbooru.")]
        [Discord.Commands.RequireNsfw(Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")] // In an NSFW channel OR DM
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.DM, Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")]
        public async Task MGelbooruAsync([Discord.Commands.Summary("Tags to be queried")][Discord.Commands.Name("tags")][Discord.Commands.Remainder]string tagsStr = "")
        {
            try
            {
                _log.Debug("\"mgelbooru\" was called!");

                _log.Debug("Obtaining default tags...");
                var defaultTags = Program.Config.DefaultGelbooruTags;

                _log.Debug("Formatting requested tags...");
                tagsStr = tagsStr.Replace(" ", "+"); // Format tags for API request
                var count = 5; // Discord only embeds 5 images per message. If we wanted to surpass this limit, we would have to have it auto-post into separate consecutive messages.

                _log.Debug("Creating client for request...");
                var client = new HttpClient(); // Create client for request
                _log.Debug("Making request...");
                var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit={count}&json=1&tags=sort:random+{defaultTags}+{tagsStr}"); // Make request
                _log.Debug("Parsing request...");
                dynamic responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request

                _log.Debug("Formatting output...");
                var output = "";
                foreach (var post in responseJson.post)
                {
                    output += post.file_url.ToString() + "\n";
                }

                _log.Debug("Replying with files...");
                await ReplyAsync(output, messageReference: new Discord.MessageReference(Context.Message.Id));
            }
            catch
            {
                _log.Warn("An exception was thrown. Replying with \"Not found.\"");
                await ReplyAsync("Not found.");
            }
        }

        [Discord.Commands.Command("xgelbooru")]
        [Discord.Commands.Summary("Requests 1-25 random files from Gelbooru.")]
        [Discord.Commands.RequireNsfw(Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")] // In an NSFW channel OR DM
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.DM, Group = "Private", ErrorMessage = "Channel must be NSFW or DM.")]
        public async Task XGelbooruAsync([Discord.Commands.Summary("(<25) Number of posts to request")]uint count, [Discord.Commands.Summary("Tags to be queried")][Discord.Commands.Name("tags")][Discord.Commands.Remainder]string tagsStr = "")
        {
            try
            {
                _log.Debug("\"xgelbooru\" was called!");

                if (count > 25)
                {
                    _log.Debug("User requested over 25 files at once. Returning...");
                    await ReplyAsync("Please request less than or equal to 25 files.");
                    return;
                }

                _log.Debug("Obtaining default tags...");
                var defaultTags = Program.Config.DefaultGelbooruTags;

                _log.Debug("Formatting requested tags...");
                tagsStr = tagsStr.Replace(" ", "+"); // Format tags for API request

                _log.Debug("Creating client for request...");
                var client = new HttpClient(); // Create client for request
                _log.Debug("Making request...");
                var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit={count}&json=1&tags=sort:random+{defaultTags}+{tagsStr}"); // Make request
                _log.Debug("Parsing request...");
                dynamic responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request

                // Some crazy bullshit that separates the http response's post links into chunks of 5 and replies with those chunks
                _log.Debug("Formatting output...");
                var i = 0;
                var output = "";
                foreach (var post in responseJson.post)
                {
                    i += 1;
                    output += post.file_url.ToString() + "\n";

                    if (i % 5 == 0)
                    {
                        _log.Debug("Replying...");
                        await ReplyAsync(output, messageReference: new Discord.MessageReference(Context.Message.Id));
                        output = "";
                    }
                }
                if (output != "")
                {
                    _log.Debug("URLs are left in the remainder of the foreach. Replying...");
                    await ReplyAsync(output, messageReference: new Discord.MessageReference(Context.Message.Id));
                }

            }
            catch
            {
                _log.Warn("An exception was thrown. Replying with \"Not found.\"");
                await ReplyAsync("Not found.");
            }
        }
    }
}

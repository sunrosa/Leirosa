public class NsfwModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

    [Discord.Commands.Command("gelbooru")]
    [Discord.Commands.Summary("Gets a random file from Gelbooru (tags optional).")]
    [Discord.Commands.RequireNsfw(Group = "Private")] // In an NSFW channel OR DM
    [Discord.Commands.RequireContext(Discord.Commands.ContextType.DM, Group = "Private")]
    public async Task GelbooruAsync([Discord.Commands.Remainder]string tags_str = "")
    {
        _log.Debug("\"gelbooru\" was called!");

        _log.Debug("Obtaining default tags...");
        var default_tags = Program.config["default_gelbooru_tags"];

        _log.Debug("Formatting requested tags...");
        tags_str = tags_str.Replace(" ", "+"); // Format tags for API request

        _log.Debug("Creating client for request...");
        var client = new HttpClient(); // Create client for request
        _log.Debug("Making request...");
        var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&json=1&tags=sort:random+{default_tags}+{tags_str}"); // Make request
        _log.Debug("Parsing request...");
        dynamic response_json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request
        _log.Debug("Replying with file...");
        await ReplyAsync(response_json.post[0].file_url.ToString(), messageReference: new Discord.MessageReference(Context.Message.Id)); // Don't touch this sacred bullshit
    }

    [Discord.Commands.Command("mgelbooru")]
    [Discord.Commands.Summary("Gets 5 random files from Gelbooru (tags optional).")]
    [Discord.Commands.RequireNsfw(Group = "Private")] // In an NSFW channel OR DM
    [Discord.Commands.RequireContext(Discord.Commands.ContextType.DM, Group = "Private")]
    public async Task MGelbooruAsync([Discord.Commands.Remainder]string tags_str = "")
    {
        _log.Debug("\"mgelbooru\" was called!");

        _log.Debug("Obtaining default tags...");
        var default_tags = Program.config["default_gelbooru_tags"];

        _log.Debug("Formatting requested tags...");
        tags_str = tags_str.Replace(" ", "+"); // Format tags for API request
        var count = 5; // Discord only embeds 5 images per message. If we wanted to surpass this limit, we would have to have it auto-post into separate consecutive messages.

        _log.Debug("Creating client for request...");
        var client = new HttpClient(); // Create client for request
        _log.Debug("Making request...");
        var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit={count}&json=1&tags=sort:random+{default_tags}+{tags_str}"); // Make request
        _log.Debug("Parsing request...");
        dynamic response_json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request

        _log.Debug("Formatting output...");
        var output = "";
        foreach (var post in response_json.post)
        {
            output += post.file_url.ToString() + "\n";
        }

        _log.Debug("Replying with files...");
        await ReplyAsync(output, messageReference: new Discord.MessageReference(Context.Message.Id));
    }
}

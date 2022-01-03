public class NsfwModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    private static string default_tags = "-scat+-loli+-shota";

    [Discord.Commands.Command("gelbooru")]
    [Discord.Commands.Summary("Gets a random file from Gelbooru (supports tags).")]
    [Discord.Commands.RequireNsfw]
    public async Task GelbooruAsync([Discord.Commands.Remainder]string tags_str = "")
    {
        tags_str = tags_str.Replace(" ", "+"); // Format tags for API request

        var client = new HttpClient(); // Create client for request
        var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&json=1&tags=sort:random+{default_tags}+{tags_str}"); // Make request
        dynamic response_json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request
        await ReplyAsync(response_json.post.file_url.ToString(), messageReference: new Discord.MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id)); // Don't touch this sacred bullshit
    }

    [Discord.Commands.Command("mgelbooru")]
    [Discord.Commands.Summary("Gets 5 random files from Gelbooru (supports tags).")]
    [Discord.Commands.RequireNsfw]
    public async Task MGelbooruAsync([Discord.Commands.Remainder]string tags_str = "")
    {
        tags_str = tags_str.Replace(" ", "+"); // Format tags for API request
        var count = 5;

        var client = new HttpClient(); // Create client for request
        var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit={count}&json=1&tags=sort:random+{default_tags}+{tags_str}"); // Make request
        dynamic response_json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request

        var output = "";
        foreach (var post in response_json.post)
        {
            output += post.file_url.ToString() + "\n";
        }

        await ReplyAsync(output, messageReference: new Discord.MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id));
    }
}

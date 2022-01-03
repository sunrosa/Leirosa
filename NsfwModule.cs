public class NsfwModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("gelbooru")]
    [Discord.Commands.RequireNsfw]
    public async Task GelbooruAsync([Discord.Commands.Remainder]string tags_str = "")
    {
        tags_str = tags_str.Replace(" ", "+");

        var client = new HttpClient(); // Create client for request
        var response = await client.GetAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1&json=1&tags=sort:random+{tags_str}"); /// Make request
        dynamic response_json = Newtonsoft.Json.JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()); // Parse request
        await ReplyAsync(response_json.post.file_url.ToString()); // Don't touch this sacred bullshit
    }
}

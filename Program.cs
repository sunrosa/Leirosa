public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private Discord.WebSocket.DiscordSocketClient _client;

    public async Task MainAsync()
    {
        _client = new Discord.WebSocket.DiscordSocketClient();
        _client.Log += Log;

        var context = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("context.json"));

        var token = context["token"];

        await _client.LoginAsync(Discord.TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += Ready; // Call Ready() when the client is ready.

        await Task.Delay(-1); // Block the thread to prevent the program from closing (infinite wait)
    }

    private async Task Ready()
    {
        _client.MessageReceived += this.MessageReceived;
    }

    private async Task MessageReceived(Discord.WebSocket.SocketMessage msg)
    {
        if (msg.Author.IsBot || !msg.Content.StartsWith(".")) return;
        await msg.Channel.SendMessageAsync(msg.Content);
    }

    private Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

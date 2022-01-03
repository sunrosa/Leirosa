public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private Discord.WebSocket.DiscordSocketClient _client;
    private Discord.Commands.CommandService _commands;

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
        _commands = new Discord.Commands.CommandService();
        var command_handler = new CommandHandler(_client, _commands);
        await command_handler.InstallCommandsAsync();
    }

    private Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public static Discord.WebSocket.DiscordSocketClient client;
    public static Discord.Commands.CommandService commands;
    public static Dictionary<string, string> config;

    public async Task MainAsync()
    {
        client = new Discord.WebSocket.DiscordSocketClient();
        client.Log += Log;

        config = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("context.json"));

        var token = config["token"];

        await client.LoginAsync(Discord.TokenType.Bot, token);
        await client.StartAsync();

        client.Ready += Ready; // Call Ready() when the client is ready.

        await Task.Delay(-1); // Block the thread to prevent the program from closing (infinite wait)
    }

    private async Task Ready()
    {
        commands = new Discord.Commands.CommandService();
        var command_handler = new CommandHandler(client, commands);
        await command_handler.InstallCommandsAsync();
    }

    private Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

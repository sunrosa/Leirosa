using Discord.Net;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private Discord.WebSocket.DiscordSocketClient _client;

    public async Task MainAsync()
    {
        _client = new Discord.WebSocket.DiscordSocketClient();
        _client.Log += Log;

        var token = "PLACEHOLDER"; // MOVE THIS ASAP

        await _client.LoginAsync(Discord.TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(Discord.LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

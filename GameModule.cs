public class GameModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
{
    [Discord.Commands.Command("roll")]
    public async Task DiceAsync(int sides = 6)
    {
        var random = new Random();
        await ReplyAsync($"{random.Next(1, sides+1)} out of {sides}!");
    }

    [Discord.Commands.Command("draw")]
    public async Task CardAsync(int size = 1)
    {
        var random = new Random();
        var card_names = new string[]{"Two of Clubs", "Three of Clubs", "Four of Clubs", "Five of Clubs", "Six of Clubs", "Seven of Clubs", "Eight of Clubs", "Nine of Clubs", "Ten of Clubs", "Jack of Clubs", "Queen of Clubs", "King of Clubs", "Ace of Clubs", "Two of Spades", "Three of Spades", "Four of Spades", "Five of Spades", "Six of Spades", "Seven of Spades", "Eight of Spades", "Nine of Spades", "Ten of Spades", "Jack of Spades", "Queen of Spades", "King of Spades", "Ace of Spades", "Two of Hearts", "Three of Hearts", "Four of Hearts", "Five of Hearts", "Six of Hearts", "Seven of Hearts", "Eight of Hearts", "Nine of Hearts", "Ten of Hearts", "Jack of Hearts", "Queen of Hearts", "King of Hearts", "Ace of Hearts", "Two of Diamonds", "Three of Diamonds", "Four of Diamonds", "Five of Diamonds", "Six of Diamonds", "Seven of Diamonds", "Eight of Diamonds", "Nine of Diamonds", "Ten of Diamonds", "Jack of Diamonds", "Queen of Diamonds", "King of Diamonds", "Ace of Diamonds"};
        card_names = card_names.OrderBy(x => random.Next()).ToArray(); // Shuffle

        if (size == 1)
        {
            var card = card_names[0];
            await ReplyAsync($"You draw a {card}!");
            return;
        }

        var selected_cards = new List<string>(card_names[0..size]);

        // "Pop" last card
        var last_card = selected_cards.Last();
        selected_cards.RemoveAt(selected_cards.Count - 1);

        var output = selected_cards.Aggregate((a, b) => a + ", " + b);
        output += ", and " + last_card;

        await ReplyAsync($"You draw a {output}!");
    }
}

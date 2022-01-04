namespace Leirosa
{
    public class GameModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        [Discord.Commands.Command("flip")]
        [Discord.Commands.Summary("Flips a coin.")]
        public async Task FlipAsync()
        {
            _log.Debug("\"flip\" was called!");

            var random = new Random();
            var result = "";
            if (random.Next(2) == 1) result = "Heads";
            else result = "Tails";
            _log.Debug($"Result is {result}.");
            await ReplyAsync($"{result}!");
        }

        [Discord.Commands.Command("roll")]
        [Discord.Commands.Summary("[sides (optional)] Rolls dice.")]
        public async Task RollAsync(int sides = 6)
        {
            _log.Debug("\"roll\" was called!");

            var random = new Random();
            var roll = random.Next(1, sides+1);
            _log.Debug($"Result is \"{roll} out of {sides}!\"");
            await ReplyAsync($"{roll} out of {sides}!");
        }

        [Discord.Commands.Command("draw")]
        [Discord.Commands.Summary("[count (optional)] Draws cards.")]
        public async Task DrawAsync(int count = 1)
        {
            _log.Debug("\"draw\" was called!");

            _log.Debug("Initializing random service and card name array...");
            var random = new Random();
            var card_names = new string[]{"Two of Clubs", "Three of Clubs", "Four of Clubs", "Five of Clubs", "Six of Clubs", "Seven of Clubs", "Eight of Clubs", "Nine of Clubs", "Ten of Clubs", "Jack of Clubs", "Queen of Clubs", "King of Clubs", "Ace of Clubs", "Two of Spades", "Three of Spades", "Four of Spades", "Five of Spades", "Six of Spades", "Seven of Spades", "Eight of Spades", "Nine of Spades", "Ten of Spades", "Jack of Spades", "Queen of Spades", "King of Spades", "Ace of Spades", "Two of Hearts", "Three of Hearts", "Four of Hearts", "Five of Hearts", "Six of Hearts", "Seven of Hearts", "Eight of Hearts", "Nine of Hearts", "Ten of Hearts", "Jack of Hearts", "Queen of Hearts", "King of Hearts", "Ace of Hearts", "Two of Diamonds", "Three of Diamonds", "Four of Diamonds", "Five of Diamonds", "Six of Diamonds", "Seven of Diamonds", "Eight of Diamonds", "Nine of Diamonds", "Ten of Diamonds", "Jack of Diamonds", "Queen of Diamonds", "King of Diamonds", "Ace of Diamonds"};

            _log.Debug("Shuffling cards...");
            card_names = card_names.OrderBy(x => random.Next()).ToArray(); // Shuffle

            if (count == 1)
            {
                _log.Debug("Caller requested only one card. Replying with first of shuffled card array...");
                var card = card_names[0];
                await ReplyAsync($"You draw a {card}!");
                return;
            }

            _log.Debug("Caller requested more than one card. Creating array slice from 0 to selection size of card array...");
            var selected_cards = new List<string>(card_names[0..count]);

            // "Pop" last card
            _log.Debug("Popping last card for grammatical \"and\"...");
            var last_card = selected_cards.Last();
            selected_cards.RemoveAt(selected_cards.Count - 1);

            _log.Debug("Arranging cards grammatically...");
            var output = selected_cards.Aggregate((a, b) => a + ", " + b);
            output += ", and " + last_card;

            _log.Debug("Replying...");
            await ReplyAsync($"You draw a {output}!");
        }
    }
}

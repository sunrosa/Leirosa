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

        [Discord.Commands.Command("vrclogin")]
        [Discord.Commands.Summary("[activity (remainder) (optional)] Login to the bot's VRChat user database.")]
        public async Task VRCLoginAsync([Discord.Commands.Remainder]string activity = "(Unspecified)")
        {
            _log.Debug("\"vrclogin\" was called!");

            var time = System.DateTime.Now;

            var data = new Dictionary<ulong, string>();

            try
            {
                _log.Debug("Reading VRChat users json to local data variable...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText(Program.config["vrchat_path"]));
            }
            catch
            {
                _log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            _log.Debug("Writing activity to local data variable...");
            data[Context.User.Id] = $"{activity} ({time})";

            if (Program.config.ContainsKey("vrchat_role_id"))
            {
                _log.Debug("Giving role...");
                await (Context.User as Discord.WebSocket.SocketGuildUser).AddRoleAsync(Convert.ToUInt64(Program.config["vrchat_role_id"]));
            }

            _log.Debug("Writing local data variable to file...");
            File.WriteAllText(Program.config["vrchat_path"], Newtonsoft.Json.JsonConvert.SerializeObject(data));

            _log.Debug("Replying...");
            await ReplyAsync($"Logged in at {time}.");
        }

        [Discord.Commands.Command("vrcstatus")]
        [Discord.Commands.Summary("See who's online in VRChat.")]
        public async Task VRCStatusAsync()
        {
            _log.Debug("\"vrcstatus\" was called!");

            var data = new Dictionary<ulong, string>();

            try
            {
                _log.Debug("Reading VRChat users json to local data variable...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText(Program.config["vrchat_path"]));
            }
            catch
            {
                _log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (data.Count == 0)
            {
                _log.Debug("Nobody is online. Replying and returning...");
                await ReplyAsync("Nobody is online.");
                return;
            }

            var output = "```\n";
            foreach (var pair in data)
            {
                var user = Context.Client.GetUser(pair.Key);
                output += $"{user.Username}#{user.Discriminator}: {pair.Value}\n";
            }
            output += "```";

            _log.Debug("Replying...");
            await ReplyAsync(output);
        }

        [Discord.Commands.Command("vrclogout")]
        [Discord.Commands.Summary("Log out of the bot's VRChat database.")]
        public async Task VRCLogoutAsync()
        {
            _log.Debug("\"vrclogin\" was called!");

            var data = new Dictionary<ulong, string>();

            try
            {
                _log.Debug("Reading VRChat users json to local data variable...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText(Program.config["vrchat_path"]));
            }
            catch
            {
                _log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (!data.ContainsKey(Context.User.Id))
            {
                _log.Debug("User was not logged in. Replying and returning...");
                await ReplyAsync("You were not logged in.");
                return;
            }

            _log.Debug("Removing user from local data variable...");
            data.Remove(Context.User.Id);

            if (Program.config.ContainsKey("vrchat_role_id"))
            {
                _log.Debug("Removing role...");
                await (Context.User as Discord.WebSocket.SocketGuildUser).RemoveRoleAsync(Convert.ToUInt64(Program.config["vrchat_role_id"]));
            }

            _log.Debug("Writing local data variable to file...");
            File.WriteAllText(Program.config["vrchat_path"], Newtonsoft.Json.JsonConvert.SerializeObject(data));

            _log.Debug("Replying...");
            await ReplyAsync($"Logged out at {System.DateTime.Now}.");
        }


        [Discord.Commands.Command("echo")]
        [Discord.Commands.Summary("[text (remainder)] Echoes what you say.")]
        public async Task EchoAsync([Discord.Commands.Remainder]string text)
        {
            _log.Debug("\"echo\" was called!");

            _log.Debug("Deleting caller...");
            await Context.Message.DeleteAsync();

            _log.Debug("Replying...");
            await ReplyAsync(text);
        }
    }
}

// Cum

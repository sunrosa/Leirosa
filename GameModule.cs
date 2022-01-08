namespace Leirosa
{
    public class GameModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{Math.Floor(timeSpan.TotalHours)}:{timeSpan.ToString(@"mm\:ss")}";
        }

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

        // These VRC commands, and any other long-term stateful (remembers shit between calls) commands, should be ported over to using a database, as opposed to JSON.

        [Discord.Commands.Command("vrclogin")]
        [Discord.Commands.Summary("[activity (remainder) (optional)] Login to the bot's VRChat user database.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task VRCLoginAsync([Discord.Commands.Remainder]string activity = "(Unspecified)")
        {
            _log.Debug("\"vrclogin\" was called!");

            _log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            _log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, VRChatSession>();

            try
            {
                _log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, VRChatSession>>(File.ReadAllText(Program.Config["vrchat_path"]));
            }
            catch
            {
                _log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (data.ContainsKey(Context.User.Id))
            {
                _log.Debug("Session already exists. Writing Activity and UpdateTime. Keeping StartTime...");
                var session = data[Context.User.Id];
                session.Activity = activity;
                session.UpdateTime = time;
                data[Context.User.Id] = session;

                _log.Debug("Replying...");
                await ReplyAsync($"Updating session...");
            }
            else
            {
                _log.Debug("Writing new session data to local dictionary...");
                data[Context.User.Id] = new VRChatSession(){Activity = activity, StartTime = time};

                _log.Debug("Replying...");
                await ReplyAsync($"Logging in at {time}...");
            }

            if (Program.Config.ContainsKey("vrchat_role_id"))
            {
                _log.Debug("Giving role...");
                await (Context.User as Discord.WebSocket.SocketGuildUser).AddRoleAsync(Convert.ToUInt64(Program.Config["vrchat_role_id"]));
            }

            _log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config["vrchat_path"], Newtonsoft.Json.JsonConvert.SerializeObject(data));
        }

        [Discord.Commands.Command("vrcstatus")]
        [Discord.Commands.Summary("See who's online in VRChat.")]
        public async Task VRCStatusAsync()
        {
            _log.Debug("\"vrcstatus\" was called!");

            _log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, VRChatSession>();

            try
            {
                _log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, VRChatSession>>(File.ReadAllText(Program.Config["vrchat_path"]));
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

            _log.Debug("Sorting sessions by start time...");
            var data_sorted = data.OrderBy(x => x.Value.StartTime);

            var output = "```\n";
            foreach (var pair in data_sorted)
            {
                var user = Context.Client.GetUser(pair.Key);
                var elapsed = DateTime.Now - pair.Value.StartTime;
                var elapsed_update = DateTime.Now - pair.Value.UpdateTime;
                var last_updated = "";
                if (pair.Value.UpdateTime != new DateTime())
                {
                    _log.Debug("Session has been updated at least once. Adding last updated detail.");
                    last_updated = $" (updated {FormatTimeSpan(elapsed_update)} ago)";
                }
                output += $"[{FormatTimeSpan(elapsed)}] {user.Username}#{user.Discriminator}: {pair.Value.Activity}{last_updated}\n";
            }
            output += "```";

            _log.Debug("Replying...");
            await ReplyAsync(output);
        }

        [Discord.Commands.Command("vrclogout")]
        [Discord.Commands.Summary("Log out of the bot's VRChat database.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task VRCLogoutAsync()
        {
            _log.Debug("\"vrclogin\" was called!");

            _log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, VRChatSession>();

            try
            {
                _log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, VRChatSession>>(File.ReadAllText(Program.Config["vrchat_path"]));
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

            var time_elapsed = DateTime.Now - data[Context.User.Id].StartTime;

            _log.Debug("Removing user from local dictionary...");
            data.Remove(Context.User.Id);

            if (Program.Config.ContainsKey("vrchat_role_id"))
            {
                _log.Debug("Removing role...");
                await (Context.User as Discord.WebSocket.SocketGuildUser).RemoveRoleAsync(Convert.ToUInt64(Program.Config["vrchat_role_id"]));
            }

            _log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config["vrchat_path"], Newtonsoft.Json.JsonConvert.SerializeObject(data));

            _log.Debug("Replying...");
            await ReplyAsync($"Session lasted {FormatTimeSpan(time_elapsed)}.");
        }


        [Discord.Commands.Command("echo")]
        [Discord.Commands.Summary("[text (remainder)] Echoes what you say.")]
        public async Task EchoAsync([Discord.Commands.Remainder]string text)
        {
            _log.Debug("\"echo\" was called!");

            if (!Context.IsPrivate)
            {
                _log.Debug("Deleting caller...");
                await Context.Message.DeleteAsync();
            }
            else
            {
                _log.Debug("Unable to delete caller due to private context.");
            }

            _log.Debug("Replying...");
            await ReplyAsync(text);
        }
    }
}

// Cum

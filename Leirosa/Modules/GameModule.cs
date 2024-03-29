namespace Leirosa.Modules
{
    public class GameModule : Discord.Commands.ModuleBase<Discord.Commands.SocketCommandContext>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        [Discord.Commands.Command("flip")]
        [Discord.Commands.Summary("Flips a coin.")]
        public async Task FlipAsync([Discord.Commands.Summary("Number of coins to flip (limit of 20)")]int count = 1)
        {
            log.Debug("\"flip\" was called!");

            if (count > 100)
            {
                await ReplyAsync("Please enter a number equal to or lower than 100 for the count parameter.");
                return;
            }

            var output = "";

            if (count == 1)
            {
                var random = new Random();
                var result = "";
                if (random.Next(2) == 1) result = "Heads";
                else result = "Tails";
                output += $"{result}!\n";
            }
            else
            {
                var results = new Dictionary<int, int>();
                var random = new Random();
                for (var i = 0; i < count; i++)
                {
                    var result = random.Next(2);
                    results[result] = results.GetValueOrDefault(result, 0) + 1;
                }

                output = $"Heads: {results[0]}\nTails: {results[1]}\n";
            }

            await ReplyAsync(output);
        }

        [Discord.Commands.Command("roll")]
        [Discord.Commands.Summary("Rolls dice.")]
        public async Task RollAsync([Discord.Commands.Summary("Number of dice to roll (limit of 20)")]int count = 1, [Discord.Commands.Summary("Number of sides that the dice to be rolled have")]int sides = 6)
        {
            log.Debug("\"roll\" was called!");

            if (count > 100)
            {
                await ReplyAsync("Please enter a number equal to or lower than 100 for the count parameter.");
                return;
            }

            var random = new Random();
            var total = 0;
            for (var i = 0; i < count; i++)
            {
                var roll = random.Next(1, sides+1);
                total += roll;
            }
            await ReplyAsync($"{total} ({count}d{sides})");
        }

        [Discord.Commands.Command("draw")]
        [Discord.Commands.Summary("Draws cards.")]
        public async Task DrawAsync([Discord.Commands.Summary("Number of cards to draw")]int count = 1)
        {
            log.Debug("\"draw\" was called!");

            log.Debug("Initializing random service and card name array...");
            var random = new Random();
            var cardNames = new string[]{"Two of Clubs", "Three of Clubs", "Four of Clubs", "Five of Clubs", "Six of Clubs", "Seven of Clubs", "Eight of Clubs", "Nine of Clubs", "Ten of Clubs", "Jack of Clubs", "Queen of Clubs", "King of Clubs", "Ace of Clubs", "Two of Spades", "Three of Spades", "Four of Spades", "Five of Spades", "Six of Spades", "Seven of Spades", "Eight of Spades", "Nine of Spades", "Ten of Spades", "Jack of Spades", "Queen of Spades", "King of Spades", "Ace of Spades", "Two of Hearts", "Three of Hearts", "Four of Hearts", "Five of Hearts", "Six of Hearts", "Seven of Hearts", "Eight of Hearts", "Nine of Hearts", "Ten of Hearts", "Jack of Hearts", "Queen of Hearts", "King of Hearts", "Ace of Hearts", "Two of Diamonds", "Three of Diamonds", "Four of Diamonds", "Five of Diamonds", "Six of Diamonds", "Seven of Diamonds", "Eight of Diamonds", "Nine of Diamonds", "Ten of Diamonds", "Jack of Diamonds", "Queen of Diamonds", "King of Diamonds", "Ace of Diamonds"};

            log.Debug("Shuffling cards...");
            cardNames = cardNames.OrderBy(x => random.Next()).ToArray(); // Shuffle

            if (count == 1)
            {
                log.Debug("Caller requested only one card. Replying with first of shuffled card array...");
                var card = cardNames[0];
                await ReplyAsync($"You draw a {card}!");
                return;
            }

            log.Debug("Caller requested more than one card. Creating array slice from 0 to selection size of card array...");
            var selectedCards = new List<string>(cardNames[0..count]);

            // "Pop" last card
            log.Debug("Popping last card for grammatical \"and\"...");
            var lastCard = selectedCards.Last();
            selectedCards.RemoveAt(selectedCards.Count - 1);

            log.Debug("Arranging cards grammatically...");
            var output = selectedCards.Aggregate((a, b) => a + ", " + b);
            output += ", and " + lastCard;

            log.Debug("Replying...");
            await ReplyAsync($"You draw a {output}!");
        }

        // These VRC commands, and any other long-term stateful (remembers shit between calls) commands, should be ported over to using a database, as opposed to JSON.

        [Discord.Commands.Command("vrclogin")]
        [Discord.Commands.Alias("vrcl")]
        [Discord.Commands.Summary("Login to the bot's VRChat user database.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task VRCLoginAsync([Discord.Commands.Summary("User status")][Discord.Commands.Remainder]string? activity = null)
        {
            log.Debug("\"vrclogin\" was called!");

            log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (data.ContainsKey(Context.User.Id))
            {
                log.Debug("Session already exists. Writing Activity and UpdateTime. Keeping StartTime...");
                var session = data[Context.User.Id];
                session.IsUpdated = true;
                session.Activity = activity;
                session.UpdateTime = time;
                data[Context.User.Id] = session;

                log.Debug("Replying...");
                await ReplyAsync($"Updating session...");
            }
            else
            {
                log.Debug("Writing new session data to local dictionary...");
                data[Context.User.Id] = new Data.VRChatSession(){Activity = activity, StartTime = time};

                log.Debug("Replying...");
                await ReplyAsync($"Logging in at {time.ToString(Program.Config.DatetimeFormat)}...");
            }

            if (Program.Config.VRChatRoleId != 0 && Program.Config.VRChatRoleId != 0)
            {
                try
                {
                    log.Debug("Giving role...");
                    await (Context.User as Discord.WebSocket.SocketGuildUser).AddRoleAsync(Program.Config.VRChatRoleId);
                }
                catch
                {
                    log.Error("Could not give VRChat online role.");
                    await ReplyAsync("Could not give VRChat online role. Make sure my bot role is above the role you are trying to apply!");
                }
            }

            log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config.VRChatPath, Newtonsoft.Json.JsonConvert.SerializeObject(data));
        }

        [Discord.Commands.Command("vrcappend")]
        [Discord.Commands.Alias("vrca")]
        [Discord.Commands.Summary("Appends text to your VRChat status.")]
        public async Task VRCAppendAsync([Discord.Commands.Summary("Status text to append")]string append)
        {
            log.Debug("\"vrcappend\" was called!");

            log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (!data.ContainsKey(Context.User.Id))
            {
                log.Debug("User is not logged in. Replying and returning...");
                await ReplyAsync("You are not logged in.");
                return;
            }

            log.Debug("Writing session...");
            var session = data[Context.User.Id];
            session.UpdateTime = time;
            session.Activity += append;
            session.IsUpdated = true;
            data[Context.User.Id] = session;

            log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config.VRChatPath, Newtonsoft.Json.JsonConvert.SerializeObject(data));

            log.Debug("Replying...");
            await ReplyAsync("Appended status.");
        }

        [Discord.Commands.Command("vrcstatus")]
        [Discord.Commands.Alias("vrcs")]
        [Discord.Commands.Summary("See who's online in VRChat.")]
        public async Task VRCStatusAsync()
        {
            log.Debug("\"vrcstatus\" was called!");

            log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (data.Count == 0)
            {
                log.Debug("Nobody is online. Replying and returning...");
                await ReplyAsync("Nobody is online.");
                return;
            }

            log.Debug("Sorting sessions by start time...");
            var dataSorted = data.OrderBy(x => x.Value.StartTime);

            var output = "```\n";
            foreach (var pair in dataSorted)
            {
                var user = Context.Client.GetUser(pair.Key);
                var elapsed = time - pair.Value.StartTime;
                var elapsedUpdate = time - pair.Value.UpdateTime;
                var elapsedPaused = time - pair.Value.PauseTime;
                var pauseEta = pair.Value.UnpauseTime - pair.Value.PauseTime;
                var lastUpdated = "";
                var paused = "";
                if (pair.Value.IsUpdated)
                {
                    log.Debug("Session has been updated at least once. Adding last updated detail...");
                    lastUpdated = $" (updated {ModuleHelpers.FormatTimeSpan(elapsedUpdate)} ago)";
                }
                if (pair.Value.IsPaused && pair.Value.UnpauseTime == new DateTime())
                {
                    log.Debug("Session is paused. Adding paused detail without ETA...");
                    paused = $" (AFK {ModuleHelpers.FormatTimeSpan(elapsedPaused)})";
                }
                else if (pair.Value.IsPaused && pair.Value.UnpauseTime != new DateTime())
                {
                    log.Debug("Session is paused. Adding paused detail wtih ETA...");
                    paused = $" (AFK {ModuleHelpers.FormatTimeSpan(elapsedPaused)} / {ModuleHelpers.FormatTimeSpan(pauseEta)})";
                }
                output += $"[{ModuleHelpers.FormatTimeSpan(elapsed)}] {user.Username}#{user.Discriminator}: {pair.Value.Activity ?? "(Unspecified)"}{lastUpdated}{paused}\n";
            }
            output += "```";

            log.Debug("Replying...");
            await ReplyAsync(output);
        }

        [Discord.Commands.Command("vrclogout")]
        [Discord.Commands.Alias("vrco")]
        [Discord.Commands.Summary("Log out of the bot's VRChat database.")]
        [Discord.Commands.RequireContext(Discord.Commands.ContextType.Guild)]
        public async Task VRCLogoutAsync()
        {
            log.Debug("\"vrclogout\" was called!");

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (!data.ContainsKey(Context.User.Id))
            {
                log.Debug("User is not logged in. Replying and returning...");
                await ReplyAsync("You are not logged in.");
                return;
            }

            var timeElapsed = DateTime.Now - data[Context.User.Id].StartTime;

            log.Debug("Removing user from local dictionary...");
            data.Remove(Context.User.Id);

            if (Program.Config.VRChatRoleId != 0 && Program.Config.VRChatRoleId != 0)
            {
                try
                {
                    log.Debug("Removing role...");
                    await (Context.User as Discord.WebSocket.SocketGuildUser).RemoveRoleAsync(Program.Config.VRChatRoleId);
                }
                catch
                {
                    log.Error("Could not take VRChat online role.");
                    await ReplyAsync("Could not take VRChat online role. Make sure my bot role is above the role you are trying to apply!");
                }
            }

            log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config.VRChatPath, Newtonsoft.Json.JsonConvert.SerializeObject(data));

            log.Debug("Replying...");
            await ReplyAsync($"Session lasted {ModuleHelpers.FormatTimeSpan(timeElapsed)}.");
        }

        [Discord.Commands.Command("vrcpause")]
        [Discord.Commands.Alias("vrcp")]
        [Discord.Commands.Summary("Set yourself as AFK in VRChat. You may specify an ETA for your return.")]
        public async Task VRCPauseAsync([Discord.Commands.Summary("Minutes until your estimated return")][Discord.Commands.Name("ETA")]uint eta = 0)
        {
            log.Debug("\"vrcpause\" was called!");

            log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (!data.ContainsKey(Context.User.Id))
            {
                log.Debug("User is not logged in. Replying and returning...");
                await ReplyAsync("You are not logged in.");
                return;
            }

            var session = data[Context.User.Id];
            session.IsPaused = true;
            session.PauseTime = time;
            if (eta != 0) session.UnpauseTime = time.AddMinutes(eta);
            else session.UnpauseTime = new DateTime();
            data[Context.User.Id] = session;

            log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config.VRChatPath, Newtonsoft.Json.JsonConvert.SerializeObject(data));

            log.Debug("Replying...");
            if (eta == 0) await ReplyAsync("Going AFK...");
            else if (eta == 1) await ReplyAsync($"Going AFK for {eta} minute...");
            else await ReplyAsync($"Going AFK for {eta} minutes...");
        }

        [Discord.Commands.Command("vrcunpause")]
        [Discord.Commands.Alias("vrcu")]
        [Discord.Commands.Summary("Return from AFK in VRChat.")]
        public async Task VRCUnpauseAsync()
        {
            log.Debug("\"vrcunpause\" was called!");

            log.Debug("Getting current time...");
            var time = System.DateTime.Now;

            log.Debug("Creating local Dictionary<ulong, VRChatSession>...");
            var data = new Dictionary<ulong, Data.VRChatSession>();

            try
            {
                log.Debug("Reading VRChat users json to local dictionary...");
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<ulong, Data.VRChatSession>>(File.ReadAllText(Program.Config.VRChatPath));
            }
            catch
            {
                log.Warn("Could not read VRChat users json. Using blank dictionary.");
            }

            if (!data.ContainsKey(Context.User.Id))
            {
                log.Debug("User is not logged in. Replying and returning...");
                await ReplyAsync("You are not logged in.");
                return;
            }

            var session = data[Context.User.Id];
            var elapsedPaused = time - session.PauseTime;
            var pauseEta = session.UnpauseTime - session.PauseTime;
            var hasEta = session.UnpauseTime != new DateTime();
            session.IsPaused = false;
            session.PauseTime = new DateTime();
            session.UnpauseTime = new DateTime();
            data[Context.User.Id] = session;

            log.Debug("Writing local dictionary to file...");
            File.WriteAllText(Program.Config.VRChatPath, Newtonsoft.Json.JsonConvert.SerializeObject(data));

            log.Debug("Replying...");
            if (hasEta) await ReplyAsync($"You were AFK for {ModuleHelpers.FormatTimeSpan(elapsedPaused)} out of {ModuleHelpers.FormatTimeSpan(pauseEta)}.");
            else await ReplyAsync($"You were AFK for {ModuleHelpers.FormatTimeSpan(elapsedPaused)}.");
        }

        [Discord.Commands.Command("echo")]
        [Discord.Commands.Summary("Echoes what you say.")]
        public async Task EchoAsync([Discord.Commands.Summary("Text to be echoed")][Discord.Commands.Remainder]string text)
        {
            log.Debug("\"echo\" was called!");

            if (!Context.IsPrivate)
            {
                log.Debug("Deleting caller...");
                await Context.Message.DeleteAsync();
            }
            else
            {
                log.Debug("Unable to delete caller due to private context.");
            }

            log.Debug("Replying...");
            await ReplyAsync(text);
        }
    }
}

// Cum

using Utils;
using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace Commands
{
    public partial class RngCertified : InteractionModuleBase
    {
        public DataBase.DataBaseContext DataBase { get; set; }

        public enum Choices
        {
            [ChoiceDisplay("Crab Rave")]
            crabRave,

            [ChoiceDisplay("Epic Outro")]
            outro,

            [ChoiceDisplay("Royalistiq")]
            royalistiq,

            [ChoiceDisplay("RNG certified")]
            rngCertified
        }

        class OutroInfo
        {
            public string Path { get; set; }
            public string Message { get; set; }
            public bool IsForPoints { get; set; } = false;
            public Emoji[] Reactions { get; set; } = Array.Empty<Emoji>();
        }

        private static readonly Dictionary<Choices, OutroInfo> OutroPath =
            new()
            {
                {
                    Choices.crabRave,
                    new OutroInfo() { Path = "./public/outro/crabrave.mp3", Message = ":crab:", }
                },
                {
                    Choices.outro,
                    new OutroInfo()
                    {
                        Path = "./public/outro/outro.mp3",
                        Message = "SMASH THAT LIKE BUTTON :thumbsup:",
                        Reactions = new[] { new Emoji("👍"), new Emoji("👎") }
                    }
                },
                {
                    Choices.royalistiq,
                    new OutroInfo()
                    {
                        Path = "./public/outro/royalistiq.mp3",
                        Message = "HOOWWH MY DAYS 😱",
                    }
                },
                {
                    Choices.rngCertified,
                    new OutroInfo()
                    {
                        Path = "./public/outro/royalistiq.mp3",
                        Message = "RNG Certified 🍀",
                        IsForPoints = true,
                    }
                },
            };

        [SlashCommand("outro", "Epic outro", runMode: Discord.Interactions.RunMode.Async)]
        public async Task Outro([Name("opties")] RngCertified.Choices selectedOutro)
        {
            // TODO: rng outro points

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await RespondAsync("Gsat join eerst een kanaal dan.", ephemeral: true);
                return;
            }

            var outro = OutroPath[selectedOutro];
            if (!File.Exists(outro.Path))
            {
                throw new Exception($"Outro file {outro.Path} not found");
            }

            // Send message and add reactions
            new Task(async () =>
            {
                await RespondAsync(outro.Message);

                if (outro.Reactions.Length > 0)
                {
                    var response = await Context.Interaction.GetOriginalResponseAsync();
                    await response.AddReactionsAsync(outro.Reactions);
                }
            }).Start();

            // play outro
            var audioClient = await channel.ConnectAsync();
            await Voice.SendAsync(audioClient, outro.Path);

            // done playing
            var usersInChannel = await Voice.GetUsersInVoiceChannelAsync(channel);

            var tasks = new List<Task>
            {
                channel.DisconnectAsync() // disconnect self
            };

            IGuildUser lastLeft = null;
            object lockObj = new();

            foreach (var user in usersInChannel)
            {
                var task = Task.Run(async () =>
                {
                    await user.ModifyAsync(x => x.Channel = null);
                    lock (lockObj)
                    {
                        lastLeft = user;
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            if (lastLeft != null)
            {
                await this.DataBase.AddOutroScore(lastLeft.Id, this.Context.Guild.Id);
                await Context.Interaction.FollowupAsync(lastLeft.Mention);
            }
        }

        [SlashCommand("outroleaderboard", "@boodschapjes")]
        public async Task Outroleaderboard()
        {
            var outroUsers = DataBase.GetOutroScores(this.Context.Guild.Id);

            if (outroUsers.Length == 0)
            {
                await this.Context.Interaction.RespondAsync(
                    "Leaderboard is nog leeg...",
                    ephemeral: true
                );
                return;
            }

            var discordUsers = await User.GetUsersByIdsAsync(
                this.Context.Client,
                outroUsers.Select(x => x.Id).ToArray()
            );

            // create embed
            var embed = new EmbedBuilder()
            {
                Title = "Vaakst het laatste de call verlaten",
                Color = Color.Gold,
                ThumbnailUrl = discordUsers[outroUsers[0].Id].GetDisplayAvatarUrl()
            };

            for (var i = 0; i < outroUsers.Length; i++)
            {
                var user = outroUsers[i];
                var discordUser = discordUsers[user.Id];

                embed.AddField(
                    new EmbedFieldBuilder()
                    {
                        Name = $"{i + 1}: {discordUser.GlobalName}",
                        Value = $"{user.OutroScore}x"
                    }
                );
            }

            await RespondAsync(embed: embed.Build());
        }
    }
}

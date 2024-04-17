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
            public bool IsForRngPoints { get; set; } = false;
            public Emoji[] Reactions { get; set; } = Array.Empty<Emoji>();
        }

        private static readonly Dictionary<Choices, OutroInfo> OutroInfoLookup =
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
                        IsForRngPoints = true,
                    }
                },
            };

        [SlashCommand("outro", "Epic outro", runMode: Discord.Interactions.RunMode.Async)]
        public async Task Outro([Name("opties")] RngCertified.Choices selectedOutroChoice)
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await RespondAsync("Gsat join eerst een kanaal dan.", ephemeral: true);
                return;
            }

            var outro = OutroInfoLookup[selectedOutroChoice];
            var usersInChannel = await Voice.GetUsersInVoiceChannelAsync(channel);

            if (outro.IsForRngPoints)
            {
                // check if all users needed are in the channel
                var usersNeeded = DataBase.GetAllRngUsers(this.Context.Guild.Id);
                if (
                    !usersNeeded.All(
                        usersNeeded => usersInChannel.Any(user => user.Id == usersNeeded.Id)
                    )
                )
                {
                    await RespondAsync(
                        "Niet iedereen die meedoet zit in call, dus deze outro kan niet.",
                        ephemeral: true
                    );
                    return;
                }
            }

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

            await this.DataBase.AddOutroScore(lastLeft.Id, this.Context.Guild.Id);

            if (!outro.IsForRngPoints)
            {
                await Context.Interaction.FollowupAsync(lastLeft.Mention);
            }
            else
            {
                if (!DataBase.IsRngUser(lastLeft.Id, this.Context.Guild.Id))
                {
                    await Context.Interaction.FollowupAsync(
                        $"{lastLeft.Mention} doet niet mee, niemand krijgt er dus punten bij."
                    );
                    return;
                }

                int rngPoints = GetOutroRngPoints();

                await Context.Interaction.FollowupAsync(
                    embed: CreateOutroRngEmbed(lastLeft, rngPoints)
                );
            }
        }

        private static Embed CreateOutroRngEmbed(IGuildUser lastLeft, int rngPoints)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Outro",
                Color = Utils.General.RandomColor(),
                ThumbnailUrl = lastLeft.GetDisplayAvatarUrl()
            };

            embed.AddField("Winnaar:", lastLeft.DisplayName);
            embed.AddField("Punten:", rngPoints);
            return embed.Build();
        }

        private static int GetOutroRngPoints()
        {
            var random = new BetterRandom();

            bool gotMainPrize = random.RandomElement(
                Enumerable.Repeat(false, 20).Concat(new bool[] { true }).ToArray()
            );

            int rngPoints;
            if (gotMainPrize)
            {
                rngPoints = 1000;
            }
            else
            {
                rngPoints = random.Next(10, 100);

                var positive = random.RandomElement(
                    Enumerable.Repeat(true, 9).Concat(new bool[] { false }).ToArray()
                );
                rngPoints *= positive ? 1 : -1;
            }

            return rngPoints;
        }

        [SlashCommand("outroleaderboard", "@boodschapjes")]
        public async Task Outroleaderboard()
        {
            var dbUsers = DataBase.GetOutroScores(this.Context.Guild.Id);

            if (dbUsers.Length == 0)
            {
                await this.Context.Interaction.RespondAsync(
                    "Leaderboard is nog leeg...",
                    ephemeral: true
                );
                return;
            }

            var discordUsers = await User.GetUsersByIdsAsync(
                this.Context.Guild,
                dbUsers.Select(x => x.Id).ToArray()
            );

            var embed = new EmbedBuilder()
            {
                Title = "Vaakst het laatste de call verlaten",
                Color = Utils.General.RandomColor(),
                ThumbnailUrl = discordUsers[dbUsers[0].Id].GetDisplayAvatarUrl()
            };

            for (var i = 0; i < dbUsers.Length; i++)
            {
                var user = dbUsers[i];
                var discordUser = discordUsers[user.Id];

                embed.AddField(
                    new EmbedFieldBuilder()
                    {
                        Name = $"{i + 1}: {discordUser.DisplayName}",
                        Value = $"{user.OutroScore}x"
                    }
                );
            }

            await RespondAsync(embed: embed.Build());
        }
    }
}

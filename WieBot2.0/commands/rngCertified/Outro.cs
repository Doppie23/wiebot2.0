using Utils;
using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace Commands
{
    public partial class RngCertified : InteractionModuleBase
    {
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

        private static readonly Dictionary<Choices, OutroInfo> OutroPath =
            new()
            {
                {
                    Choices.crabRave,
                    new OutroInfo()
                    {
                        Path = "./public/outro/crabrave.mp3",
                        Message = ":crab:",
                        IsForPoints = false,
                    }
                },
                {
                    Choices.outro,
                    new OutroInfo()
                    {
                        Path = "./public/outro/outro.mp3",
                        Message = "SMASH THAT LIKE BUTTON :thumbsup:",
                        IsForPoints = false,
                        Reactions = new[] { new Emoji("👍"), new Emoji("👎") }
                    }
                },
                {
                    Choices.royalistiq,
                    new OutroInfo()
                    {
                        Path = "./public/outro/royalistiq.mp3",
                        Message = "HOOWWH MY DAYS 😱",
                        IsForPoints = false,
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
            // TODO: score most last left

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
                await Context.Interaction.FollowupAsync(lastLeft.Mention);
        }

        class OutroInfo
        {
            public string Path { get; set; }
            public string Message { get; set; }
            public bool IsForPoints { get; set; }
            public Emoji[] Reactions { get; set; } = Array.Empty<Emoji>();
        }
    }
}

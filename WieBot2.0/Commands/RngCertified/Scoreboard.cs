using Discord;
using Discord.Interactions;
using Utils;

namespace Commands
{
    partial class RngCertified
    {
        [SlashCommand("scorebord-rng-certified", "rng certified")]
        public async Task ScoreboardRngCertified()
        {
            var dbUsers = DataBase.GetAllRngUsers(this.Context.Guild.Id);

            if (dbUsers.Length == 0)
            {
                await RespondAsync(
                    "Er doet niemand mee, dus is er geen scorebord.",
                    ephemeral: true
                );
                return;
            }

            var discordUsers = await User.GetUsersByIdsAsync(
                this.Context.Guild,
                dbUsers.Select(u => u.Id).ToArray()
            );

            var embed = new EmbedBuilder()
            {
                Title = "Beste RNG",
                Color = Rng.RandomColor(),
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
                        Value = $"{user.RngScore} punten"
                    }
                );
            }

            await RespondAsync(embed: embed.Build());
        }
    }
}

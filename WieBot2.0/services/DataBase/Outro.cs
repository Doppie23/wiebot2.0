namespace DataBase;

public partial class DataBaseContext
{
    /// <summary>
    ///     Adds a score to the outro leaderboard
    ///     <br />
    ///     <br />
    ///     If the user doesn't exist, it will be added
    /// </summary>
    public async Task AddOutroScore(ulong userId, ulong guildId, int score = 1)
    {
        var user = await this.GetUser(userId, guildId);
        user.OutroScore += score;
        await this.SaveChangesAsync();
    }

    public User[] GetOutroScores(ulong guildId)
    {
        return [.. (
            from user in this.Users
            where user.GuildId == guildId
            orderby user.OutroScore descending
            select user
        )
            .Cast<User>()];
    }
}

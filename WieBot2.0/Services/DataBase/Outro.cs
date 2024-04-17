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

    /// <summary>
    ///     Get the outro leaderboard for a guild, sorted by score
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    public User[] GetOutroScores(ulong guildId, int limit = 15)
    {
        return (
            from user in this.Users
            where user.GuildId == guildId && user.OutroScore != 0
            orderby user.OutroScore descending
            select user
        )
            .Take(limit)
            .Cast<User>()
            .ToArray();
    }
}

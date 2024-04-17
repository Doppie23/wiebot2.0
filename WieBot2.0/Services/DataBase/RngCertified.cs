namespace DataBase;

public partial class DataBaseContext
{
    /// <summary>
    ///     Adds a score to the rng leaderboard
    ///     <br />
    ///     Score cannot be negative
    /// </summary>
    public async Task AddRngScore(ulong userId, ulong guildId, int score)
    {
        var user = await this.GetUser(userId, guildId);
        if (user.RngScore == null)
            return;

        user.RngScore += score;

        if (user.RngScore < 0)
            user.RngScore = 0;

        await this.SaveChangesAsync();
    }

    /// <summary>
    ///     Get the outro leaderboard for a guild, sorted by score
    /// </summary>
    public User[] GetRngScores(ulong guildId, int limit = 15)
    {
        return this.GetAllRngUsers(guildId).Take(limit).ToArray();
    }

    public User[] GetAllRngUsers(ulong guildId)
    {
        return (
            from user in this.Users
            where user.GuildId == guildId && user.RngScore != null
            orderby user.RngScore descending
            select user
        )
            .Cast<User>()
            .ToArray();
    }

    public bool IsRngUser(ulong userId, ulong guildId)
    {
        return this.GetAllRngUsers(guildId).Any(x => x.Id == userId);
    }
}

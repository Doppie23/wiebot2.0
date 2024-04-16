namespace DataBase;

public partial class DataBaseContext
{
    public void AddOutroScore(ulong userId, ulong guildId, int score = 1)
    {
        var user = this.GetUser(userId, guildId);
        user.OutroScore += score;
        this.SaveChanges();
    }

    public User[] GetOutroScores(ulong guildId)
    {
        return this.Users
            .Where(u => u.GuildId == guildId)
            .OrderByDescending(x => x.OutroScore)
            .Cast<User>()
            .ToArray();
    }
}

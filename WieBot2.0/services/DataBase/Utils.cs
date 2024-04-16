namespace DataBase;

public partial class DataBaseContext
{
    private bool UserExists(ulong userId, ulong guildId)
    {
        return this.Users.Where(u => u.Id == userId && u.GuildId == guildId).Any();
    }

    private User AddUser(ulong userId, ulong guildId)
    {
        var user = new User { Id = userId, GuildId = guildId };
        this.Users.Add(user);
        return user;
    }

    private User GetUser(ulong userId, ulong guildId)
    {
        if (!this.UserExists(userId, guildId))
            return this.AddUser(userId, guildId);
        else
            return this.Users.Where(u => userId == u.Id && guildId == u.GuildId).First();
    }
}

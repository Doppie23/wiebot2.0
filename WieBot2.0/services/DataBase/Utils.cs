namespace DataBase;

public partial class DataBaseContext
{
    private bool UserExists(ulong userId, ulong guildId)
    {
        return this.Users.Where(u => u.Id == userId && u.GuildId == guildId).Any();
    }

    private async Task<User> AddUser(ulong userId, ulong guildId)
    {
        var user = new User { Id = userId, GuildId = guildId };
        this.Users.Add(user);
        await this.SaveChangesAsync();
        return user;
    }

    /// <summary>
    ///     Get user from database or adds it if it doesn't exist
    /// </summary>
    private Task<User> GetUser(ulong userId, ulong guildId)
    {
        if (!this.UserExists(userId, guildId))
            return this.AddUser(userId, guildId);
        else
            return Task.FromResult(
                (
                    from user in this.Users
                    where user.Id == userId && user.GuildId == guildId
                    select user
                ).First()
            );
    }
}

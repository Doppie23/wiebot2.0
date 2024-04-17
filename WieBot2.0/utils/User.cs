using Discord;

namespace Utils;

static class User
{
    /// <summary>
    /// Get a dictionary of users by their IDs
    /// </summary>
    /// <param name="client"></param>
    /// <param name="userIds"></param>
    /// <returns>A dictionary of users with as key the ID</returns>
    public static async Task<Dictionary<ulong, IGuildUser>> GetUsersByIdsAsync(
        IGuild guild,
        ulong[] userIds
    )
    {
        List<Task<(ulong, IGuildUser)>> tasks = new();

        foreach (var userId in userIds)
        {
            async Task<(ulong, IGuildUser)> task()
            {
                var discordUser = await guild.GetUserAsync(userId);
                return (userId, discordUser);
            }

            tasks.Add(task());
        }

        var idAndUsers = await Task.WhenAll(tasks);

        Dictionary<ulong, IGuildUser> dict = new();

        foreach (var (id, user) in idAndUsers)
        {
            if (!dict.ContainsKey(id))
                dict[id] = user;
        }

        return dict;
    }
}

using Discord;

namespace Utils
{
    static class User
    {
        /// <summary>
        /// Get a dictionary of users by their IDs
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userIds"></param>
        /// <returns>A dictionary of users with as key the ID</returns>
        public static async Task<Dictionary<ulong, IUser>> GetUsersByIdsAsync(
            IDiscordClient client,
            ulong[] userIds
        )
        {
            List<Task<(ulong, IUser)>> tasks = new();

            foreach (var userId in userIds)
            {
                async Task<(ulong, IUser)> task()
                {
                    var discordUser = await client.GetUserAsync(userId);
                    return (userId, discordUser);
                }

                tasks.Add(task());
            }

            var idAndUsers = await Task.WhenAll(tasks);

            Dictionary<ulong, IUser> dict = new();

            foreach (var (id, user) in idAndUsers)
            {
                if (!dict.ContainsKey(id))
                    dict[id] = user;
            }

            return dict;
        }
    }
}

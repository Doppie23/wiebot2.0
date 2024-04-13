using Discord.Interactions;
using Discord.WebSocket;

static partial class Commands
{
    [SlashCommand("ping", "pong")]
    public static async Task Ping(SocketSlashCommand command)
    {
        await command.RespondAsync("pong");
    }
}

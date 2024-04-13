using Discord.Interactions;
using Discord.WebSocket;

static partial class Commands
{
    [SlashCommand("test", "Test command")]
    public static async Task TestCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Test");
    }
}

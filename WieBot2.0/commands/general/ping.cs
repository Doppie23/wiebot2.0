using Discord.Interactions;

namespace Commands
{
    public partial class General : InteractionModuleBase
    {
        [SlashCommand("ping", "pong")]
        public async Task Ping()
        {
            await this.RespondAsync("pong");
        }
    }
}

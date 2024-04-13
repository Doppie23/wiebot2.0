using Discord.Commands;
using Discord.Interactions;

namespace Commands
{
    public partial class RngCertified : InteractionModuleBase
    {
        public enum Choices
        {
            test,
            test2
        }

        [SlashCommand("outro", "Epic outro", runMode: Discord.Interactions.RunMode.Async)]
        public async Task Outro([Name("opties")] RngCertified.Choices selectedOutro) =>
            await RespondAsync($"Your choice: {selectedOutro}");
    }
}

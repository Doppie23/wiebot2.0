using Discord.Interactions;

namespace Commands
{
    public partial class General : InteractionModuleBase
    {
        public DataBase Test { get; set; }

        [SlashCommand("ping", "pong")]
        public async Task Ping()
        {
            Console.WriteLine(this.Test.TestString);
            await this.RespondAsync("pong");
        }
    }
}

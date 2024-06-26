using Newtonsoft.Json;
using DataBase;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

class WieBot
{
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly InteractionService interactionService;
    private readonly DataBaseContext dataBase;

    private readonly Config config;

    public WieBot()
    {
        // TODO: check if config has all required values
        // TODO: add isDev to config
        this.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));

        this.dataBase = new DataBaseContext();

        this.serviceProvider = this.CreateProvider();

        this.client = new DiscordSocketClient(
            new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All }
        );

        this.interactionService = new InteractionService(
            client.Rest,
            new InteractionServiceConfig() { LogLevel = LogSeverity.Verbose }
        );

        this.interactionService.Log += (LogMessage message) =>
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        };

        this.client.Log += (LogMessage message) =>
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        };

        this.client.Ready += async () =>
        {
            await this.interactionService.RegisterCommandsToGuildAsync(this.config.GuildId);
        };

        this.client.InteractionCreated += async (SocketInteraction interaction) =>
        {
            try
            {
                var context = new SocketInteractionContext(this.client, interaction);

                var result = await this.interactionService.ExecuteCommandAsync(
                    context,
                    this.serviceProvider
                );

                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:

                            throw new Exception("UnmetPrecondition in SlashCommand");
                        default:
                            break;
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] {e.Message}");
                if (interaction.Type is InteractionType.ApplicationCommand)
                {
                    await interaction.DeleteOriginalResponseAsync();
                    await interaction.RespondAsync(
                        "Sorry man, werkt ff niet ofzo weet ik veel",
                        ephemeral: true
                    );
                }
            }
        };

        this.client.MessageReceived += async (SocketMessage message) =>
        {
            if (message.Author.Id == this.client.CurrentUser.Id)
                return;

            if (message.Content.ToLower().StartsWith("wie"))
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} wie vroeg?");
            }
        };
    }

    private IServiceProvider CreateProvider()
    {
        var collection = new ServiceCollection().AddSingleton(this.dataBase);

        return collection.BuildServiceProvider();
    }

    public async Task Start()
    {
        await this.interactionService.AddModulesAsync(
            Assembly.GetEntryAssembly(),
            this.serviceProvider
        );

        await this.client.LoginAsync(TokenType.Bot, this.config.Token);
        await this.client.StartAsync();

        await Task.Delay(-1);
    }
}

class Config
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("clientId")]
    public string ClientId { get; set; }

    [JsonProperty("guildId")]
    public ulong GuildId { get; set; }
}

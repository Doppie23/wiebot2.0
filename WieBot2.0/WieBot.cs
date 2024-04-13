using System.Reflection;
using Newtonsoft.Json;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Interactions;

class WieBot
{
    private readonly DiscordSocketClient client;
    private readonly Config config;
    private readonly Dictionary<string, MethodInfo> commands = new();

    public WieBot()
    {
        this.client = new DiscordSocketClient();

        this.config = JsonConvert.DeserializeObject<Config>(
            File.ReadAllText("../../../../config.json")
        );

        this.client.Log += Log;
        this.client.Ready += this.ClientReady;
        this.client.SlashCommandExecuted += this.SlashCommandHandler;
    }

    public async Task Start()
    {
        await this.client.LoginAsync(TokenType.Bot, this.config.Token);
        await this.client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task ClientReady()
    {
        var guild = client.GetGuild(this.config.GuildId);

        var guildCommand = new SlashCommandBuilder();

        // get all command handlers from the `Commands` class
        Type type = typeof(Commands);

        MethodInfo[] methods = type.GetMethods();

        foreach (MethodInfo method in methods)
        {
            if (method.ReturnType == typeof(System.Threading.Tasks.Task))
            {
                var attributes = method.GetCustomAttributes(true);

                foreach (var attribute in attributes)
                {
                    if (attribute.GetType() == typeof(SlashCommandAttribute))
                    {
                        var slashCommandAttribute = (SlashCommandAttribute)attribute;

                        guildCommand.WithName(slashCommandAttribute.Name);
                        guildCommand.WithDescription(slashCommandAttribute.Description);

                        this.commands.Add(slashCommandAttribute.Name, method);

                        Console.WriteLine($"Registered Command {slashCommandAttribute.Name}");
                    }
                }
            }
        }

        try
        {
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            var method = this.commands[command.Data.Name];

            if (
                method.GetParameters().Length != 1
                || method.GetParameters()[0].ParameterType != typeof(SocketSlashCommand)
            )
            {
                throw new Exception(
                    "method does not accept correct arguments, it should take a SocketSlashCommand"
                );
            }

            var resultTask = (Task)method.Invoke(null, new object[] { command });
            await resultTask;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await command.RespondAsync("Dat commando ken ik niet...", null, false, true);
        }
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

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
        this.client = new DiscordSocketClient(
            new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All }
        );

        this.config = JsonConvert.DeserializeObject<Config>(
            File.ReadAllText("../../../../config.json")
        );

        this.client.Log += Log;
        this.client.Ready += this.ClientReady;
        this.client.SlashCommandExecuted += this.SlashCommandHandler;

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

        // get all command handlers from the `Commands` class
        Console.WriteLine();
        Console.WriteLine("------------------------------");
        Type type = typeof(Commands);

        var slashCommands = new List<SlashCommandBuilder>();

        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (MethodInfo method in methods)
        {
            var registerd = false;
            var problems = new List<string>();

            if (method.ReturnType == typeof(Task))
            {
                var attributes = method.GetCustomAttributes(true);

                foreach (var attribute in attributes)
                {
                    if (attribute.GetType() != typeof(SlashCommandAttribute))
                        continue;

                    if (method.GetParameters()[0].ParameterType != typeof(SocketSlashCommand))
                    {
                        problems.Add("it does not accept a SocketSlashCommand");
                        continue;
                    }

                    var slashCommandAttribute = (SlashCommandAttribute)attribute;

                    var guildCommand = new SlashCommandBuilder();

                    guildCommand.WithName(slashCommandAttribute.Name);
                    guildCommand.WithDescription(slashCommandAttribute.Description);

                    slashCommands.Add(guildCommand);
                    this.commands.Add(slashCommandAttribute.Name, method);

                    Console.WriteLine(
                        $"    [INFO] Registered Command {slashCommandAttribute.Name}"
                    );

                    registerd = true;
                    break;
                }

                if (!registerd)
                    problems.Add("it does not have a SlashCommandAttribute");
            }
            else
            {
                problems.Add("it does not return a Task");
            }

            if (!registerd)
            {
                Console.WriteLine($"    [WARN] {method.Name} not registered because:");
                foreach (var problem in problems)
                {
                    Console.WriteLine($"        - {problem}");
                }
            }
        }
        Console.WriteLine("------------------------------");
        Console.WriteLine();

        try
        {
            await guild.DeleteApplicationCommandsAsync();

            foreach (var slashCommand in slashCommands)
            {
                await guild.CreateApplicationCommandAsync(slashCommand.Build());
            }
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

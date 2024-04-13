using System.Reflection;
using Discord.Interactions;

class Program
{
    public static async Task Main()
    {
        var client = new WieBot();
        await client.Start();
    }
}

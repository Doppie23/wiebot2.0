using System.Reflection;
using Discord.Interactions;

// TODO: no way to join rng game yet, has to be done by hand

class Program
{
    public static async Task Main()
    {
        var client = new WieBot();
        await client.Start();
    }
}

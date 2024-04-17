namespace Utils;

public class Rng
{
    static readonly Random random = new();

    public static T RandomElement<T>(T[] values)
    {
        var i = random.Next(0, values.Length);
        return values[i];
    }

    public static Random Random
    {
        get { return random; }
    }

    public static Discord.Color RandomColor()
    {
        Discord.Color[] colors = new[]
        {
            Discord.Color.Teal,
            Discord.Color.DarkTeal,
            Discord.Color.Green,
            Discord.Color.DarkGreen,
            Discord.Color.Blue,
            Discord.Color.DarkBlue,
            Discord.Color.Purple,
            Discord.Color.DarkPurple,
            Discord.Color.Magenta,
            Discord.Color.DarkMagenta,
            Discord.Color.Gold,
            Discord.Color.LightOrange,
            Discord.Color.Orange,
            Discord.Color.DarkOrange,
            Discord.Color.Red,
            Discord.Color.DarkRed,
            Discord.Color.LightGrey,
            Discord.Color.LighterGrey,
            Discord.Color.DarkGrey,
            Discord.Color.DarkerGrey,
        };
        return RandomElement(colors);
    }
}

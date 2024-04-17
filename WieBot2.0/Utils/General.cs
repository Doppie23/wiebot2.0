namespace Utils;

public static class General
{
    public static Discord.Color RandomColor()
    {
        var random = new BetterRandom();
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
        return random.RandomElement(colors);
    }
}

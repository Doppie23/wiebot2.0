namespace Utils;

/// <summary>
///     A better random class with more methods
/// </summary>
public class BetterRandom : Random
{
    public T RandomElement<T>(T[] values)
    {
        var i = this.Next(0, values.Length);
        return values[i];
    }
}

using System.Diagnostics;
using Discord;
using Discord.Audio;

namespace Utils;

static class Voice
{
    public static async Task SendAsync(IAudioClient client, string path)
    {
        using var ffmpeg = Voice.CreateStream(path);
        using var output = ffmpeg.StandardOutput.BaseStream;
        using var discord = client.CreatePCMStream(AudioApplication.Voice);
        try
        {
            await output.CopyToAsync(discord);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            await discord.FlushAsync();
        }
    }

    private static Process CreateStream(string path)
    {
        return Process.Start(
            new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            }
        );
    }

    public static async Task<IGuildUser[]> GetUsersInVoiceChannelAsync(IVoiceChannel channel)
    {
        var users = new List<IGuildUser>();

        var peopleInVoiceChannel = channel.GetUsersAsync();
        await foreach (var usersBatch in peopleInVoiceChannel)
        {
            foreach (var user in usersBatch)
            {
                if (user.VoiceChannel == channel)
                {
                    users.Add(user);
                }
            }
        }

        return users.ToArray();
    }
}

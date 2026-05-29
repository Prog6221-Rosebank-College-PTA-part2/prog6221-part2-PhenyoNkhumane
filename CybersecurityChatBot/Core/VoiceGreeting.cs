using System.IO;
using System.Media;
public static class VoiceGreeting
{
    public static void PlayVoiceGreeting()
    {
        string path = Path.Combine(System.AppContext.BaseDirectory, "Greeting.wav");
        try
        {
            using (SoundPlayer player = new SoundPlayer(path))
            {
                player.PlaySync();
            }
        }
        catch
        {
            // Silently continue — missing audio must never crash the UI
        }
    }
}
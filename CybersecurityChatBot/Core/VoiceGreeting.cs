using System.IO;
using System.Media;

/// <summary>
/// Plays the WAV voice greeting when the application launches.
/// Unchanged from Part 1 except the console fallback is removed —
/// errors are silently swallowed so the UI is never blocked.
///
/// Place Greeting.wav in the project root and set:
///   Build Action  → Content
///   Copy to Output Directory → Copy if newer
/// </summary>
public static class VoiceGreeting
{
    /// <summary>
    /// Plays Greeting.wav synchronously from the application's base directory.
    /// Called once from MainWindow constructor before the UI is shown.
    /// Fails silently if the file is missing or audio is unavailable.
    /// </summary>
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

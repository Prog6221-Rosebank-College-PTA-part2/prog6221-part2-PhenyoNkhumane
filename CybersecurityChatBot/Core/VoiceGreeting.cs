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
        string baseDir = System.AppContext.BaseDirectory;
        string[] candidatePaths = new[]
        {
            Path.Combine(baseDir, "Greeting.wav"),
            Path.Combine(baseDir, "CybersecurityChatBot", "Greeting.wav"),
            Path.Combine(baseDir, "Assest", "Greeting.wav")
        };

        foreach (string path in candidatePaths)
        {
            if (!File.Exists(path))
                continue;

            try
            {
                using (SoundPlayer player = new SoundPlayer(path))
                {
                    player.PlaySync();
                }
                return;
            }
            catch
            {
                // Try the next candidate if playback fails.
            }
        }
    }
}

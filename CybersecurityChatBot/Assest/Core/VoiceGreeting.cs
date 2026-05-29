using System;
using System.IO;
using System.Media;

class VoiceGreeting
{
    public static void PlayVoiceGreeting()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Greeting.wav");
        try
        {
            using (SoundPlayer player = new SoundPlayer(path))
            {
                player.PlaySync();
            }
        }
        catch
        {
            Console.WriteLine("Audio could not be played.");
        }
    }
}

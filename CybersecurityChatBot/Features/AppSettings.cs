using System;

/// <summary>
/// Manages application settings per user (theme, sounds, voice greeting, etc.).
/// Persisted in the MySQL database.
/// </summary>
public class AppSettings
{
    public int UserId { get; set; }
    public bool DarkMode { get; set; } = true;
    public bool EnableSounds { get; set; } = true;
    public bool VoiceGreeting { get; set; } = true;
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Displays current settings in a formatted menu.
    /// </summary>
    public string DisplaySettings()
    {
        return $@"⚙️  Settings
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎨 Theme:              {(DarkMode ? "Dark" : "Light")} Mode
🔊 Sounds:             {(EnableSounds ? "Enabled" : "Disabled")} 🔇
🎤 Voice Greeting:     {(VoiceGreeting ? "Enabled" : "Disabled")} 📢

Actions:
  [ toggle dark mode ]    [ toggle sounds ]
  [ toggle voice ]        [ reset chat ]
  [ reset quiz ]          [ reset all tasks ]
  [ back to chat ]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
    }

    /// <summary>
    /// Toggles a setting and returns confirmation message.
    /// </summary>
    public string ToggleSetting(string settingName)
    {
        var lower = settingName.ToLowerInvariant();
        
        if (lower == "dark mode" || lower == "theme")
        {
            DarkMode = !DarkMode;
            return $"Theme changed to {(DarkMode ? "Dark" : "Light")} Mode ✓";
        }
        
        if (lower == "sounds")
        {
            EnableSounds = !EnableSounds;
            return $"Sounds {(EnableSounds ? "Enabled" : "Disabled")} ✓";
        }
        
        if (lower == "voice" || lower == "voice greeting")
        {
            VoiceGreeting = !VoiceGreeting;
            return $"Voice Greeting {(VoiceGreeting ? "Enabled" : "Disabled")} ✓";
        }
        
        return "Setting not recognized.";
    }
}

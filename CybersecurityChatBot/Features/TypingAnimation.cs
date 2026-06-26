using System;
using System.Threading.Tasks;

/// <summary>
/// Handles typing animation display for bot responses.
/// Shows "Mavicks is typing..." with animated dots.
/// </summary>
public static class TypingAnimation
{
    /// <summary>
    /// Generates a typing indicator message.
    /// </summary>
    public static string GetTypingIndicator()
    {
        return "✏️  Mavicks is typing...";
    }

    /// <summary>
    /// Returns an animated typing state (for UI updates).
    /// </summary>
    public static string GetAnimatedTypingState(int frameNumber)
    {
        string dots = (frameNumber % 3) switch
        {
            0 => "•",
            1 => "••",
            _ => "•••"
        };
        return $"✏️  Mavicks is typing{dots}";
    }

    /// <summary>
    /// Simulates typing delay in milliseconds.
    /// </summary>
    public static int GetTypingDelay() => 800; // 800ms before showing response
}

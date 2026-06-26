using System;

/// <summary>
/// Represents a user's complete profile, including statistics and preferences.
/// Persisted in the MySQL database and updated throughout the session.
/// </summary>
public class UserProfile
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FavouriteTopic { get; set; }
    public int QuizBestScore { get; set; } = 0;
    public int QuizAttempts { get; set; } = 0;
    public int QuizAverageScore { get; set; } = 0;
    public int TotalTasks { get; set; } = 0;
    public int CompletedTasks { get; set; } = 0;
    public DateTime LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Calculated property: number of pending tasks.</summary>
    public int PendingTasks => TotalTasks - CompletedTasks;

    /// <summary>Calculated property: completion percentage.</summary>
    public int CompletionPercentage => TotalTasks > 0
        ? (int)((double)CompletedTasks / TotalTasks * 100)
        : 0;

    /// <summary>Formatted greeting based on time of day.</summary>
    public string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        string period = hour < 12 ? "Morning" : hour < 17 ? "Afternoon" : "Evening";
        return $"Good {period}, {Name} 👋";
    }

    /// <summary>Formatted status line for display.</summary>
    public string GetStatusLine()
    {
        return $"📋 Pending: {PendingTasks} | ✅ Completed: {CompletedTasks} | 🎮 Best: {QuizBestScore}/12";
    }
}

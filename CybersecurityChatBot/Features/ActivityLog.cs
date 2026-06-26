using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Records significant chatbot actions with timestamps for the activity log feature.
/// </summary>
public static class ActivityLog
{
    private const int MaxEntries = 50;

    private static readonly List<ActivityEntry> _entries = new List<ActivityEntry>();

    public static void Log(string description)
    {
        _entries.Insert(0, new ActivityEntry(DateTime.Now, description));

        if (_entries.Count > MaxEntries)
            _entries.RemoveAt(_entries.Count - 1);

        try
        {
            TaskDatabase.LogActivity(description);
        }
        catch
        {
            // Keep activity logging resilient even when the database is unavailable.
        }
    }

    public static string FormatRecent(int count = 8, bool showAll = false)
    {
        if (_entries.Count == 0)
            return "No actions recorded yet. Try adding a task, setting a reminder, or starting the quiz!";

        IEnumerable<ActivityEntry> slice = showAll
            ? _entries
            : _entries.Take(Math.Min(count, _entries.Count));

        var sb = new StringBuilder();
        sb.AppendLine("📅 Activity Log");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        foreach (ActivityEntry entry in slice)
        {
            // Format with emoji icons based on action type
            string icon = entry.Description switch
            {
                var s when s.Contains("Quiz") => "🎮",
                var s when s.Contains("Task") || s.Contains("task") => "📝",
                var s when s.Contains("Completed") || s.Contains("completed") => "✅",
                var s when s.Contains("Reminder") || s.Contains("reminder") => "🔔",
                var s when s.Contains("Password") || s.Contains("password") => "🔐",
                var s when s.Contains("Phishing") || s.Contains("phishing") => "🎣",
                _ => "📌"
            };

            sb.AppendLine($"{icon} {entry.Timestamp:HH:mm}");
            sb.AppendLine($"   {entry.Description}");
            sb.AppendLine();
        }

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        if (!showAll && _entries.Count > count)
            sb.AppendLine($"({_entries.Count - count} older entries — type \"show more activity log\" to see all)");

        return sb.ToString().TrimEnd();
    }

    public static IReadOnlyList<ActivityEntry> GetEntries() => _entries.AsReadOnly();
}

public readonly record struct ActivityEntry(DateTime Timestamp, string Description);

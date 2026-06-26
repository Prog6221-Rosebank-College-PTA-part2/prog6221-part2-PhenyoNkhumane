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
        sb.AppendLine("Here's a summary of recent actions:");

        int index = 1;
        foreach (ActivityEntry entry in slice)
        {
            sb.AppendLine($"  {index}. [{entry.Timestamp:HH:mm dd MMM}] {entry.Description}");
            index++;
        }

        if (!showAll && _entries.Count > count)
            sb.AppendLine($"\n({ _entries.Count - count} older entries hidden — type \"show more activity log\" to see all.)");

        return sb.ToString().TrimEnd();
    }

    public static IReadOnlyList<ActivityEntry> GetEntries() => _entries.AsReadOnly();
}

public readonly record struct ActivityEntry(DateTime Timestamp, string Description);

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Simulates NLP using keyword detection, phrase lists, and regular expressions
/// to recognise user intent even when phrasing varies.
/// </summary>
public static class NlpIntentDetector
{
    public enum Intent
    {
        None,
        AddTask,
        ViewTasks,
        DeleteTask,
        CompleteTask,
        SetReminder,
        StartQuiz,
        ShowActivityLog,
        ShowMoreActivityLog,
        ConfirmYes,
        ConfirmNo
    }

    public sealed class IntentResult
    {
        public Intent Intent { get; set; }
        public string? TaskTitle { get; set; }
        public string? ReminderPhrase { get; set; }
        public int? TaskId { get; set; }
    }

    private static readonly string[] AddTaskPhrases =
    {
        "add task", "add a task", "create task", "create a task", "new task",
        "add todo", "create todo", "make a task", "set a task", "add-task"
    };

    private static readonly string[] ViewTaskPhrases =
    {
        "view tasks", "show tasks", "list tasks", "my tasks", "see tasks",
        "display tasks", "what tasks", "show my tasks", "task list"
    };

    private static readonly string[] DeleteTaskPhrases =
    {
        "delete task", "remove task", "delete a task", "remove a task"
    };

    private static readonly string[] CompleteTaskPhrases =
    {
        "complete task", "mark task", "mark as complete", "finish task",
        "task done", "task completed", "done with task"
    };

    private static readonly string[] ReminderPhrases =
    {
        "remind me", "set a reminder", "set reminder", "schedule reminder",
        "reminder for", "remind me to", "can you remind me", "don't let me forget"
    };

    private static readonly string[] QuizPhrases =
    {
        "start quiz", "play quiz", "begin quiz", "quiz me", "cyber quiz",
        "start the quiz", "play the quiz", "mini game", "start game", "take quiz"
    };

    private static readonly string[] ActivityLogPhrases =
    {
        "show activity log", "activity log", "what have you done",
        "what have you done for me", "show log", "recent actions",
        "what did you do", "action log", "show actions"
    };

    private static readonly string[] ShowMoreLogPhrases =
    {
        "show more activity log", "show full log", "show all activity",
        "full activity log", "show more log"
    };

    private static readonly string[] YesPhrases =
    {
        "yes", "yeah", "yep", "sure", "ok", "okay", "please", "definitely", "absolutely"
    };

    private static readonly string[] NoPhrases =
    {
        "no", "nope", "nah", "not now", "skip", "no thanks", "don't"
    };

    public static IntentResult Detect(string rawInput)
    {
        string input = rawInput.ToLowerInvariant().Trim();
        var result   = new IntentResult { Intent = Intent.None };

        if (ContainsAny(input, ShowMoreLogPhrases))
        {
            result.Intent = Intent.ShowMoreActivityLog;
            return result;
        }

        if (ContainsAny(input, ActivityLogPhrases))
        {
            result.Intent = Intent.ShowActivityLog;
            return result;
        }

        if (ContainsAny(input, QuizPhrases))
        {
            result.Intent = Intent.StartQuiz;
            return result;
        }

        if (ContainsAny(input, ViewTaskPhrases))
        {
            result.Intent = Intent.ViewTasks;
            return result;
        }

        if (ContainsAny(input, DeleteTaskPhrases))
        {
            result.Intent = Intent.DeleteTask;
            result.TaskId = TryExtractTaskId(input);
            result.TaskTitle = TryExtractTaskTitleAfterKeyword(input, DeleteTaskPhrases);
            return result;
        }

        if (ContainsAny(input, CompleteTaskPhrases))
        {
            result.Intent = Intent.CompleteTask;
            result.TaskId = TryExtractTaskId(input);
            result.TaskTitle = TryExtractTaskTitleAfterKeyword(input, CompleteTaskPhrases);
            return result;
        }

        // Reminder-only or combined reminder + task
        if (ContainsAny(input, ReminderPhrases))
        {
            result.Intent = Intent.SetReminder;
            result.TaskTitle = ExtractReminderTaskTitle(rawInput, input);
            result.ReminderPhrase = ExtractReminderTimeframe(input);
            return result;
        }

        if (ContainsAny(input, AddTaskPhrases))
        {
            result.Intent = Intent.AddTask;
            result.TaskTitle = ExtractTaskTitleFromAddPhrase(rawInput, input);
            return result;
        }

        // Flexible: "enable 2FA" as task if contains task-related keywords
        if (input.Contains("task") && !input.Contains("view") && !input.Contains("delete"))
        {
            result.Intent = Intent.AddTask;
            result.TaskTitle = ExtractLooseTaskTitle(rawInput);
            return result;
        }

        if (IsAffirmative(input))
        {
            result.Intent = Intent.ConfirmYes;
            result.ReminderPhrase = ExtractReminderTimeframe(input);
            return result;
        }

        if (IsNegative(input))
        {
            result.Intent = Intent.ConfirmNo;
            return result;
        }

        return result;
    }

    private static bool ContainsAny(string input, IEnumerable<string> phrases)
    {
        foreach (string phrase in phrases)
        {
            if (input.Contains(phrase))
                return true;
        }
        return false;
    }

    private static bool IsAffirmative(string input)
    {
        foreach (string phrase in YesPhrases)
        {
            if (input == phrase || input.StartsWith(phrase + " ") || input.StartsWith(phrase + ","))
                return true;
        }

        // "yes, remind me in 3 days"
        if (input.StartsWith("yes") || input.StartsWith("sure"))
            return ExtractReminderTimeframe(input) != null || input.Length <= 20;

        return false;
    }

    private static bool IsNegative(string input)
    {
        foreach (string phrase in NoPhrases)
        {
            if (input == phrase || input.StartsWith(phrase + " ") || input.StartsWith(phrase + ","))
                return true;
        }
        return false;
    }

    private static string? ExtractTaskTitleFromAddPhrase(string raw, string lower)
    {
        foreach (string phrase in AddTaskPhrases)
        {
            int idx = lower.IndexOf(phrase, StringComparison.Ordinal);
            if (idx < 0) continue;

            string remainder = raw.Substring(idx + phrase.Length).Trim();
            remainder = remainder.TrimStart('-', ':', ' ').Trim();

            if (remainder.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
                remainder = remainder.Substring(3).Trim();

            return CleanTitle(remainder);
        }

        return ExtractLooseTaskTitle(raw);
    }

    private static string? ExtractLooseTaskTitle(string raw)
    {
        string cleaned = raw.Trim();
        cleaned = Regex.Replace(cleaned, @"^(please|can you|could you)\s+", "", RegexOptions.IgnoreCase);
        return CleanTitle(cleaned);
    }

    private static string? ExtractReminderTaskTitle(string raw, string lower)
    {
        foreach (string phrase in ReminderPhrases)
        {
            int idx = lower.IndexOf(phrase, StringComparison.Ordinal);
            if (idx < 0) continue;

            string remainder = raw.Substring(idx + phrase.Length).Trim();

            // Strip timeframe from end: "update my password tomorrow"
            remainder = StripTimeframeFromEnd(remainder);
            remainder = remainder.TrimStart("to ".ToCharArray());

            return CleanTitle(remainder);
        }

        return null;
    }

    private static string? ExtractReminderTimeframe(string input)
    {
        var patterns = new[]
        {
            @"in\s+(\d+)\s+(day|days|week|weeks|hour|hours|minute|minutes)",
            @"(\d+)\s+(day|days|week|weeks)",
            @"tomorrow",
            @"next week",
            @"today",
            @"tonight"
        };

        foreach (string pattern in patterns)
        {
            Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Value.Trim();
        }

        return null;
    }

    private static string StripTimeframeFromEnd(string text)
    {
        string[] timeWords =
        {
            "tomorrow", "today", "tonight", "next week"
        };

        string lower = text.ToLowerInvariant();
        foreach (string word in timeWords)
        {
            if (lower.EndsWith(word))
                return text.Substring(0, text.Length - word.Length).Trim().TrimEnd(',', '.');
        }

        var match = Regex.Match(text,
            @"\s+in\s+\d+\s+(day|days|week|weeks|hour|hours)\.?$",
            RegexOptions.IgnoreCase);
        if (match.Success)
            return text.Substring(0, match.Index).Trim();

        return text.Trim().TrimEnd('.', ',');
    }

    private static int? TryExtractTaskId(string input)
    {
        Match match = Regex.Match(input, @"\b(?:task|#)\s*(\d+)\b", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    private static string? TryExtractTaskTitleAfterKeyword(string raw, string[] keywords)
    {
        string lower = raw.ToLowerInvariant();
        foreach (string keyword in keywords)
        {
            int idx = lower.IndexOf(keyword, StringComparison.Ordinal);
            if (idx < 0) continue;

            string remainder = raw.Substring(idx + keyword.Length).Trim();
            remainder = remainder.TrimStart('-', ':', ' ').Trim();

            Match idMatch = Regex.Match(remainder, @"^\d+\s*[-:]?\s*");
            if (idMatch.Success)
                remainder = remainder.Substring(idMatch.Length);

            return CleanTitle(remainder);
        }
        return null;
    }

    private static string? CleanTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        title = title.Trim().TrimEnd('.', '!', '?');
        return title.Length > 0 ? title : null;
    }
}

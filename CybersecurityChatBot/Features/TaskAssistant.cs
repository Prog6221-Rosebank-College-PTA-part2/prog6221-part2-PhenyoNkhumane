using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Manages cybersecurity tasks with MySQL storage and a conversational reminder flow.
/// </summary>
public class TaskAssistant
{
    private enum PendingState
    {
        None,
        AwaitingReminderChoice
    }

    private PendingState _pendingState = PendingState.None;
    private int? _pendingTaskId;
    private string? _pendingTaskTitle;

    public bool HasPendingPrompt => _pendingState != PendingState.None;

    public string Process(NlpIntentDetector.IntentResult intent, string rawInput)
    {
        if (_pendingState == PendingState.AwaitingReminderChoice)
            return HandlePendingReminder(intent, rawInput);

        return intent.Intent switch
        {
            NlpIntentDetector.Intent.AddTask => HandleAddTask(intent),
            NlpIntentDetector.Intent.SetReminder => HandleSetReminder(intent),
            NlpIntentDetector.Intent.ViewTasks => HandleViewTasks(),
            NlpIntentDetector.Intent.DeleteTask => HandleDeleteTask(intent),
            NlpIntentDetector.Intent.CompleteTask => HandleCompleteTask(intent),
            _ => string.Empty
        };
    }

    public string FormatTasksForDisplay()
    {
        try
        {
            List<CyberTask> tasks = TaskDatabase.GetAllTasks();
            if (tasks.Count == 0)
                return "You have no tasks yet. Try: \"Add task - Enable two-factor authentication\"";

            var sb = new StringBuilder();
            sb.AppendLine("Your cybersecurity tasks:");
            sb.AppendLine();

            foreach (CyberTask task in tasks)
            {
                string status = task.IsCompleted ? "✅" : "⏳";
                sb.AppendLine($"{status} [{task.Id}] {task.Title}");
                sb.AppendLine($"    {task.Description}");
                sb.AppendLine($"    Reminder: {task.ReminderDisplay}");
                sb.AppendLine();
            }

            sb.AppendLine("Say \"complete task 1\" or \"delete task 2\" to manage tasks.");
            return sb.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "load tasks");
        }
    }

    public string AddTaskFromUi(string title, string description, DateTime? reminderDate, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "⚠ Please enter a task title.";

        if (string.IsNullOrWhiteSpace(description))
            return "⚠ Please enter a task description.";

        try
        {
            int id = TaskDatabase.AddTask(title, description, reminderDate, dueDate);
            ActivityLog.Log($"Task added: '{title}'.");

            string reminderLine = reminderDate.HasValue
                ? $"Reminder: {FormatFriendlyDate(reminderDate.Value)}"
                : "Reminder: none";

            return $"✓ Task added successfully!\n\nTitle: {title}\n{reminderLine}\n\nGood luck staying secure!";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "save your task");
        }
    }

    public string CompleteTaskById(int id)
    {
        try
        {
            CyberTask? task = TaskDatabase.GetTaskById(id);
            if (task == null)
                return $"No task found with ID {id}.";

            bool success = TaskDatabase.MarkCompleted(id);
            if (!success)
                return $"I couldn't update task {id}.";

            ActivityLog.Log($"Task completed: '{task.Title}'.");
            return "✓ Task marked as completed.\n\nWell done!";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "complete the task");
        }
    }

    public string DeleteTaskById(int id)
    {
        try
        {
            CyberTask? task = TaskDatabase.GetTaskById(id);
            if (task == null)
                return $"No task found with ID {id}.";

            bool success = TaskDatabase.DeleteTask(id);
            if (!success)
                return $"I couldn't delete task {id}.";

            ActivityLog.Log($"Task deleted: '{task.Title}'.");
            return "🗑 Task deleted successfully.";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "delete the task");
        }
    }

    public IReadOnlyList<CyberTask> GetTasks() => TaskDatabase.GetAllTasks();

    private string HandleAddTask(NlpIntentDetector.IntentResult intent)
    {
        string? title = intent.TaskTitle;
        if (string.IsNullOrWhiteSpace(title))
            return "What task would you like to add? For example: \"Add task - Review privacy settings\"";

        string description = BuildDescription(title);

        try
        {
            int id = TaskDatabase.AddTask(title, description, null, null);
            _pendingTaskId = id;
            _pendingTaskTitle = title;
            _pendingState = PendingState.AwaitingReminderChoice;

            ActivityLog.Log($"Task added: '{title}' (no reminder set yet).");

            return $"Task added with the description \"{description}\". Would you like a reminder?";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "save the task");
        }
    }

    private string HandleSetReminder(NlpIntentDetector.IntentResult intent)
    {
        string? title = intent.TaskTitle;
        if (string.IsNullOrWhiteSpace(title))
            return "What should I remind you about? For example: \"Remind me to update my password tomorrow.\"";

        string description = BuildDescription(title);
        DateTime? reminderDate = ParseReminderDate(intent.ReminderPhrase ?? string.Empty);

        if (!reminderDate.HasValue)
        {
            try
            {
                int id = TaskDatabase.AddTask(title, description, null, null);
                _pendingTaskId = id;
                _pendingTaskTitle = title;
                _pendingState = PendingState.AwaitingReminderChoice;
                ActivityLog.Log($"Task added via reminder request: '{title}'.");
                return $"Task added: '{title}.' When would you like the reminder? (e.g. \"in 3 days\" or \"tomorrow\")";
            }
            catch (Exception ex)
            {
                return $"Could not save task: {ex.Message}";
            }
        }

        try
        {
            int id = TaskDatabase.AddTask(title, description, reminderDate, null);
            ActivityLog.Log($"Reminder set for '{title}' on {reminderDate.Value:dd MMM yyyy}.");
            return $"Reminder set for '{title}' on {FormatFriendlyDate(reminderDate.Value)}.";
        }
        catch (Exception ex)
        {
            return $"Could not set reminder: {ex.Message}";
        }
    }

    private string HandlePendingReminder(NlpIntentDetector.IntentResult intent, string rawInput)
    {
        if (intent.Intent == NlpIntentDetector.Intent.ConfirmNo)
        {
            ClearPending();
            return "No problem — the task was saved without a reminder.";
        }

        string timeframe = intent.ReminderPhrase ?? rawInput.Trim();
        DateTime? reminderDate = ParseReminderDate(timeframe);

        if (!reminderDate.HasValue && intent.Intent == NlpIntentDetector.Intent.ConfirmYes)
            return "When should I remind you? For example: \"in 7 days\" or \"tomorrow\".";

        if (!reminderDate.HasValue && intent.Intent == NlpIntentDetector.Intent.None)
            return "Would you like a reminder? Reply \"yes, in 3 days\" or \"no\".";

        if (!reminderDate.HasValue)
            return "I didn't catch the reminder time. Try: \"Yes, remind me in 3 days.\"";

        if (!_pendingTaskId.HasValue)
        {
            ClearPending();
            return "Something went wrong with the pending task. Please add the task again.";
        }

        try
        {
            TaskDatabase.SetReminder(_pendingTaskId.Value, reminderDate.Value);
            string title = _pendingTaskTitle ?? "your task";
            ActivityLog.Log($"Reminder set for '{title}' on {reminderDate.Value:dd MMM yyyy}.");
            ClearPending();
            return $"Got it! I'll remind you about '{title}' on {FormatFriendlyDate(reminderDate.Value)}.";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "set the reminder");
        }
    }

    private string HandleViewTasks()
    {
        ActivityLog.Log("Viewed tasks.");
        return FormatTasksForDisplay();
    }

    private string HandleDeleteTask(NlpIntentDetector.IntentResult intent)
    {
        try
        {
            int? id = intent.TaskId ?? FindTaskIdByTitle(intent.TaskTitle);
            if (!id.HasValue)
                return "Which task should I delete? Say \"delete task 1\" or include the task title.";

            CyberTask? task = TaskDatabase.GetTaskById(id.Value);
            if (task == null)
                return $"No task found with ID {id.Value}.";

            TaskDatabase.DeleteTask(id.Value);
            ActivityLog.Log($"Task deleted: '{task.Title}'.");
            return $"Deleted task: '{task.Title}'.";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "delete the task");
        }
    }

    private string HandleCompleteTask(NlpIntentDetector.IntentResult intent)
    {
        try
        {
            int? id = intent.TaskId ?? FindTaskIdByTitle(intent.TaskTitle);
            if (!id.HasValue)
                return "Which task is complete? Say \"complete task 1\" or include the task title.";

            CyberTask? task = TaskDatabase.GetTaskById(id.Value);
            if (task == null)
                return $"No task found with ID {id.Value}.";

            TaskDatabase.MarkCompleted(id.Value);
            ActivityLog.Log($"Task marked completed: '{task.Title}'.");
            return $"Marked '{task.Title}' as completed. Great work staying on top of your security!";
        }
        catch (Exception ex)
        {
            return BuildDatabaseErrorMessage(ex, "complete the task");
        }
    }

    private static int? FindTaskIdByTitle(string? titleFragment)
    {
        if (string.IsNullOrWhiteSpace(titleFragment))
            return null;

        string lower = titleFragment.ToLowerInvariant();
        CyberTask? match = TaskDatabase.GetAllTasks()
            .FirstOrDefault(t => t.Title.ToLowerInvariant().Contains(lower));

        return match?.Id;
    }

    private static string BuildDescription(string title)
    {
        string lower = title.ToLowerInvariant();

        if (lower.Contains("privacy"))
            return "Review account privacy settings to ensure your data is protected.";
        if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor"))
            return "Enable two-factor authentication to add an extra layer of account security.";
        if (lower.Contains("password"))
            return "Update your password using a strong, unique passphrase and a password manager.";
        if (lower.Contains("phishing"))
            return "Review recent emails and report any suspicious phishing attempts.";
        if (lower.Contains("backup"))
            return "Back up important files to a secure, offline or cloud location.";
        if (lower.Contains("update") || lower.Contains("patch"))
            return "Install the latest security updates for your devices and applications.";

        return $"Complete this cybersecurity task: {title}.";
    }

    public static DateTime? ParseReminderDate(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return null;

        string lower = phrase.ToLowerInvariant().Trim();
        DateTime now = DateTime.Now;

        if (lower.Contains("tomorrow"))
            return now.Date.AddDays(1).AddHours(9);

        if (lower.Contains("today") || lower.Contains("tonight"))
            return now.AddHours(2);

        if (lower.Contains("next week"))
            return now.Date.AddDays(7).AddHours(9);

        Match match = Regex.Match(lower, @"in\s+(\d+)\s+(day|days|week|weeks|hour|hours)");
        if (match.Success)
        {
            int amount = int.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value;
            return unit.StartsWith("week")
                ? now.AddDays(amount * 7)
                : unit.StartsWith("hour")
                    ? now.AddHours(amount)
                    : now.AddDays(amount);
        }

        match = Regex.Match(lower, @"(\d+)\s+(day|days|week|weeks)");
        if (match.Success)
        {
            int amount = int.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value;
            return unit.StartsWith("week") ? now.AddDays(amount * 7) : now.AddDays(amount);
        }

        return null;
    }

    private static string FormatFriendlyDate(DateTime date) =>
        date.Date == DateTime.Today
            ? "today"
            : date.Date == DateTime.Today.AddDays(1)
                ? "tomorrow"
                : date.ToString("dd MMM yyyy 'at' HH:mm");

    private static string BuildDatabaseErrorMessage(Exception ex, string action)
    {
        System.Diagnostics.Debug.WriteLine($"Task action failed during {action}: {ex}");

        string message = ex.Message ?? string.Empty;
        bool isConnectionIssue = message.IndexOf("Unable to connect", StringComparison.OrdinalIgnoreCase) >= 0 ||
            message.IndexOf("MySQL", StringComparison.OrdinalIgnoreCase) >= 0 && message.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0;

        if (isConnectionIssue)
        {
            return "⚠ Task Manager is currently offline.\n\nI can't save tasks right now because the database connection isn't available.\n\nPlease check the database connection and try again.";
        }

        return $"⚠ I couldn't {action} right now.";
    }

    private void ClearPending()
    {
        _pendingState = PendingState.None;
        _pendingTaskId = null;
        _pendingTaskTitle = null;
    }
}

using System;
using System.Collections.Generic;

/// <summary>
/// Orchestrates Part 3 features: tasks, quiz, activity log, and NLP intent routing.
/// </summary>
public class Part3FeatureManager
{
    private readonly TaskAssistant _tasks = new TaskAssistant();
    private readonly QuizGame _quiz = new QuizGame();

    public bool QuizIsActive => _quiz.IsActive;
    public int QuizScore => _quiz.CurrentScore;
    public int QuizQuestionNumber => _quiz.CurrentQuestionNumber;
    public int QuizTotalQuestions => _quiz.TotalQuestions;

    public string? TryHandle(string rawInput)
    {
        string trimmed = rawInput.Trim();
        var intent = NlpIntentDetector.Detect(trimmed);

        // Quiz answers take priority when a quiz is in progress
        if (_quiz.IsActive)
        {
            string? quizResponse = _quiz.ProcessAnswer(trimmed);
            if (quizResponse != null)
                return quizResponse;
        }

        // Pending task reminder flow
        if (_tasks.HasPendingPrompt)
        {
            string taskResponse = _tasks.Process(intent, trimmed);
            if (!string.IsNullOrEmpty(taskResponse))
                return taskResponse;
        }

        if (intent.Intent != NlpIntentDetector.Intent.None)
        {
            ActivityLog.Log($"Recognised NLP intent: {intent.Intent}.");
        }

        switch (intent.Intent)
        {
            case NlpIntentDetector.Intent.ShowDashboard:
                ActivityLog.Log("User requested dashboard view.");
                return "[DASHBOARD]"; // Special marker for UI

            case NlpIntentDetector.Intent.ShowStatistics:
                ActivityLog.Log("User requested statistics.");
                return "[STATISTICS]"; // Special marker for UI

            case NlpIntentDetector.Intent.ShowSettings:
                ActivityLog.Log("User requested settings.");
                return "[SETTINGS]"; // Special marker for UI

            case NlpIntentDetector.Intent.ShowHelp:
                ActivityLog.Log("User requested help.");
                return "[HELP]"; // Special marker for UI

            case NlpIntentDetector.Intent.ShowSuggestions:
                ActivityLog.Log("User requested suggestions.");
                return "[SUGGESTIONS]"; // Special marker for UI

            case NlpIntentDetector.Intent.ShowActivityLog:
                return ActivityLog.FormatRecent();

            case NlpIntentDetector.Intent.ShowMoreActivityLog:
                return ActivityLog.FormatRecent(showAll: true);

            case NlpIntentDetector.Intent.StartQuiz:
                return _quiz.Start();

            case NlpIntentDetector.Intent.ViewTasks:
                return _tasks.FormatTasksForDisplay();

            case NlpIntentDetector.Intent.AddTask:
            case NlpIntentDetector.Intent.SetReminder:
            case NlpIntentDetector.Intent.DeleteTask:
            case NlpIntentDetector.Intent.CompleteTask:
                string result = _tasks.Process(intent, trimmed);
                if (!string.IsNullOrEmpty(result))
                    return result;
                break;
        }

        return null;
    }

    public IReadOnlyList<CyberTask> GetTasks()
    {
        try
        {
            return TaskDatabase.GetAllTasks();
        }
        catch
        {
            return Array.Empty<CyberTask>();
        }
    }

    public string AddTask(string title, string description, DateTime? reminderDate, DateTime? dueDate) =>
        _tasks.AddTaskFromUi(title, description, reminderDate, dueDate);

    public string CompleteTask(int id) => _tasks.CompleteTaskById(id);

    public string DeleteTask(int id) => _tasks.DeleteTaskById(id);

    public string GetDatabaseStatusMessage()
    {
        if (TaskDatabase.IsAvailable)
            return "Task database connected.";

        return $"Task database offline: {TaskDatabase.LastError ?? "unknown error"}. " +
               "Update dbconfig.json or set MAVICKS_DB_CONNECTION.";
    }

    public string ToggleSettings(string settingName)
    {
        int userId = TaskDatabase.CurrentUserId;
        var settings = TaskDatabase.GetOrCreateUserSettings(userId);
        if (settings == null)
            return "⚠ Settings are not available right now.";

        string result = settings.ToggleSetting(settingName);
        TaskDatabase.UpdateUserSettings(userId, settings);
        return result;
    }
}

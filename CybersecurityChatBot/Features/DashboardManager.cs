using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Manages the home dashboard display with statistics, pending reminders, and quick actions.
/// </summary>
public static class DashboardManager
{
    private static Random _random = new Random();

    /// <summary>
    /// Random cybersecurity tips shown on dashboard load.
    /// </summary>
    private static readonly string[] CyberTips = new string[]
    {
        "Never reuse passwords across multiple accounts.",
        "Enable two-factor authentication (2FA) on all important accounts.",
        "Always verify sender email addresses before clicking links.",
        "Use strong passwords: at least 12 characters with symbols.",
        "Keep your software and operating system up to date.",
        "Be cautious of public WiFi networks — use a VPN.",
        "Review your security settings monthly.",
        "Never share your passwords with anyone, even IT support.",
        "Backup your important files regularly.",
        "Use a password manager to store complex passwords securely."
    };

    /// <summary>
    /// Random cybersecurity facts shown on dashboard.
    /// </summary>
    private static readonly string[] CyberFacts = new string[]
    {
        "Over 90% of cyber attacks begin with phishing emails.",
        "The average data breach costs organizations $4.29 million.",
        "81% of breaches use weak or stolen passwords.",
        "Zero-day exploits are sold on dark web for $50k-$500k.",
        "A new vulnerability is discovered every 18 hours.",
        "Cybercrime damages will reach $10.5 trillion by 2025.",
        "Ransomware attacks happen every 11 seconds globally.",
        "60% of companies experience a breach within 6 months.",
        "Biometric attacks increased by 300% in 2023.",
        "Social engineering succeeds 70% of the time."
    };

    /// <summary>
    /// Generates the home dashboard UI.
    /// </summary>
    public static string GenerateDashboard(UserProfile profile, int pendingTasks, int completedTasks, int quizAttempts, string? lastTopic)
    {
        var sb = new StringBuilder();

        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
        sb.AppendLine($"               {profile.GetGreeting()}");
        sb.AppendLine($"               Today's Date: {DateTime.Now:dd MMMM yyyy}");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine($"📋 Pending Tasks: {pendingTasks}");
        sb.AppendLine($"✅ Completed Tasks: {completedTasks}");
        sb.AppendLine($"🎮 Quiz Best Score: {profile.QuizBestScore}/12");
        if (!string.IsNullOrEmpty(lastTopic))
            sb.AppendLine($"🧠 Last Topic: {lastTopic}");
        sb.AppendLine();
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("[ 🔐 Start Chat ]    [ 📋 View Tasks ]    [ 🎮 Start Quiz ]");
        sb.AppendLine("[ ⚙️  Settings ]      [ 📖 Help ]          [ 📊 Statistics ]");
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates the statistics panel.
    /// </summary>
    public static string GenerateStatisticsPanel(UserProfile profile)
    {
        var sb = new StringBuilder();

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━ 📊 Statistics ━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"Total Tasks:         {profile.TotalTasks}");
        sb.AppendLine($"Completed:           {profile.CompletedTasks}");
        sb.AppendLine($"Pending:             {profile.PendingTasks}");
        sb.AppendLine($"Completion Rate:     {profile.CompletionPercentage}%");
        sb.AppendLine();
        sb.AppendLine($"Quiz Attempts:       {profile.QuizAttempts}");
        sb.AppendLine($"Best Score:          {profile.QuizBestScore}/12");
        sb.AppendLine($"Average Score:       {profile.QuizAverageScore}%");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        return sb.ToString();
    }

    /// <summary>
    /// Returns today's random cyber tip.
    /// </summary>
    public static string GetDailyTip()
    {
        int index = _random.Next(CyberTips.Length);
        return $"💡 Today's Cyber Tip\n{CyberTips[index]}";
    }

    /// <summary>
    /// Returns today's random cybersecurity fact.
    /// </summary>
    public static string GetDailyFact()
    {
        int index = _random.Next(CyberFacts.Length);
        return $"🔍 Did you know?\n{CyberFacts[index]}";
    }

    /// <summary>
    /// Generates a formatted reminder notification.
    /// </summary>
    public static string GenerateReminderNotification(string taskTitle, DateTime dueDate)
    {
        return $"🔔 Reminder\n{taskTitle}\nDue: {dueDate:dddd, dd MMMM}\nWould you like to mark it complete?";
    }

    /// <summary>
    /// Returns conversation suggestions as formatted options.
    /// </summary>
    public static string GetConversationSuggestions()
    {
        var sb = new StringBuilder();
        sb.AppendLine("💬 What would you like to do?");
        sb.AppendLine("  [ 🔐 Password Safety ]");
        sb.AppendLine("  [ 🎣 Phishing ]");
        sb.AppendLine("  [ 🛡️  Privacy ]");
        sb.AppendLine("  [ 🎮 Start Quiz ]");
        sb.AppendLine("  [ 📋 View Tasks ]");
        return sb.ToString();
    }
}

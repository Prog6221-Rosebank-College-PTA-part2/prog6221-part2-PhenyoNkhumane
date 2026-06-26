using System;

/// <summary>
/// Represents a cybersecurity task stored in MySQL.
/// </summary>
public class CyberTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ReminderDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ReminderDisplay =>
        ReminderDate.HasValue
            ? ReminderDate.Value.ToString("dd MMM yyyy HH:mm")
            : "No reminder set";

    public string DueDisplay =>
        DueDate.HasValue
            ? DueDate.Value.ToString("dd MMM yyyy HH:mm")
            : "No due date";

    public string StatusDisplay => IsCompleted ? "Completed" : "Pending";

    public string PriorityDisplay => InferPriority();

    public string CategoryDisplay => InferCategory();

    public bool IsHighPriority => PriorityDisplay.Equals("High", StringComparison.OrdinalIgnoreCase);

    public bool IsDueToday => DueDate.HasValue && !IsCompleted && DueDate.Value.Date == DateTime.Today;

    private string InferPriority()
    {
        string text = $"{Title} {Description}".ToLowerInvariant();
        if (text.Contains("urgent") || text.Contains("critical") || text.Contains("2fa") || text.Contains("password") || text.Contains("privacy") || text.Contains("phishing"))
            return "High";

        if (text.Contains("review") || text.Contains("update") || text.Contains("backup") || text.Contains("reminder"))
            return "Medium";

        return "Low";
    }

    private string InferCategory()
    {
        string text = $"{Title} {Description}".ToLowerInvariant();
        if (text.Contains("password")) return "Passwords";
        if (text.Contains("privacy")) return "Privacy";
        if (text.Contains("phish")) return "Phishing";
        if (text.Contains("malware") || text.Contains("virus")) return "Malware";
        if (text.Contains("network") || text.Contains("wifi")) return "Networking";
        if (text.Contains("backup") || text.Contains("update")) return "System";
        return "General";
    }
}

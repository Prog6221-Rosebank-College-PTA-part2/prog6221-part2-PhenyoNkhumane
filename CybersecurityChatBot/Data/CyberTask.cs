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
}

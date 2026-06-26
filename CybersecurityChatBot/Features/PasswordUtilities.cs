using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Provides simple password generation and strength analysis helpers for the chatbot.
/// </summary>
public static class PasswordUtilities
{
    private static readonly Random Random = new Random();

    public static string GeneratePassword(int length = 16)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*?-_";
        const string all = upper + lower + digits + symbols;

        var builder = new StringBuilder(length);
        builder.Append(upper[Random.Next(upper.Length)]);
        builder.Append(lower[Random.Next(lower.Length)]);
        builder.Append(digits[Random.Next(digits.Length)]);
        builder.Append(symbols[Random.Next(symbols.Length)]);

        for (int i = 4; i < length; i++)
        {
            builder.Append(all[Random.Next(all.Length)]);
        }

        return new string(builder.ToString().OrderBy(_ => Random.Next()).ToArray());
    }

    public static string CheckPasswordStrength(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "⚠ Please enter a password to check.";

        string trimmed = password.Trim();
        var issues = new List<string>();
        var score = 0;

        if (trimmed.Length >= 12)
            score += 2;
        else
            issues.Add("❌ Too short");

        if (Regex.IsMatch(trimmed, "[A-Z]"))
            score += 1;
        else
            issues.Add("❌ No uppercase letters");

        if (Regex.IsMatch(trimmed, "[a-z]"))
            score += 1;
        else
            issues.Add("❌ No lowercase letters");

        if (Regex.IsMatch(trimmed, "\\d"))
            score += 1;
        else
            issues.Add("❌ No numbers");

        if (Regex.IsMatch(trimmed, "[^A-Za-z0-9]"))
            score += 1;
        else
            issues.Add("❌ No symbols");

        string lower = trimmed.ToLowerInvariant();
        if (!ContainsCommonWord(lower))
            score += 1;
        else
            issues.Add("❌ Common word");

        string strength = score >= 6 ? "★★★★★" : score >= 4 ? "★★★★☆" : score >= 3 ? "★★★☆☆" : score >= 2 ? "★★☆☆☆" : "★☆☆☆☆";
        string label = score >= 6 ? "Strong" : score >= 4 ? "Good" : score >= 3 ? "Fair" : "Weak";

        var sb = new StringBuilder();
        sb.AppendLine($"Password Strength: {label} {strength}");
        if (issues.Count == 0)
        {
            sb.AppendLine("✅ No obvious issues found.");
        }
        else
        {
            sb.AppendLine("Problems:");
            foreach (string issue in issues)
                sb.AppendLine(issue);
        }

        sb.AppendLine();
        sb.AppendLine($"Suggestion: {SuggestImprovedPassword(trimmed)}");
        return sb.ToString().TrimEnd();
    }

    private static bool ContainsCommonWord(string password)
    {
        string[] common = { "password", "qwerty", "welcome", "letmein", "hello", "admin", "123456", "login" };
        return common.Any(word => password.Contains(word));
    }

    private static string SuggestImprovedPassword(string current)
    {
        if (string.IsNullOrWhiteSpace(current))
            return "Use a longer passphrase with upper/lowercase, numbers, and symbols.";

        return current.Length >= 12
            ? "Keep it unique and store it in a password manager."
            : "Try a longer passphrase such as 'Hello@2026Secure!'";
    }
}

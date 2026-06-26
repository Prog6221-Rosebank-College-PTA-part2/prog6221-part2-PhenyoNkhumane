using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Cybersecurity quiz mini-game with multiple-choice and true/false questions.
/// </summary>
public class QuizGame
{
    private readonly List<QuizQuestion> _questions;
    private int _currentIndex;
    private int _score;
    private bool _active;

    public bool IsActive => _active;
    public int CurrentQuestionNumber => _active ? _currentIndex + 1 : 0;
    public int TotalQuestions => _questions.Count;
    public int CurrentScore => _score;

    public QuizGame()
    {
        _questions = BuildQuestions();
    }

    public string Start()
    {
        _currentIndex = 0;
        _score = 0;
        _active = true;

        ActivityLog.Log("Quiz started.");
        return "🎮 Cybersecurity Quiz started!\n\n" + FormatCurrentQuestion();
    }

    public string? ProcessAnswer(string rawInput)
    {
        if (!_active)
            return null;

        int? choice = ParseAnswer(rawInput, _questions[_currentIndex]);
        if (!choice.HasValue)
        {
            return "Please answer with A, B, C, D, True, or False.\n\n" + FormatCurrentQuestion();
        }

        QuizQuestion question = _questions[_currentIndex];
        bool correct = choice.Value == question.CorrectIndex;

        if (correct)
            _score++;

        var sb = new StringBuilder();
        sb.AppendLine(correct
            ? "✅ Correct!"
            : $"❌ Not quite. The correct answer was {question.GetCorrectLabel()}.");
        sb.AppendLine($"💡 {question.Explanation}");

        _currentIndex++;

        if (_currentIndex >= _questions.Count)
        {
            _active = false;
            sb.AppendLine();
            sb.AppendLine(GetFinalSummary());
            ActivityLog.Log($"Quiz completed — score {_score}/{_questions.Count}.");
            return sb.ToString().TrimEnd();
        }

        sb.AppendLine();
        sb.AppendLine(FormatCurrentQuestion());
        return sb.ToString().TrimEnd();
    }

    public string FormatCurrentQuestion()
    {
        if (!_active || _currentIndex >= _questions.Count)
            return string.Empty;

        QuizQuestion q = _questions[_currentIndex];
        var sb = new StringBuilder();
        sb.AppendLine($"Question {_currentIndex + 1} of {_questions.Count} (Score: {_score})");
        sb.AppendLine();
        sb.AppendLine(q.Question);

        char label = 'A';
        foreach (string option in q.Options)
        {
            sb.AppendLine($"  {label}) {option}");
            label++;
        }

        return sb.ToString().TrimEnd();
    }

    private string GetFinalSummary()
    {
        double pct = (double)_score / _questions.Count * 100;
        string feedback = pct >= 80
            ? "Great job! You're a cybersecurity pro! 🏆"
            : pct >= 50
                ? "Good effort! Review the explanations and try again to sharpen your skills."
                : "Keep learning to stay safe online! Ask me about phishing, passwords, or 2FA anytime.";

        return $"Final score: {_score}/{_questions.Count} ({pct:0}%)\n{feedback}";
    }

    private static int? ParseAnswer(string input, QuizQuestion question)
    {
        string trimmed = input.Trim().ToUpperInvariant();

        if (question.IsTrueFalse)
        {
            if (trimmed is "TRUE" or "T" or "YES")
                return 0;
            if (trimmed is "FALSE" or "F" or "NO")
                return 1;
            return null;
        }

        Match letter = Regex.Match(trimmed, @"^[A-D]$");
        if (letter.Success)
            return letter.Value[0] - 'A';

        if (int.TryParse(trimmed, out int num) && num >= 1 && num <= question.Options.Count)
            return num - 1;

        return null;
    }

    private static List<QuizQuestion> BuildQuestions()
    {
        return new List<QuizQuestion>
        {
            new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                2,
                "Reporting phishing emails helps prevent scams and protects others."),

            new QuizQuestion(
                "True or False: Using the same password for every account is safe if it is very long.",
                new[] { "True", "False" },
                1,
                "Reusing passwords means one breach can compromise all your accounts. Use unique passwords.",
                isTrueFalse: true),

            new QuizQuestion(
                "Which is the strongest password?",
                new[] { "Password123", "P@ssw0rd!", "Xk9#mL2$vQ7!", "yourname2024" },
                2,
                "Strong passwords are long, random, and include mixed character types."),

            new QuizQuestion(
                "What is two-factor authentication (2FA)?",
                new[] { "Two passwords", "A second verification step after your password", "Logging in twice", "Using two email addresses" },
                1,
                "2FA adds a second layer of security, such as a code from an authenticator app."),

            new QuizQuestion(
                "True or False: Public Wi-Fi is always safe for online banking.",
                new[] { "True", "False" },
                1,
                "Public Wi-Fi can be intercepted. Avoid sensitive transactions or use a trusted VPN.",
                isTrueFalse: true),

            new QuizQuestion(
                "What is phishing?",
                new[] { "A type of malware", "Tricking users into revealing sensitive information", "Encrypting your files", "A firewall feature" },
                1,
                "Phishing uses fake messages or websites to steal credentials or personal data."),

            new QuizQuestion(
                "You get a USB drive labelled 'Confidential' in the parking lot. What should you do?",
                new[] { "Plug it in to see what's on it", "Turn it in to IT/security", "Share it with colleagues", "Take it home" },
                1,
                "Unknown USB devices can install malware. Never plug them into your computer."),

            new QuizQuestion(
                "True or False: Software updates often include important security patches.",
                new[] { "True", "False" },
                0,
                "Updates fix known vulnerabilities. Keeping software current is essential.",
                isTrueFalse: true),

            new QuizQuestion(
                "What is social engineering?",
                new[] { "Building social media apps", "Manipulating people to bypass security", "Engineering secure networks", "Creating strong passwords" },
                1,
                "Social engineering exploits human trust rather than technical flaws."),

            new QuizQuestion(
                "Which link is safest to click in an unexpected email?",
                new[] { "A shortened URL from an unknown sender", "A link that matches the official domain you know", "Any link in a urgent message", "A link asking you to 'verify your account'" },
                1,
                "Always verify URLs and sender identity before clicking links in emails."),

            new QuizQuestion(
                "What does HTTPS in a browser address bar indicate?",
                new[] { "The site is always trustworthy", "The connection to the site is encrypted", "The site is free of malware", "Your password is stored on the site" },
                1,
                "HTTPS encrypts data in transit, but you should still verify the site is legitimate."),

            new QuizQuestion(
                "True or False: Antivirus software alone guarantees you will never be hacked.",
                new[] { "True", "False" },
                1,
                "Security requires layered defences: updates, strong passwords, awareness, and backups.",
                isTrueFalse: true),
        };
    }
}

public class QuizQuestion
{
    public string Question { get; }
    public IReadOnlyList<string> Options { get; }
    public int CorrectIndex { get; }
    public string Explanation { get; }
    public bool IsTrueFalse { get; }

    public QuizQuestion(string question, string[] options, int correctIndex, string explanation, bool isTrueFalse = false)
    {
        Question = question;
        Options = options;
        CorrectIndex = correctIndex;
        Explanation = explanation;
        IsTrueFalse = isTrueFalse;
    }

    public string GetCorrectLabel()
    {
        if (IsTrueFalse)
            return CorrectIndex == 0 ? "True" : "False";

        return $"{(char)('A' + CorrectIndex)}) {Options[CorrectIndex]}";
    }
}

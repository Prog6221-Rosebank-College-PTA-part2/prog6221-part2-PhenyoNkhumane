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
    private int _correctAnswers;
    private int _wrongAnswers;
    private bool _quizStarted;
    private bool _quizFinished;

    public bool IsActive => _active;
    public int CurrentQuestionNumber => _active ? _currentIndex + 1 : 0;
    public int TotalQuestions => _questions.Count;
    public int CurrentScore => _score;
    public int CurrentQuestion => _active ? _currentIndex + 1 : 0;
    public int CorrectAnswers => _correctAnswers;
    public int WrongAnswers => _wrongAnswers;
    public bool QuizStarted => _quizStarted;
    public bool QuizFinished => _quizFinished;

    public QuizGame()
    {
        _questions = BuildQuestions();
    }

    public string Start()
    {
        // Step 2 — Initialise Variables
        _currentIndex = 0;
        _score = 0;
        _correctAnswers = 0;
        _wrongAnswers = 0;
        _quizFinished = false;
        _quizStarted = true;
        _active = true;

        ActivityLog.Log("Quiz Started");
        return FormatCurrentQuestion();
    }

    public string? ProcessAnswer(string rawInput)
    {
        if (!_active)
            return null;
        // Step 4 & 5 — Wait for input & Validate
        QuizQuestion question = _questions[_currentIndex];
        int? parsed = ParseAnswerStrict(rawInput, question);
        if (!parsed.HasValue)
        {
            return "⚠ Invalid answer.\nPlease answer using A, B, C, D, True, or False\n\n" + FormatCurrentQuestion();
        }

        bool correct = parsed.Value == question.CorrectIndex;
        var sb = new StringBuilder();

        if (correct)
        {
            // Step 6 — If correct
            _score++;
            _correctAnswers++;
            sb.AppendLine("✔ Correct!");
            sb.AppendLine(question.Explanation);
            sb.AppendLine($"+1 point");
            ActivityLog.Log($"Question {_currentIndex + 1} Correct");
        }
        else
        {
            // Step 6 — If wrong
            _wrongAnswers++;
            sb.AppendLine("❌ Incorrect.");
            sb.AppendLine($"The correct answer is {question.GetCorrectShortLabel()}");
            sb.AppendLine($"Why? {question.Explanation}");
            ActivityLog.Log($"Question {_currentIndex + 1} Incorrect");
        }

        // Step 8 — Next question automatically
        _currentIndex++;

        if (_currentIndex >= _questions.Count)
        {
            _active = false;
            _quizFinished = true;
            // Step 10 — Final results
            sb.AppendLine();
            sb.AppendLine(FormatFinalResults());
            // Step 12 — Save quiz result
            SaveQuizResult();
            ActivityLog.Log($"Quiz Completed — Score {_score}/{_questions.Count}");
            // Step 13 — Return to chat mode state
            _quizStarted = false;
            _currentIndex = 0;
            return sb.ToString().TrimEnd() + "\n\nYou're now back in normal chat mode. You can ask cybersecurity questions, manage tasks, or start another quiz whenever you like.";
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
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"Question {_currentIndex + 1} of {_questions.Count}    Current Score {_score}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(q.Question);

        char label = 'A';
        foreach (string option in q.Options)
        {
            sb.AppendLine($"{label}) {option}");
            label++;
        }

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
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

        // Legacy lenient parser — keep but prefer the strict parser used by the game loop.
        if (question.IsTrueFalse)
        {
            if (trimmed == "TRUE" || trimmed == "T")
                return 0;
            if (trimmed == "FALSE" || trimmed == "F")
                return 1;
            return null;
        }

        Match letter = Regex.Match(trimmed, "^[A-D]");
        if (letter.Success)
            return letter.Value[0] - 'A';

        return null;
    }

    // Strict parser that enforces the allowed inputs per spec
    private static int? ParseAnswerStrict(string input, QuizQuestion question)
    {
        string trimmed = input.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        string up = trimmed.ToUpperInvariant();

        // Accept only A,B,C,D for multiple choice
        if (!question.IsTrueFalse)
        {
            if (Regex.IsMatch(up, "^[A-D]$"))
                return up[0] - 'A';
            return null;
        }

        // True/False questions — accept only True/False (case-insensitive)
        if (up == "TRUE")
            return 0;
        if (up == "FALSE")
            return 1;
        return null;
    }

    private void SaveQuizResult()
    {
        try
        {
            string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "database");
            string path = System.IO.Path.Combine(folder, "quiz_results.txt");
            System.IO.Directory.CreateDirectory(folder);
            double pct = (double)_score / _questions.Count * 100;
            string line = $"QuizResult----------------------{DateTime.Now:yyyy-MM-dd HH:mm:ss} Score:{_score} Percentage:{pct:0}% Correct:{_correctAnswers}\\n";
            System.IO.File.AppendAllText(path, line);
        }
        catch
        {
            // non-fatal — ignore file write failures, activity log already records results
        }
    }

    private string FormatFinalResults()
    {
        int total = _questions.Count;
        double pct = (double)_score / total * 100;
        var sb = new StringBuilder();
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("🏆 Quiz Complete!");
        sb.AppendLine($"Final Score {_score} / {total}");
        sb.AppendLine($"Correct {_correctAnswers}");
        sb.AppendLine($"Incorrect {_wrongAnswers}");
        sb.AppendLine($"Percentage {pct:0}%");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━");

        if (pct >= 100)
            sb.AppendLine("🌟 Outstanding!\nYou're a Cybersecurity Expert.");
        else if (pct >= 80)
            sb.AppendLine("🎉 Excellent!\nYou have strong cybersecurity knowledge.");
        else if (pct >= 60)
            sb.AppendLine("👍 Good Job!\nYou understand the basics, but keep practising.");
        else if (pct >= 40)
            sb.AppendLine("📚 Fair Attempt.\nReview password safety, phishing and privacy.");
        else
            sb.AppendLine("💡 Keep Learning.\nTry the quiz again after reviewing the cybersecurity topics.");

        return sb.ToString().TrimEnd();
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

    public string GetCorrectShortLabel()
    {
        if (IsTrueFalse)
            return CorrectIndex == 0 ? "True" : "False";

        return $"{(char)('A' + CorrectIndex)}";
    }
}

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
    private bool _paused;
    private readonly Random _rng = new Random();
    private List<ActiveQuestion> _activeQuestions = new List<ActiveQuestion>();
    private bool _awaitingQuitConfirmation;
    private int _bestScore;
    private string _bestScorePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "database", "best_score.txt");

    public bool IsActive => _active;
    public bool IsPaused => _paused;
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
        _activeQuestions = new List<ActiveQuestion>(_questions.Count);
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
        _paused = false;

        ActivityLog.Log("Quiz Started");
        // Randomize questions and shuffle choices
        _activeQuestions.Clear();
        var shuffled = _questions.OrderBy(q => _rng.Next()).ToList();
        foreach (var q in shuffled)
            _activeQuestions.Add(ActiveQuestion.CreateFrom(q, _rng));

        // Load best score if exists
        try
        {
            var dir = System.IO.Path.GetDirectoryName(_bestScorePath) ?? ".";
            System.IO.Directory.CreateDirectory(dir);
            if (System.IO.File.Exists(_bestScorePath) && int.TryParse(System.IO.File.ReadAllText(_bestScorePath), out int bs))
                _bestScore = bs;
        }
        catch
        {
        }

        return FormatCurrentQuestion();
    }

    public string? ProcessAnswer(string rawInput)
    {
        if (!_active)
            return null;
        // Handle quit confirmation
        string trimmed = rawInput?.Trim() ?? string.Empty;
        if (_awaitingQuitConfirmation)
        {
            string up = trimmed.ToUpperInvariant();
            if (up == "Y" || up == "YES")
            {
                _active = false;
                _paused = true;
                _quizFinished = false;
                _awaitingQuitConfirmation = false;
                ActivityLog.Log("Quiz paused by user.");
                return "Quiz paused. Say 'resume quiz' when you're ready to continue.";
            }
            else
            {
                _awaitingQuitConfirmation = false;
                return "Quit cancelled.\n\n" + FormatCurrentQuestion();
            }
        }

        // Recognize quit command
        if (!string.IsNullOrEmpty(trimmed) && (string.Equals(trimmed, "quit quiz", StringComparison.OrdinalIgnoreCase) || string.Equals(trimmed, "quit", StringComparison.OrdinalIgnoreCase)))
        {
            _awaitingQuitConfirmation = true;
            return "Are you sure you want to quit the quiz? (Y/N)";
        }

        // Validate answer against active question
        if (_currentIndex < 0 || _currentIndex >= _activeQuestions.Count)
            return string.Empty;

        ActiveQuestion aq = _activeQuestions[_currentIndex];
        int? parsed = ParseAnswerStrict(trimmed, aq.BaseQuestion);
        if (!parsed.HasValue)
        {
            return "⚠ Invalid answer.\nPlease answer using A, B, C, D, True, or False\n\n" + FormatCurrentQuestion();
        }

        bool correct = parsed.Value == aq.ShuffledCorrectIndex;
        var sb = new StringBuilder();

        if (correct)
        {
            _score++;
            _correctAnswers++;
            sb.AppendLine("✔ Correct!");
            sb.AppendLine(aq.BaseQuestion.Explanation);
            sb.AppendLine("+1 point");
            ActivityLog.Log($"Question {_currentIndex + 1} Correct");
        }
        else
        {
            _wrongAnswers++;
            sb.AppendLine("❌ Incorrect.");
            sb.AppendLine($"The correct answer is {aq.BaseQuestion.GetCorrectShortLabel()}");
            sb.AppendLine($"Why? {aq.BaseQuestion.Explanation}");
            ActivityLog.Log($"Question {_currentIndex + 1} Incorrect");
        }

        // Encouraging message between questions
        if (_currentIndex + 1 < _activeQuestions.Count)
            sb.AppendLine().AppendLine($"Great work! Let's continue to Question {_currentIndex + 2}.");

        _currentIndex++;

        if (_currentIndex >= _activeQuestions.Count)
        {
            _active = false;
            _quizFinished = true;
            sb.AppendLine();
            sb.AppendLine(FormatFinalResults());
            SaveQuizResult();
            ActivityLog.Log($"Quiz Completed — Score {_score}/{_activeQuestions.Count}");
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
        if ((!_active && !_paused) || _currentIndex >= _activeQuestions.Count)
            return string.Empty;

        ActiveQuestion aq = _activeQuestions[_currentIndex];
        QuizQuestion q = aq.BaseQuestion;
        var sb = new StringBuilder();
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        int total = _activeQuestions.Count;
        int filled = (int)Math.Round((double)(_currentIndex + 1) / total * 10);
        filled = Math.Max(0, Math.Min(10, filled));
        string bar = new string('█', filled) + new string('░', 10 - filled);
        sb.AppendLine($"{bar} {_currentIndex + 1}/{total}");
        sb.AppendLine($"Category: {q.Category}");
        sb.AppendLine($"Question {_currentIndex + 1} of {total}    Current Score {_score}");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine(q.Question);

        char label = 'A';
        foreach (string option in aq.ShuffledOptions)
        {
            sb.AppendLine($"{label}) {option}");
            label++;
        }

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        return sb.ToString().TrimEnd();
    }

    public string Resume()
    {
        if (!_paused)
            return "There is no paused quiz to resume.";

        _active = true;
        _paused = false;
        ActivityLog.Log("Quiz resumed by user.");
        return FormatCurrentQuestion();
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
            int total = _activeQuestions != null && _activeQuestions.Count > 0 ? _activeQuestions.Count : _questions.Count;
            double pct = (double)_score / total * 100;
            string line = $"QuizResult----------------------{DateTime.Now:yyyy-MM-dd HH:mm:ss} Score:{_score} Percentage:{pct:0}% Correct:{_correctAnswers}\n";
            System.IO.File.AppendAllText(path, line);

            // update best score if beaten
            try
            {
                if (_score > _bestScore)
                {
                    System.IO.File.WriteAllText(_bestScorePath, _score.ToString());
                    _bestScore = _score;
                    ActivityLog.Log($"New best quiz score: {_bestScore}");
                }
            }
            catch
            {
            }
        }
        catch
        {
            // non-fatal — ignore file write failures, activity log already records results
        }
    }

    private record ActiveQuestion(QuizQuestion BaseQuestion, List<string> ShuffledOptions, int ShuffledCorrectIndex)
    {
        public static ActiveQuestion CreateFrom(QuizQuestion q, Random rng)
        {
            var opts = q.Options.ToList();
            // shuffle options
            for (int i = opts.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = opts[i];
                opts[i] = opts[j];
                opts[j] = tmp;
            }

            // find new index of correct answer
            string correct = q.Options[q.CorrectIndex];
            int newIndex = opts.IndexOf(correct);

            return new ActiveQuestion(q, opts, newIndex);
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
                "Reporting phishing emails helps prevent scams and protects others.",
                category: "Phishing"),

            new QuizQuestion(
                "True or False: Using the same password for every account is safe if it is very long.",
                new[] { "True", "False" },
                1,
                "Reusing passwords means one breach can compromise all your accounts. Use unique passwords.",
                isTrueFalse: true,
                category: "Passwords"),

            new QuizQuestion(
                "Which is the strongest password?",
                new[] { "Password123", "P@ssw0rd!", "Xk9#mL2$vQ7!", "yourname2024" },
                2,
                "Strong passwords are long, random, and include mixed character types.",
                category: "Passwords"),

            new QuizQuestion(
                "What is two-factor authentication (2FA)?",
                new[] { "Two passwords", "A second verification step after your password", "Logging in twice", "Using two email addresses" },
                1,
                "2FA adds a second layer of security, such as a code from an authenticator app.",
                category: "Authentication"),

            new QuizQuestion(
                "True or False: Public Wi-Fi is always safe for online banking.",
                new[] { "True", "False" },
                1,
                "Public Wi-Fi can be intercepted. Avoid sensitive transactions or use a trusted VPN.",
                isTrueFalse: true,
                category: "Privacy"),

            new QuizQuestion(
                "What is phishing?",
                new[] { "A type of malware", "Tricking users into revealing sensitive information", "Encrypting your files", "A firewall feature" },
                1,
                "Phishing uses fake messages or websites to steal credentials or personal data.",
                category: "Phishing"),

            new QuizQuestion(
                "You get a USB drive labelled 'Confidential' in the parking lot. What should you do?",
                new[] { "Plug it in to see what's on it", "Turn it in to IT/security", "Share it with colleagues", "Take it home" },
                1,
                "Unknown USB devices can install malware. Never plug them into your computer.",
                category: "Malware"),

            new QuizQuestion(
                "True or False: Software updates often include important security patches.",
                new[] { "True", "False" },
                0,
                "Updates fix known vulnerabilities. Keeping software current is essential.",
                isTrueFalse: true,
                category: "Updates"),

            new QuizQuestion(
                "What is social engineering?",
                new[] { "Building social media apps", "Manipulating people to bypass security", "Engineering secure networks", "Creating strong passwords" },
                1,
                "Social engineering exploits human trust rather than technical flaws.",
                category: "Social Engineering"),

            new QuizQuestion(
                "Which link is safest to click in an unexpected email?",
                new[] { "A shortened URL from an unknown sender", "A link that matches the official domain you know", "Any link in a urgent message", "A link asking you to 'verify your account'" },
                1,
                "Always verify URLs and sender identity before clicking links in emails.",
                category: "Phishing"),

            new QuizQuestion(
                "What does HTTPS in a browser address bar indicate?",
                new[] { "The site is always trustworthy", "The connection to the site is encrypted", "The site is free of malware", "Your password is stored on the site" },
                1,
                "HTTPS encrypts data in transit, but you should still verify the site is legitimate.",
                category: "Privacy"),

            new QuizQuestion(
                "True or False: Antivirus software alone guarantees you will never be hacked.",
                new[] { "True", "False" },
                1,
                "Security requires layered defences: updates, strong passwords, awareness, and backups.",
                isTrueFalse: true,
                category: "Security"),
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
    public string Category { get; }

    public QuizQuestion(string question, string[] options, int correctIndex, string explanation, bool isTrueFalse = false, string category = "General")
    {
        Question = question;
        Options = options;
        CorrectIndex = correctIndex;
        Explanation = explanation;
        IsTrueFalse = isTrueFalse;
        Category = category;
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

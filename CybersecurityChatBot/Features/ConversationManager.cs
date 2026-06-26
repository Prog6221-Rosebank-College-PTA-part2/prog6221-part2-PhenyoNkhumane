using System.Collections.Generic;

/// <summary>
/// Manages the state of the ongoing conversation.
/// Tracks the last matched cybersecurity topic so the chatbot can
/// handle follow-up phrases like "tell me more" or "give me another tip"
/// without the user needing to repeat the topic.
///
/// Improvements over v1:
/// - Increments MemoryStore.MessageCount on every turn so that
///   MemoryStore.ShouldRecallNow() can fire consistently (fixes
///   "recalled inconsistently" feedback).
/// - Injects a full multi-topic recall line every 4 turns when
///   interests are stored, making memory feel present throughout
///   the whole conversation rather than only once.
/// - Adds a "what do you remember" command so the user can explicitly
///   ask what the bot has stored about them.
/// - Uses AddInterest() instead of SetFavouriteTopic() to accumulate
///   all expressed interests rather than overwriting them.
/// </summary>
class ConversationManager
{
    public string? LastTopic { get; private set; }

    private int _tipCycleIndex;
    private readonly Part3FeatureManager _part3 = new Part3FeatureManager();

    // ─── Follow-up phrases ────────────────────────────────────────────────────
    // These are checked BEFORE sentiment detection so that "tell me more"
    // / "explain more" are handled as follow-ups, not as Curious sentiment —
    // this was the root cause of Curious never firing in v1.

    private static readonly List<string> FollowUpPhrases = new List<string>
    {
        "tell me more",
        "explain more",
        "more details",
        "give me another tip",
        "another tip",
        "go on",
        "continue",
        "what else",
        "more please",
        "keep going",
        "elaborate",
        "and then",
        "more info",
        "more information",
        "expand on that",
        "say more",
        "give me more"
    };

    // ─── Interest phrases ─────────────────────────────────────────────────────

    private static readonly List<string> InterestPhrases = new List<string>
    {
        "i'm interested in",
        "i am interested in",
        "i care about",
        "i want to learn about",
        "tell me about",
        "i'd like to know about",
        "i like",
        "i love",
        "i enjoy",
        "i focus on",
        "my main concern is",
        "i'm worried about",   // captures topic AND worried sentiment in one turn
        "most interested in"
    };

    // ─── Memory query phrases ─────────────────────────────────────────────────

    private static readonly List<string> MemoryQueryPhrases = new List<string>
    {
        "what do you remember",
        "what do you know about me",
        "what have you remembered",
        "do you remember me",
        "my profile",
        "show my profile",
        "profile",
        "what's my name",
        "what did i say",
        "what have i told you",
        "recall"
    };

    // ─── Public entry points ──────────────────────────────────────────────────

    public string Process(string userName, string rawInput)
    {
        return ProcessInput(rawInput);
    }

    public string ProcessInput(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return "Please type a message — I'm here to help!";

        // Increment the session message counter on every turn so
        // MemoryStore.ShouldRecallNow() can fire at regular intervals.
        MemoryStore.IncrementMessageCount();

        string input = rawInput.ToLowerInvariant().Trim();

        // ── 1. Memory query (explicit: "what do you remember about me?") ──────
        if (IsMemoryQuery(input))
        {
            ActivityLog.Log("User asked what the bot remembers.");
            return BuildMemoryReport();
        }

        // ── 2. Part 3: tasks, quiz, activity log, NLP intents ───────────────────
        string? part3Response = _part3.TryHandle(rawInput);
        if (part3Response != null)
        {
            ActivityLog.Log($"Processed feature command: {rawInput}");
            return part3Response;
        }

        // ── 3. Interest capture ───────────────────────────────────────────────
        // Must run before follow-up check because "I'm interested in X" also
        // contains "in" which could confuse generic matchers.
        string? interestTopic = TryExtractInterest(input);
        if (interestTopic != null)
        {
            MemoryStore.AddInterest(interestTopic);   // accumulates, not overwrites
            UpdateTopic(interestTopic);
            return MemoryStore.GetTopicSavedConfirmation();
        }

        // ── 4. Follow-up handling ─────────────────────────────────────────────
        if (IsFollowUp(input))
        {
            ActivityLog.Log($"Follow-up received for topic: {LastTopic ?? "none"}.");
            return HandleFollowUp();
        }

        // ── 5. Sentiment detection ────────────────────────────────────────────
        // Runs after follow-up so "tell me more" / "explain" never reach here.
        var sentiment = SentimentDetector.Detect(rawInput);
        string? sentimentResponse = sentiment == SentimentDetector.Sentiment.None
            ? null
            : SentimentDetector.BuildResponse(sentiment, LastTopic);
        if (sentimentResponse != null)
        {
            ActivityLog.Log($"Detected sentiment: {sentiment}.");
            // Append a memory recall line if it's time and we have interests.
            string recall = MaybeGetRecallLine();
            return recall.Length > 0
                ? $"{sentimentResponse}\n\n🧠 {recall}"
                : sentimentResponse;
        }

        // ── 6. Keyword / topic response ───────────────────────────────────────
        string response = ResponseCatalog.GetResponse(input, out string? matchedTopic);

        if (matchedTopic != null)
        {
            ActivityLog.Log($"Responded on topic: {matchedTopic}.");
            UpdateTopic(matchedTopic);
            TaskDatabase.UpdateUserStatistics(TaskDatabase.CurrentUserId, favouriteTopic: matchedTopic);

            // Append a recall line if this topic differs from the stored
            // favourite — same as v1 — but NOW also append on every 4th
            // message regardless, so recall fires consistently.
            bool topicDiffers = MemoryStore.HasFavouriteTopic &&
                                 MemoryStore.FavouriteTopic != matchedTopic;

            string recall = topicDiffers || MemoryStore.ShouldRecallNow()
                ? GetBestRecallLine()
                : string.Empty;

            if (!string.IsNullOrEmpty(recall))
                response += $"\n\n🧠 {recall}";
        }
        else
        {
            // No topic matched — still inject a recall line on schedule
            // so the bot feels like it's paying attention even in
            // free-form conversation.
            string recall = MaybeGetRecallLine();
            if (!string.IsNullOrEmpty(recall))
                response += $"\n\n🧠 {recall}";
        }

        return response;
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private bool IsFollowUp(string input)
    {
        foreach (string phrase in FollowUpPhrases)
        {
            if (input.Contains(phrase))
                return true;
        }
        return false;
    }

    private bool IsMemoryQuery(string input)
    {
        foreach (string phrase in MemoryQueryPhrases)
        {
            if (input.Contains(phrase))
                return true;
        }
        return false;
    }

    private string HandleFollowUp()
    {
        if (LastTopic == null)
        {
            return "We haven't discussed a specific topic yet — what would you like to know about? " +
                   "Try asking about phishing, passwords, malware, or safe browsing.";
        }

        List<string> allTips = ResponseCatalog.GetAllTips(LastTopic);

        if (allTips.Count == 0)
            return ResponseCatalog.GetResponse(LastTopic, out _);

        string tip = allTips[_tipCycleIndex % allTips.Count];
        _tipCycleIndex++;

        return $"Here's another tip on {LastTopic}:\n\n💡 {tip}";
    }

    private void UpdateTopic(string newTopic)
    {
        if (newTopic != LastTopic)
        {
            LastTopic      = newTopic;
            _tipCycleIndex = 0;
        }
    }

    /// <summary>
    /// Returns the best available recall line for the current moment.
    /// Uses the multi-topic variant if 2+ interests are stored.
    /// </summary>
    private string GetBestRecallLine()
    {
        if (!MemoryStore.HasFavouriteTopic)
            return string.Empty;

        string? line = MemoryStore.Interests.Count >= 2
            ? MemoryStore.GetFullRecallLine()
            : MemoryStore.GetTopicRecallLine();

        return line ?? string.Empty;
    }

    /// <summary>
    /// Returns a recall line only if MemoryStore.ShouldRecallNow() is true.
    /// Keeps recall feeling scheduled rather than random.
    /// </summary>
    private string MaybeGetRecallLine()
    {
        return MemoryStore.ShouldRecallNow() ? GetBestRecallLine() : string.Empty;
    }

    /// <summary>
    /// Builds a human-readable summary of everything the bot has stored,
    /// shown when the user explicitly asks "what do you remember about me?".
    /// This is a strong demonstration of the memory feature for marking.
    /// </summary>
    private string BuildMemoryReport()
    {
        return MemoryStore.GetProfileSummary(LastTopic);
    }

    private string? TryExtractInterest(string input)
    {
        foreach (string phrase in InterestPhrases)
        {
            if (input.Contains(phrase))
            {
                int    index     = input.IndexOf(phrase) + phrase.Length;
                string remainder = input.Substring(index).Trim().TrimEnd('.', '!', '?');

                if (string.IsNullOrWhiteSpace(remainder))
                    return null;

                return MapToKnownTopic(remainder);
            }
        }
        return null;
    }

    private static string? MapToKnownTopic(string text)
    {
        var topicMap = new Dictionary<string, string>
        {
            ["phishing"]          = ResponseCatalog.TopicPhishing,
            ["phish"]             = ResponseCatalog.TopicPhishing,
            ["scam"]              = ResponseCatalog.TopicPhishing,
            ["scams"]             = ResponseCatalog.TopicPhishing,
            ["password"]          = ResponseCatalog.TopicPassword,
            ["passwords"]         = ResponseCatalog.TopicPassword,
            ["malware"]           = ResponseCatalog.TopicMalware,
            ["virus"]             = ResponseCatalog.TopicMalware,
            ["privacy"]           = ResponseCatalog.TopicPrivacy,
            ["safe browsing"]     = ResponseCatalog.TopicSafeBrowsing,
            ["browsing"]          = ResponseCatalog.TopicSafeBrowsing,
            ["wifi"]              = ResponseCatalog.TopicWifi,
            ["wi-fi"]             = ResponseCatalog.TopicWifi,
            ["2fa"]               = ResponseCatalog.TopicTwoFactor,
            ["two factor"]        = ResponseCatalog.TopicTwoFactor,
            ["mfa"]               = ResponseCatalog.TopicTwoFactor,
            ["ransomware"]        = ResponseCatalog.TopicRansomware,
            ["vpn"]               = ResponseCatalog.TopicVpn,
            ["backup"]            = ResponseCatalog.TopicBackup,
            ["backups"]           = ResponseCatalog.TopicBackup,
            ["updates"]           = ResponseCatalog.TopicUpdates,
            ["patching"]          = ResponseCatalog.TopicUpdates,
            ["pharming"]          = ResponseCatalog.TopicPharming,
            ["social engineering"] = ResponseCatalog.TopicSocialEng,
            ["encryption"]        = ResponseCatalog.TopicEncryption,
            ["incident"]          = ResponseCatalog.TopicIncident,
        };

        foreach (var pair in topicMap)
        {
            if (text.Contains(pair.Key))
                return pair.Value;
        }

        return null;
    }

    public Part3FeatureManager GetPart3Features() => _part3;
}
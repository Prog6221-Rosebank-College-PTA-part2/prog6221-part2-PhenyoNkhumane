using System;
using System.Collections.Generic;

/// <summary>
/// Detects emotional sentiment in the user's input and produces an
/// empathetic response. When a sentiment is detected, the chatbot
/// automatically appends a relevant cybersecurity tip so the user
/// does not need to ask a follow-up question.
///
/// Improvements over v1:
/// - Each sentiment now has multiple empathy response variants so
///   repeated detections feel natural, not robotic (fixes "limited
///   response variation" marker feedback).
/// - Curious keywords no longer overlap with follow-up phrases —
///   "tell me more" / "explain" are handled by ConversationManager
///   as follow-ups first; Curious is detected only for genuinely
///   inquisitive phrasing that isn't a follow-up.
/// - Overwhelmed and Confident get full multi-variant treatment.
/// </summary>
public static class SentimentDetector
{
    // ─── Sentiment enum ───────────────────────────────────────────────────────

    public enum Sentiment
    {
        None,
        Worried,
        Frustrated,
        Curious,
        Overwhelmed,
        Confident
    }

    // ─── Keyword tables ───────────────────────────────────────────────────────

    /// <summary>
    /// Keywords are checked in enum declaration order; the first match wins.
    /// NOTE: "tell me more", "explain", "what is" are intentionally ABSENT
    /// here — those are follow-up phrases handled by ConversationManager
    /// before sentiment detection is reached, so Curious now fires correctly.
    /// </summary>
    private static readonly Dictionary<Sentiment, string[]> SentimentKeywords =
        new Dictionary<Sentiment, string[]>
        {
            [Sentiment.Worried] = new[]
            {
                "worried", "scared", "afraid", "anxious", "nervous",
                "fear", "frightened", "unsafe", "not safe", "concerned",
                "at risk", "vulnerable", "terrified", "panicking"
            },

            [Sentiment.Frustrated] = new[]
            {
                "frustrated", "annoyed", "angry", "useless",
                "not helpful", "don't understand", "lost",
                "makes no sense", "too complicated", "difficult",
                "hate this", "this is hard", "so hard"
            },

            [Sentiment.Curious] = new[]
            {
                // Genuinely inquisitive — not follow-up rephrasings
                "curious", "i'm curious", "i am curious",
                "want to know", "want to learn", "keen to learn",
                "interested to know", "how does it work",
                "wondering about", "can you explain why",
                "i wonder", "fascinated"
            },

            [Sentiment.Overwhelmed] = new[]
            {
                "overwhelmed", "too much", "so much to remember",
                "can't keep up", "don't know where to start",
                "a lot to remember", "hard to keep track",
                "drowning in", "information overload"
            },

            [Sentiment.Confident] = new[]
            {
                "confident", "i know this", "got it", "understood",
                "makes sense", "feel safe", "all good", "no problem",
                "i already do this", "i've got this", "ready for this"
            }
        };

    // ─── Multi-variant empathy responses ──────────────────────────────────────

    /// <summary>
    /// Each sentiment has a list of empathy openers. The variant is chosen
    /// by hashing the user's input length, giving consistent but varied
    /// replies without needing a stateful counter.
    /// </summary>
    private static readonly Dictionary<Sentiment, string[]> EmpathyVariants =
        new Dictionary<Sentiment, string[]>
        {
            [Sentiment.Worried] = new[]
            {
                "It's completely understandable to feel that way — " +
                "cyber threats can be unsettling. Here's a tip to help you feel more in control:",

                "Your concern shows you take this seriously, and that's already a great start. " +
                "Let me share something practical:",

                "Feeling worried about online safety is very common, {name}. " +
                "The good news is there are clear steps you can take. Here's one:",

                "I hear you — the online world can feel scary at times. " +
                "Let me give you something concrete to act on right now:"
            },

            [Sentiment.Frustrated] = new[]
            {
                "I hear you — cybersecurity can feel overwhelming at first. " +
                "Let me try to make this clearer with a straightforward tip:",

                "Totally fair, {name}. Some of this stuff is genuinely confusing. " +
                "Let's simplify it with one thing to focus on:",

                "Sorry this has been tricky! Let me break it down differently:",

                "Frustration usually means we need a different angle. " +
                "Here's the simplest version of what you need to know:"
            },

            [Sentiment.Curious] = new[]
            {
                "I love the curiosity, {name}! " +
                "That's exactly the right mindset for staying safe online. Here's something useful:",

                "Great question energy! Curiosity is your best cybersecurity tool. " +
                "Here's something worth knowing:",

                "That inquisitive mindset will serve you well, {name}. " +
                "Here's a fact that might surprise you:",

                "Brilliant — the more you want to learn, the safer you'll be. " +
                "Here's a nugget to add to your knowledge:"
            },

            [Sentiment.Overwhelmed] = new[]
            {
                "It's okay, {name} — you don't need to learn everything at once. " +
                "Let's focus on just one simple thing you can do right now:",

                "Take a breath! Cybersecurity is a journey, not a test. " +
                "Here's the single most impactful thing to start with:",

                "You're not alone in feeling this way. " +
                "Let me give you one small, manageable action to take today:",

                "Small steps are totally valid, {name}. " +
                "Here's one habit that will make an immediate difference:"
            },

            [Sentiment.Confident] = new[]
            {
                "That's great to hear! Confidence in cybersecurity goes a long way. " +
                "Here's something extra to keep you sharp:",

                "Love that energy, {name}! " +
                "Here's an advanced tip for someone who already has the basics covered:",

                "Excellent — you're already ahead of most people. " +
                "Here's something to level up even further:",

                "Perfect mindset! Now let's make sure that confidence is backed by this tip:"
            }
        };

    // ─── Default topic per sentiment ──────────────────────────────────────────

    private static readonly Dictionary<Sentiment, string> DefaultTopicForSentiment =
        new Dictionary<Sentiment, string>
        {
            [Sentiment.Worried]     = ResponseCatalog.TopicPhishing,
            [Sentiment.Frustrated]  = ResponseCatalog.TopicPassword,
            [Sentiment.Curious]     = ResponseCatalog.TopicSafeBrowsing,
            [Sentiment.Overwhelmed] = ResponseCatalog.TopicBackup,
            [Sentiment.Confident]   = ResponseCatalog.TopicTwoFactor
        };

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Scans the user's input and returns the detected sentiment.
    /// Returns Sentiment.None if no emotional keywords are found.
    /// </summary>
    public static Sentiment Detect(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return Sentiment.None;

        string input = userInput.ToLowerInvariant();

        foreach (var pair in SentimentKeywords)
        {
            foreach (string keyword in pair.Value)
            {
                if (input.Contains(keyword))
                    return pair.Key;
            }
        }

        return Sentiment.None;
    }

    /// <summary>
    /// Builds a full empathetic response for a detected sentiment.
    /// Combines a varied empathy line (personalised with the user's name)
    /// with an automatic tip on the most relevant topic.
    /// Returns null if sentiment is None.
    /// </summary>
    public static string? BuildResponse(Sentiment sentiment, string? preferredTopic = null)
    {
        if (sentiment == Sentiment.None)
            return null;

        // Pick tip topic — prefer the user's current conversation topic.
        string topic = !string.IsNullOrEmpty(preferredTopic) && ResponseCatalog.HasRandomTips(preferredTopic)
            ? preferredTopic
            : DefaultTopicForSentiment[sentiment];

        // Select empathy variant based on input length for natural variety.
        string[] variants  = EmpathyVariants[sentiment];
        int      variantIdx = Math.Abs(MemoryStore.MessageCount % variants.Length);
        string   empathy   = variants[variantIdx]
            .Replace("{name}", string.IsNullOrEmpty(MemoryStore.UserName)
                ? "friend"
                : MemoryStore.UserName);

        // Fetch tip — fall back to standard response if no random tips exist.
        string? tip = ResponseCatalog.GetRandomTip(topic)
                   ?? ResponseCatalog.GetResponse(topic, out _);

        return $"{empathy}\n\n💡 {tip}";
    }

    /// <summary>
    /// Convenience: detects sentiment and builds the full response in one call.
    /// Returns null if no sentiment is detected.
    /// </summary>
    public static string? DetectAndRespond(string userInput, string? preferredTopic = null)
    {
        Sentiment sentiment = Detect(userInput);
        return sentiment == Sentiment.None ? null : BuildResponse(sentiment, preferredTopic);
    }
}
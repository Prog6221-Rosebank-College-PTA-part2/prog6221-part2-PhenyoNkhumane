using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stores user-specific information gathered during the conversation.
/// Tracks the user's name, all expressed topic interests, and message
/// count so the chatbot can personalise responses consistently throughout
/// the session — addressing the "recalled inconsistently" marker feedback.
/// </summary>
public static class MemoryStore
{
    // ─── User identity ────────────────────────────────────────────────────────

    /// <summary>The user's name, captured at the start of the session.</summary>
    public static string UserName { get; private set; } = string.Empty;

    // ─── Topic memory ─────────────────────────────────────────────────────────

    /// <summary>
    /// All topics the user has expressed interest in, in the order they
    /// were first mentioned. Using a List preserves insertion order and
    /// allows duplicate-free additions via AddInterest().
    /// </summary>
    private static readonly List<string> _interests = new List<string>();

    /// <summary>Read-only view of all stored interests.</summary>
    public static IReadOnlyList<string> Interests => _interests.AsReadOnly();

    /// <summary>
    /// The most recently expressed interest — used as the "primary" topic
    /// for personalised recall lines mid-conversation.
    /// </summary>
    public static string? FavouriteTopic => _interests.Count > 0
        ? _interests[_interests.Count - 1]
        : null;

    /// <summary>True when at least one interest has been stored.</summary>
    public static bool HasFavouriteTopic => _interests.Count > 0;

    // ─── Conversation counter ─────────────────────────────────────────────────

    /// <summary>
    /// Number of user messages processed this session.
    /// Used to decide when to inject unsolicited recall lines so the
    /// chatbot references stored topics at natural intervals (every 4 turns).
    /// </summary>
    public static int MessageCount { get; private set; }

    /// <summary>Increments the message counter. Called by ConversationManager.</summary>
    public static void IncrementMessageCount() => MessageCount++;

    // ─── Setters ──────────────────────────────────────────────────────────────

    /// <summary>Saves the user's name for use in personalised responses.</summary>
    public static void SetUserName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            UserName = name.Trim();
    }

    /// <summary>
    /// Adds a cybersecurity topic to the user's interest list.
    /// Silently ignores duplicates and blank values.
    /// Replaces the old single-topic SetFavouriteTopic() so that
    /// every expressed interest is retained, not just the last one.
    /// </summary>
    public static void AddInterest(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return;

        if (!_interests.Contains(topic))
            _interests.Add(topic);
    }

    /// <summary>
    /// Backward-compat alias used by ConversationManager.
    /// Delegates to AddInterest so no existing call sites need changing.
    /// </summary>
    public static void SetFavouriteTopic(string topic) => AddInterest(topic);

    // ─── Recall lines ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a personalised greeting using the stored name.
    /// Falls back gracefully if no name has been set yet.
    /// </summary>
    public static string GetPersonalisedGreeting()
    {
        return string.IsNullOrEmpty(UserName)
            ? "Welcome! What would you like to know about cybersecurity?"
            : $"Welcome, {UserName}! What would you like to know about cybersecurity today?";
    }

    /// <summary>
    /// Returns a personalised recall line referencing the user's most recent
    /// interest. Rotates through a set of recall templates so repeated
    /// mentions feel natural rather than robotic — fixing the "inconsistent"
    /// marker feedback.
    /// Returns null when no interest has been stored.
    /// </summary>
    public static string? GetTopicRecallLine()
    {
        if (!HasFavouriteTopic)
            return null;

        string topic = FavouriteTopic!;

        // Rotate through multiple recall phrasings based on how many
        // interests have been stored so far, keeping variety high.
        int variant = _interests.Count % 4;

        return variant switch
        {
            0 => $"As someone interested in {topic}, you might also want to " +
                 "review the security settings on your most-used accounts.",

            1 => $"Since you mentioned {topic} earlier, here's a related reminder: " +
                 "small habits — like regular check-ins — make a big difference.",

            2 => $"Keeping your interest in {topic} in mind, {UserName}: " +
                 "staying informed is one of the best defences you have.",

            _ => $"I remember you care about {topic}, {UserName}. " +
                 "That awareness already puts you ahead of most users!"
        };
    }

    /// <summary>
    /// Returns a recall line that references ALL stored interests, used
    /// when the user has mentioned three or more topics. Gives a sense that
    /// the bot has been paying attention throughout the whole conversation.
    /// </summary>
    public static string? GetFullRecallLine()
    {
        if (_interests.Count < 2)
            return GetTopicRecallLine();

        string joined = string.Join(", ", _interests.Take(_interests.Count - 1)) +
                        " and " + _interests[_interests.Count - 1];

        return $"You've shown interest in {joined} today, {UserName} — " +
               "that's a great broad foundation for staying safe online!";
    }

    /// <summary>
    /// Returns true if the chatbot should inject a recall line at this point
    /// in the conversation. Fires on every 4th user message when at least one
    /// interest has been stored, so recall feels consistent without being
    /// intrusive.
    /// </summary>
    public static bool ShouldRecallNow()
    {
        return HasFavouriteTopic && MessageCount > 0 && MessageCount % 4 == 0;
    }

    /// <summary>
    /// Builds a confirmation message shown when a topic is first stored.
    /// Example: "Great! I'll remember that you're interested in privacy."
    /// </summary>
    public static string GetTopicSavedConfirmation()
    {
        if (!HasFavouriteTopic)
            return string.Empty;

        string topic = FavouriteTopic!;

        // If this is the second or later interest, acknowledge the history.
        if (_interests.Count > 1)
        {
            string prev = _interests[_interests.Count - 2];
            return $"Got it, {UserName}! I'll add {topic} to your interests — " +
                   $"alongside {prev} that you mentioned before. " +
                   "Building broad awareness is a smart move!";
        }

        return $"Great! I'll remember that you're interested in {topic}, {UserName}. " +
               "It's a crucial part of staying safe online.";
    }

    /// <summary>Clears all stored memory for a fresh session.</summary>
    public static void Reset()
    {
        UserName    = string.Empty;
        MessageCount = 0;
        _interests.Clear();
    }
}
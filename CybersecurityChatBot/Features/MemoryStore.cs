/// <summary>
/// Stores user-specific information gathered during the conversation.
/// Allows the chatbot to personalise responses using the user's name
/// and remembered cybersecurity interest throughout the session.
/// </summary>
public static class MemoryStore
{
    /// <summary>The user's name, captured at the start of the session.</summary>
    public static string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// The cybersecurity topic the user expressed interest in.
    /// Set when the user says something like "I'm interested in privacy."
    /// </summary>
    public static string? FavouriteTopic { get; private set; }

    /// <summary>
    /// Whether a favourite topic has been stored for this session.
    /// </summary>
    public static bool HasFavouriteTopic => !string.IsNullOrEmpty(FavouriteTopic);

    /// <summary>Saves the user's name for use in personalised responses.</summary>
    public static void SetUserName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            UserName = name.Trim();
        }
    }

    /// <summary>
    /// Saves the user's favourite cybersecurity topic.
    /// Only updates if the topic is a known, non-null topic string.
    /// </summary>
    public static void SetFavouriteTopic(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic))
        {
            FavouriteTopic = topic;
        }
    }

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
    /// Returns a personalised recall line referencing the user's favourite topic.
    /// Called mid-conversation to remind the user that their interest was remembered.
    /// Returns null if no favourite topic has been stored.
    /// </summary>
    public static string? GetTopicRecallLine()
    {
        if (!HasFavouriteTopic)
        {
            return null;
        }

        return $"As someone interested in {FavouriteTopic}, you might also want to " +
               "review the security settings on your most-used accounts.";
    }

    /// <summary>
    /// Builds a confirmation message to display when a favourite topic is first saved.
    /// Example: "Great! I'll remember that you're interested in privacy."
    /// </summary>
    public static string GetTopicSavedConfirmation()
    {
        if (!HasFavouriteTopic)
        {
            return string.Empty;
        }

        return $"Great! I'll remember that you're interested in {FavouriteTopic}. " +
               "It's a crucial part of staying safe online.";
    }

    /// <summary>Clears all stored memory for a fresh session.</summary>
    public static void Reset()
    {
        UserName = string.Empty;
        FavouriteTopic = null;
    }
}

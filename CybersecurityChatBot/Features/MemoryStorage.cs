public static class MemoryStore
{
    public static string UserName { get; private set; } = string.Empty;
    public static string? FavouriteTopic { get; private set; }
    public static bool HasFavouriteTopic => !string.IsNullOrEmpty(FavouriteTopic);
    public static void SetUserName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            UserName = name.Trim();
        }
    }

    public static void SetFavouriteTopic(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic))
        {
            FavouriteTopic = topic;
        }
    }

    public static string GetPersonalisedGreeting()
    {
        return string.IsNullOrEmpty(UserName)
            ? "Welcome! What would you like to know about cybersecurity?"
            : $"Welcome, {UserName}! What would you like to know about cybersecurity today?";
    }

    public static string? GetTopicRecallLine()
    {
        if (!HasFavouriteTopic)
        {
            return null;
        }

        return $"As someone interested in {FavouriteTopic}, you might also want to " +
               "review the security settings on your most-used accounts.";
    }
    public static string GetTopicSavedConfirmation()
    {
        if (!HasFavouriteTopic)
        {
            return string.Empty;
        }

        return $"Great! I'll remember that you're interested in {FavouriteTopic}. " +
               "It's a crucial part of staying safe online.";
    }
    public static void Reset()
    {
        UserName = string.Empty;
        FavouriteTopic = null;
    }
}

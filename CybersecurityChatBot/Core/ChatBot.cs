/// <summary>
/// Core chatbot controller for the WPF application.
///
/// All console I/O has been removed. This class now acts as a pure
/// logic layer — it receives strings and returns strings, leaving all
/// display work to MainWindow.xaml and MainWindow.xaml.cs.
///
/// Holds one ConversationManager instance per session so topic
/// tracking and follow-up handling persist across messages.
/// </summary>
public class ChatBot
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    /// <summary>
    /// One ConversationManager per ChatBot instance.
    /// Tracks LastTopic and tip cycle index across the full session.
    /// </summary>
    private readonly ConversationManager _conversation = new ConversationManager();

    /// <summary>
    /// Whether the user has been welcomed yet.
    /// Used by MainWindow to decide whether to show the name prompt.
    /// </summary>
    public bool SessionStarted { get; private set; } = false;

    // -------------------------------------------------------------------------
    // Session setup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates and stores the user's name.
    /// Called once when the user submits their name in the name prompt.
    ///
    /// Returns an error message string if the name is invalid,
    /// or null if the name was accepted successfully.
    /// </summary>
    public string? SubmitName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return "Name cannot be empty — please enter your name to continue.";

        MemoryStore.SetUserName(rawName.Trim());
        SessionStarted = true;
        return null; // null = success, no error
    }

    /// <summary>
    /// Returns the opening welcome message to display in the chat area
    /// once the user's name has been accepted.
    /// </summary>
    public string GetWelcomeMessage()
    {
        return AsciiArt.GetWelcomeMessage(MemoryStore.UserName);
    }

    // -------------------------------------------------------------------------
    // Message processing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes a single user message and returns the bot's response.
    ///
    /// Handles:
    ///   - Empty input validation
    ///   - Exit/bye detection (returns a goodbye string; UI handles closing)
    ///   - Full conversation processing via ConversationManager
    /// </summary>
    public ChatBotResponse ProcessMessage(string userInput)
    {
        // Guard: empty input
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return new ChatBotResponse(
                "Please type a message — I'm here to help!",
                isExit: false,
                isWarning: true);
        }

        string trimmed = userInput.Trim();
        string lower   = trimmed.ToLowerInvariant();

        // Exit detection
        if (lower == "exit" || lower == "bye" || lower == "quit" || lower == "goodbye")
        {
            string goodbye = $"Goodbye, {MemoryStore.UserName}! Stay safe online. 🔐";
            return new ChatBotResponse(goodbye, isExit: true, isWarning: false);
        }

        // Delegate to ConversationManager for all other input
        string response = _conversation.ProcessInput(trimmed);

        // Flag default/unmatched responses so the UI can style them differently
        bool isWarning = response == ResponseCatalog.DefaultResponse;

        return new ChatBotResponse(response, isExit: false, isWarning: isWarning);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the stored user name for use in UI labels.
    /// </summary>
    public string GetUserName() => MemoryStore.UserName;

    /// <summary>
    /// Resets the session — clears memory and starts a fresh conversation.
    /// Useful if you want to add a "Start Over" button in the GUI.
    /// </summary>
    public void ResetSession()
    {
        MemoryStore.Reset();
        SessionStarted = false;
    }
}

// =============================================================================

/// <summary>
/// Wraps the bot's response with metadata the UI needs to style the message.
///
/// isExit    → UI should show goodbye and optionally disable the input box.
/// isWarning → Input wasn't matched; UI can style this message differently
///             (e.g. yellow/amber instead of the normal bot colour).
/// </summary>
public class ChatBotResponse
{
    public string  Message   { get; }
    public bool    IsExit    { get; }
    public bool    IsWarning { get; }

    public ChatBotResponse(string message, bool isExit, bool isWarning)
    {
        Message   = message;
        IsExit    = isExit;
        IsWarning = isWarning;
    }
}

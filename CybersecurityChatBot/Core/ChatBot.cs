public class ChatBot
{
    private readonly ConversationManager _conversation = new ConversationManager();
    public bool SessionStarted { get; private set; } = false;

    public string? SubmitName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return "Name cannot be empty — please enter your name to continue.";

        MemoryStore.SetUserName(rawName.Trim());
        SessionStarted = true;
        return null; // null = success, no error
    }

    public string GetWelcomeMessage()
    {
        return AsciiArt.GetWelcomeMessage(MemoryStore.UserName);
    }
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
    public string GetUserName() => MemoryStore.UserName;
    public void ResetSession()
    {
        MemoryStore.Reset();
        SessionStarted = false;
    }
}
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

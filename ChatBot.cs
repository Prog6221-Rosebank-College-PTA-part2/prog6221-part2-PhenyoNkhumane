using System;

class ChatBot
{
    public static string GetUserName()
    {
        ConsoleUi.WriteTextColored("\nPlease enter your name: ", ConsoleColor.Yellow);

        string name = Console.ReadLine() ?? string.Empty;

        while (string.IsNullOrWhiteSpace(name))
        {
            ConsoleUi.WriteTextColored("Name cannot be empty. Enter your name: ", ConsoleColor.Red);

            name = Console.ReadLine() ?? string.Empty;
        }

        return name;
    }

    public static void StartChat(string userName)
    {
        ConsoleUi.WriteSectionHeader("Chat Session", ConsoleColor.DarkCyan);
        ConsoleUi.WriteLineColored("Ask me about staying safe online (phishing, passwords, and more), or enter 'exit' to leave.", ConsoleColor.Cyan);
        ConsoleUi.WriteDivider();
        Console.WriteLine();

        while (true)
        {
            ConsoleUi.WriteUserPrompt(userName);
            string input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleUi.WriteErrorLine(
                    "That input is empty. Please type a question or topic (for example passwords or phishing).");
                ConsoleUi.WriteExchangeSpacer();
                continue;
            }

            input = input.ToLower().Trim();

            if (input == "exit" || input == "bye")
            {
                ConsoleUi.WriteBotLine($"Goodbye, {userName}! Stay safe online.");
                ConsoleUi.WriteExchangeSpacer();
                break;
            }

            string response = ResponseCatalog.GetResponse(input);
            if (string.Equals(response, ResponseCatalog.DefaultResponse, StringComparison.Ordinal))
            {
                ConsoleUi.WriteWarningLine("I didn't match that to a topic yet—try rephrasing or ask about cybersecurity.");
            }

            ConsoleUi.WriteBotLine(response);
            ConsoleUi.WriteExchangeSpacer();
        }
    }
}
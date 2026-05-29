using System;

class AsciiArt
{
    public static void DisplayBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(@"
            .-""-.
           / .--. \
          / /    \ \
          | |    | |
          | |.-""-.|
         ///`.::::.`\
        ||| ::/  \:: ;
        ||; ::\__/:: ;
         \\\ '::::' /
          `=':-..-'`
░░      ░░░░      ░░░        ░░        ░░░░░░░░  ░░░░  ░░        ░░        ░░       ░░░        ░░   ░░░  ░░░      ░░
▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒  ▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒▒▒▒
▓▓      ▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓     ▓▓▓▓▓      ▓▓▓▓      ▓▓▓▓       ▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓   ▓
███████  ██        ██  ████████  ██████████████  ███  ███  ████████  ████████  ███████████  █████  ██    ██  ████  █
██      ███  ████  ██  ████████        ████████  ████  ██        ██        ██  ████████        ██  ███   ███      ██
                                                                                                                    

              🔐  WITH MAVICKS  🔐
");

        Console.ResetColor();
    }

    public static void DisplayWelcome(string name)
    {
        Console.WriteLine();
        ConsoleUi.WriteBoxedLines(
            new[]
            {
                "Mavicks Cybersecurity Awareness Training",
                $"Welcome, {name}!",
                "Stay Safe Online.",
            },
            ConsoleColor.Green);
        Console.WriteLine();
        ConsoleUi.WriteLineColored("Tip: Ask about phishing, passwords, 2FA, malware, or safe browsing.", ConsoleColor.DarkYellow);
        ConsoleUi.WriteLineColored("Type 'exit' or 'bye' at any time to end the session.", ConsoleColor.DarkYellow);
        Console.WriteLine();
    }
}

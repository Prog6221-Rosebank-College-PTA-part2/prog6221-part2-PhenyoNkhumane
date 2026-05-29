using System;
using System.Collections.Generic;
using System.Linq;

class AsciiArt
{
    public static string GetBanner()
    {
        return @"
░░      ░░░░      ░░░        ░░        ░░░░░░░░  ░░░░  ░░        ░░        ░░       ░░░        ░░   ░░░  ░░░      ░░
▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒  ▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒  ▒▒▒▒▒▒▒
▓▓      ▓▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓      ▓▓▓▓▓▓▓▓▓▓     ▓▓▓▓▓      ▓▓▓▓      ▓▓▓▓       ▓▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓▓▓   ▓
███████  ██        ██  ████████  ██████████████  ███  ███  ████████  ████████  ███████████  █████  ██    ██  ████  █
██      ███  ████  ██  ████████        ████████  ████  ██        ██        ██  ████████        ██  ███   ███      ██

              WITH MAVICKS";
    }

    public static string GetBannerText() => GetBanner();

    public static string GetWelcomeMessage(string name) => GetWelcome(name);

    public static string GetWelcome(string name)
    {
        string[] lines =
        {
            "Mavicks AI Cybersecurity Awareness Training",
            $"Welcome, {name}!",
            "Stay Safe Online.",
        };

        int innerWidth = lines.Max(l => l.Length);
        string top = "╔" + new string('═', innerWidth + 2) + "╗";
        string bottom = "╚" + new string('═', innerWidth + 2) + "╝";
        IEnumerable<string> body = lines.Select(l => "║ " + l.PadRight(innerWidth) + " ║");

        List<string> boxed = new List<string> { top };
        boxed.AddRange(body);
        boxed.Add(bottom);
        boxed.Add(string.Empty);
        boxed.Add("Tip: Ask about phishing, passwords, 2FA, malware, or safe browsing.");
        boxed.Add("Type 'exit' or 'bye' at any time to end the session.");
        boxed.Add(string.Empty);

        return string.Join(Environment.NewLine, boxed);
    }
}

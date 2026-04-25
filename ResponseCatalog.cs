using System;

class ResponseCatalog
{
    /// <summary>Returned when input is empty, unclear, or does not match any topic.</summary>
    public const string DefaultResponse =
        "I didn't quite understand that. Could you rephrase?";

    private static readonly (string[] Keywords, string Response)[] Entries =
    {

        (new[] { "how are you", "how're you", "how r you", "how's it going", "you ok", "you okay" },
            "I'm doing well—thanks for asking! I'm here whenever you want cybersecurity tips."),

        (new[] { "what's your purpose", "what is your purpose", "why do you exist", "what do you do" },
            "I'm Mavicks, a cybersecurity assistant—I'm here to help you stay safer online with practical advice."),

        (new[] { "who are you", "your name", "what are you called" },
            "I'm Mavicks, your cybersecurity chat assistant. Ask me about passwords, phishing, safe browsing, and more."),

        (new[] { "hello", "hey there", "good morning", "good afternoon", "good evening", "greetings" },
            "Hello! Ask me anything about staying safe online, or say what you'd like to know about."),

        (new[] { "what can i ask you about", "what can you help with", "what topics", "show topics", "what else?", "anything else"},
            "You can ask about: password safety, phishing, pharming, safe browsing, malware, 2FA, updates, backups, Wi-Fi safety, privacy, and reporting incidents. Type a topic or ask in your own words."),

        (new[] { "menu", "list commands", "options" },
            "Try asking about: passwords, phishing, pharming, safe browsing, malware, two-factor authentication, or public Wi-Fi. You can also ask how I am or what I'm for!"),

        // --- Cybersecurity topics (keywords ordered so specific phrases match first in iteration) ---
        (new[] { "phishing", "phish", "spam email", "suspicious email", "fake email", "spoofed email" },
            "Phishing tricks you into revealing secrets or clicking bad links. Verify the sender, don't open unexpected attachments, and open sites by typing the URL or using a bookmark—not from urgent links in messages."),

        (new[] { "pharming", "dns poisoning", "rogue dns", "fake website redirect", "redirected site" },
            "Pharming redirects you to a fake website even when the address looks right. Always type sensitive URLs directly, bookmark trusted sites, and avoid using suspicious links or public DNS services without protection."),

        (new[] { "password", "passphrase", "credential", "weak password", "password manager" },
            "Use long, unique passphrases for important accounts. A password manager helps you generate and store them safely—never reuse passwords across sites or share them."),

        (new[] { "2fa", "mfa", "two factor", "multi factor", "authenticator", "second factor" },
            "Multi-factor authentication adds a second step after your password. Prefer an authenticator app or hardware key over SMS when you can."),

        (new[] { "malware", "virus", "trojan", "spyware", "keylogger" },
            "Malware is harmful software. Keep your system updated, use reputable security software, and avoid suspicious downloads or pirated files."),

        (new[] { "social engineering", "pretext", "scam call", "vishing", "impersonation" },
            "Social engineering manipulates people into breaking security rules. Verify unexpected requests through a trusted channel, and never give sensitive info to unsolicited callers."),

        (new[] { "safe browsing", "https", "check url", "suspicious link", "clickjacking" },
            "Check that sites use HTTPS (the padlock), avoid unknown short links, don't ignore browser warnings, and be cautious with downloads."),

        (new[] { "public wifi", "free wifi", "unsecured wifi", "wifi security" },
            "On public Wi-Fi, others may snoop on traffic. Avoid logging into sensitive accounts unless you use a VPN; prefer cellular data for banking when unsure."),

        (new[] { "vpn", "virtual private network" },
            "A VPN encrypts traffic between your device and the VPN provider—useful on untrusted networks. Choose a reputable provider; a VPN is not a substitute for HTTPS and good habits."),

        (new[] { "ransomware", "encrypt my files", "locked files", "pay bitcoin" },
            "Ransomware locks or steals your files. Keep offline backups, patch systems, and never enable macros on unexpected documents."),

        (new[] { "update", "patch", "security update", "outdated software" },
            "Updates fix security holes. Turn on automatic updates for your OS and apps, and restart when prompted so patches actually apply."),

        (new[] { "backup", "backups", "restore", "lost files" },
            "Regular backups (3-2-1: three copies, two media types, one off-site) help you recover from ransomware, theft, or accidents without paying criminals."),

        (new[] { "privacy", "tracking", "cookies", "personal data", "data breach" },
            "Limit what you share online, review app permissions, use privacy settings on accounts, and assume breached passwords will leak—use unique passwords everywhere."),

        (new[] { "encryption", "encrypted", "end to end" },
            "Encryption scrambles data so only intended parties can read it. Prefer encrypted messaging and full-disk encryption on laptops; look for HTTPS on websites."),

        (new[] { "report", "incident", "i was hacked", "report scam" },
            "If something's wrong: disconnect if needed, change passwords (from a clean device), enable MFA, contact your bank or IT, and report phishing to your email provider or authorities as appropriate."),

        (new[] { "cybersecurity", "cyber security", "information security", "infosec", "online safety" },
            "Cybersecurity is about protecting devices, accounts, and data from theft or harm. Strong passwords, updates, skepticism toward unexpected messages, and backups go a long way."),

        // --- Thanks & help (broad "help" last among topic-like entries) ---
        (new[] { "thank you", "thanks", "thx", "appreciate it", "thank u", "much appreciated" },
            "You're welcome! Feel free to ask anytime, or type 'exit' when you're done."),

        (new[] { "help", "i need help", "assist me" },
            "I can explain password safety, phishing, safe browsing, malware, 2FA, Wi-Fi risks, backups, and more—what would you like to know?")
    };

    public static string GetResponse(string userInput)
    {
        foreach (var entry in Entries)
        {
            foreach (var keyword in entry.Keywords)
            {
                if (userInput.Contains(keyword))
                {
                    return entry.Response;
                }
            }
        }

        return DefaultResponse;
    }
}

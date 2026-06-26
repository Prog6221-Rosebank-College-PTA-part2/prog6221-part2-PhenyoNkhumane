using System;
using System.Collections.Generic;

/// <summary>
/// Central knowledge base for the chatbot.
/// Handles keyword matching, random tip variation, and topic identification
/// for use by ConversationManager and SentimentDetector in Part 2.
/// </summary>
public static class ResponseCatalog
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Returned when input does not match any known topic.</summary>
    public const string DefaultResponse =
        "I didn't quite understand that. Try asking about phishing, passwords, 2FA, safe browsing, or use chat commands like \"start quiz\", \"view tasks\", or \"show activity log\".";

    // -------------------------------------------------------------------------
    // Topic name constants
    // Used by ConversationManager to track the last matched topic,
    // and by GetRandomTip() to retrieve varied follow-up responses.
    // -------------------------------------------------------------------------
    public const string TopicPhishing        = "phishing";
    public const string TopicPassword        = "password";
    public const string TopicMalware         = "malware";
    public const string TopicPrivacy         = "privacy";
    public const string TopicSafeBrowsing    = "safe browsing";
    public const string TopicWifi            = "wifi";
    public const string TopicTwoFactor       = "2fa";
    public const string TopicAuthentication  = "authentication";
    public const string TopicRansomware      = "ransomware";
    public const string TopicVpn             = "vpn";
    public const string TopicBackup          = "backup";
    public const string TopicUpdates         = "updates";
    public const string TopicPharming        = "pharming";
    public const string TopicSocialEng       = "social engineering";
    public const string TopicEncryption      = "encryption";
    public const string TopicIncident        = "incident";

    // -------------------------------------------------------------------------
    // Random tip pools
    // Each cybersecurity topic has several tips so the chatbot gives a
    // different response each time the user asks for "another tip" or
    // when sentiment detection triggers an automatic follow-up.
    // -------------------------------------------------------------------------
    private static readonly Dictionary<string, List<string>> RandomTips =
        new Dictionary<string, List<string>>
        {
            [TopicPhishing] = new List<string>
            {
                "Never click links in unsolicited emails—type the URL directly into your browser instead.",
                "Check the sender's actual email address, not just the display name. Scammers often spoof names.",
                "Legitimate organisations will never ask for your password by email. Treat any such request as a red flag.",
                "Look for urgency tactics like 'Act now!' or 'Your account will be suspended'—these are classic phishing pressure techniques.",
                "When in doubt, contact the company directly using a phone number from their official website, not from the email."
            },

            [TopicPassword] = new List<string>
            {
                "Use a passphrase of four or more random words—it's long, memorable, and hard to crack.",
                "Never reuse a password across different sites. A breach on one site puts all your accounts at risk.",
                "A password manager generates and stores unique passwords for every account—you only remember one master password.",
                "Avoid personal details in passwords such as birthdays, names, or pet names. These are easily guessed.",
                "Change passwords immediately if you suspect a breach—don't wait to see if anything bad happens."
            },

            [TopicMalware] = new List<string>
            {
                "Only download software from official sources or trusted stores—avoid pirated files or unofficial mirrors.",
                "Keep your antivirus definitions updated so it can recognise the latest threats.",
                "Never enable macros in unexpected Office documents—macro malware is a common attack vector.",
                "If your device suddenly slows down or shows unexpected pop-ups, run a full security scan immediately.",
                "Disconnect from the internet first if you suspect active malware—this limits what data it can send out."
            },

            [TopicPrivacy] = new List<string>
            {
                "Review app permissions regularly—many apps request access to your camera, contacts, or location unnecessarily.",
                "Use a different email address for sign-ups and newsletters to protect your primary inbox.",
                "Check HaveIBeenPwned.com to see if your email has appeared in a known data breach.",
                "Limit what you share on social media—personal details can be used to answer security questions.",
                "Read privacy policies before accepting—look especially for data-sharing and third-party clauses."
            },

            [TopicSafeBrowsing] = new List<string>
            {
                "Always check for HTTPS and a padlock icon before entering any personal or payment information.",
                "Avoid clicking shortened URLs from unknown sources—use a URL expander tool to preview them first.",
                "Keep your browser updated; browsers patch security holes frequently.",
                "Use a reputable ad blocker—malicious ads (malvertising) can redirect you to harmful sites.",
                "Never ignore browser security warnings. If a site is flagged, trust the warning and leave."
            },

            [TopicWifi] = new List<string>
            {
                "Avoid accessing banking or email on public Wi-Fi—use your mobile data instead.",
                "Always use a VPN when connecting to public or untrusted Wi-Fi networks.",
                "Make sure your home Wi-Fi uses WPA3 or at least WPA2 encryption—never use WEP.",
                "Disable auto-connect on your device so it doesn't join unknown networks automatically.",
                "Log out of accounts and forget the network when you're done using public Wi-Fi."
            },

            [TopicTwoFactor] = new List<string>
            {
                "Use an authenticator app like Google Authenticator or Authy instead of SMS-based 2FA when possible.",
                "Enable 2FA on your most critical accounts first—email, banking, and cloud storage.",
                "Store your 2FA backup codes somewhere safe offline in case you lose your phone.",
                "Hardware security keys like YubiKey offer the strongest form of two-factor authentication.",
                "Even if someone has your password, 2FA stops them from logging in without your second device."
            },

            [TopicAuthentication] = new List<string>
            {
                "Use strong login credentials and keep your username and password unique for every account.",
                "Treat one-time passwords (OTP) like passwords—never share them with anyone.",
                "A secure PIN is short and memorable but not easy to guess. Avoid 1234, 0000, or your birthday.",
                "Biometric options like fingerprint and Face ID are convenient, but pair them with a strong password or PIN.",
                "Authentication is stronger when you combine something you know with something you have or are."
            },

            [TopicRansomware] = new List<string>
            {
                "Never pay the ransom—there is no guarantee you will get your files back, and it funds more attacks.",
                "Keep offline backups so ransomware cannot encrypt your only copy of important files.",
                "Patch your systems regularly—many ransomware attacks exploit known, unpatched vulnerabilities.",
                "Restrict who can install software on shared or work computers to limit ransomware entry points.",
                "Disable Remote Desktop Protocol (RDP) if you don't need it—it's a common ransomware entry point."
            },

            [TopicVpn] = new List<string>
            {
                "Choose a VPN with a strict no-logs policy and a reputable privacy audit.",
                "A VPN hides your traffic from your ISP and public networks, but it does not make you anonymous.",
                "Free VPNs often monetise your data—invest in a paid, trusted provider.",
                "Always verify the VPN connection is active before doing anything sensitive on public Wi-Fi.",
                "A VPN does not replace HTTPS—always look for the padlock on websites even when using a VPN."
            },

            [TopicBackup] = new List<string>
            {
                "Follow the 3-2-1 rule: three copies, two different media types, one stored off-site.",
                "Test your backups regularly by restoring a file—an untested backup may be useless when needed.",
                "Cloud backups are convenient, but keep at least one offline copy in case of ransomware.",
                "Automate your backups so you never forget—manual backups are often skipped.",
                "Encrypt your backup files, especially if they are stored in the cloud or on portable drives."
            },

            [TopicUpdates] = new List<string>
            {
                "Enable automatic updates for your OS, browser, and apps—most attacks exploit unpatched systems.",
                "Restart your device after updates so patches actually take effect.",
                "Don't ignore update prompts—delaying them leaves known vulnerabilities open to attackers.",
                "Update your router firmware too—routers are often forgotten but are a major attack target.",
                "Check that your antivirus software itself is up to date, not just its virus definitions."
            },

            [TopicPharming] = new List<string>
            {
                "Type sensitive URLs directly into your browser rather than following links from emails.",
                "Bookmark your most important sites—banks, email, work portals—and always use those bookmarks.",
                "Use a reputable DNS provider with built-in security filtering, such as Cloudflare or Google DNS.",
                "If a familiar site looks different or asks for unusual info, stop and verify via another channel.",
                "Pharming can happen even on legitimate-looking HTTPS sites if your DNS has been poisoned."
            },

            [TopicSocialEng] = new List<string>
            {
                "Always verify unexpected requests for access or information through a known, trusted channel.",
                "Attackers often impersonate IT support, managers, or banks—take your time and don't be rushed.",
                "Never share passwords or one-time codes with anyone, even someone who claims to be from IT.",
                "Be suspicious of unsolicited help—attackers often offer assistance to gain access.",
                "Train yourself to pause and question urgency—social engineers rely on making you act fast."
            },

            [TopicEncryption] = new List<string>
            {
                "Enable full-disk encryption on your laptop so data is protected if the device is lost or stolen.",
                "Use end-to-end encrypted messaging apps like Signal for sensitive conversations.",
                "Encrypt sensitive files before uploading them to cloud storage.",
                "Look for HTTPS on every website before entering personal data—it encrypts your connection.",
                "Encryption protects data in transit and at rest; make sure you use both where possible."
            },

            [TopicIncident] = new List<string>
            {
                "If hacked, disconnect from the internet first to limit ongoing damage.",
                "Change all passwords from a clean, unaffected device—not the one that was compromised.",
                "Enable MFA on all accounts immediately after a suspected breach.",
                "Report phishing emails to your email provider and relevant authorities such as the SAPS cybercrime unit.",
                "Document what happened—timestamps, messages, URLs—before reporting to help investigators."
            }
        };

    // -------------------------------------------------------------------------
    // Keyword-to-topic mapping
    // Maps user input keywords to a topic name constant.
    // ConversationManager uses the returned topic to support follow-up
    // handling ("tell me more", "give me another tip", etc.).
    // -------------------------------------------------------------------------
    private static readonly (string[] Keywords, string? Topic, string Response)[] Entries =
    {
        // --- General conversation ---
        (new[] { "how are you", "how're you", "how r you", "how's it going", "you ok", "you okay" },
            null,
            "I'm doing well—thanks for asking! I'm here whenever you want cybersecurity tips."),

        (new[] { "what's your purpose", "what is your purpose", "why do you exist", "what do you do" },
            null,
            "I'm Mavicks, a cybersecurity assistant—I'm here to help you stay safer online with practical advice."),

        (new[] { "who are you", "your name", "what are you called" },
            null,
            "I'm Mavicks, your cybersecurity chat assistant. Ask me about passwords, phishing, safe browsing, and more."),

        (new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening", "greetings" },
            null,
            "Hello! Ask me anything about staying safe online, or say what you'd like to know about."),

        (new[] { "what can i ask", "what can you help", "what topics", "show topics", "what else", "anything else" },
            null,
            "You can ask about: password safety, phishing, pharming, safe browsing, malware, 2FA, updates, " +
            "backups, Wi-Fi safety, privacy, VPNs, ransomware, encryption, and reporting incidents. " +
            "Type a topic or ask in your own words."),

        (new[] { "tips", "security tips", "cybersecurity tips", "best practices", "best practices for security", "cyber safety tips", "online safety tips", "security advice", "protect personal data", "personal data protection" },
            null,
            "Comprehensive Cybersecurity Tips for Protecting Personal Data:\n" +
            "Cybersecurity helps protect your devices, accounts, and personal information from unauthorized access, theft, or damage. " +
            "Use these practical habits to strengthen your privacy, finances, and identity security:\n\n" +
            "1. Password Safety:\n" +
            "   • Use strong passwords with 12–16 characters, mixed letters, numbers, and symbols.\n" +
            "   • Avoid personal information like your name, birthday, or pet names.\n" +
            "   • Never reuse passwords across multiple accounts.\n" +
            "   • Consider a password manager to generate and store passwords securely.\n\n" +
            "2. Multi-Factor Authentication (MFA):\n" +
            "   • Enable MFA for email, banking, social media, and other critical accounts.\n" +
            "   • Prefer authenticator apps or hardware keys over SMS when possible.\n\n" +
            "3. Phishing Awareness:\n" +
            "   • Watch for poor spelling, urgent requests, and unexpected attachments.\n" +
            "   • Verify sender addresses and hover over links before clicking.\n" +
            "   • Never send passwords or codes over email or text.\n\n" +
            "4. Safe Browsing Practices:\n" +
            "   • Use HTTPS sites and check for the padlock icon.\n" +
            "   • Avoid suspicious downloads and pop-ups.\n" +
            "   • Keep your browser and extensions updated.\n\n" +
            "5. Device and Network Security:\n" +
            "   • Protect phones and computers with PINs, passwords, or biometrics.\n" +
            "   • Use a secure home Wi-Fi network with WPA2/WPA3 and a strong password.\n" +
            "   • Avoid unsecured public Wi-Fi for sensitive tasks or use a VPN.\n\n" +
            "6. Data Backups and Malware Protection:\n" +
            "   • Back up important files regularly using the 3-2-1 rule.\n" +
            "   • Install reputable antivirus software and scan regularly.\n\n" +
            "7. Privacy and Personal Information:\n" +
            "   • Limit what you share on social media.\n" +
            "   • Review app permissions and remove access that is not needed.\n\n" +
            "8. Account Monitoring and Threat Awareness:\n" +
            "   • Check your financial and online accounts frequently for suspicious activity.\n" +
            "   • Stay informed about new scams and security alerts.\n\n" +
            "9. Secure Disposal and Device Handling:\n" +
            "   • Securely erase devices before disposal or recycling.\n" +
            "   • Remove SIM cards and memory cards.\n\n" +
            "10. Good Cybersecurity Habits:\n" +
            "   • Think before you click. Verify before you trust.\n" +
            "   • Log out of shared computers and do not save passwords on public devices.\n\n" +
            "These practices give you a strong foundation for protecting personal data, preventing identity theft, and staying safer online."),

        (new[] { "menu", "list commands", "options" },
            null,
            "Try asking about: passwords, phishing, pharming, safe browsing, malware, " +
            "two-factor authentication, or public Wi-Fi. You can also ask how I am or what I'm for!"),

        // --- Cybersecurity topics ---
        (new[] { "phishing", "phish", "scam", "scams", "scammer", "spam", "spam email", "suspicious email", "fake email", "spoofed email", "sms scam", "smishing", "vishing", "fake website", "impersonation" },
            TopicPhishing,
            "Phishing tricks you into revealing secrets or clicking bad links. Verify the sender, " +
            "don't open unexpected attachments, and go directly to sites by typing the URL—not from urgent email links."),

        (new[] { "pharming", "dns poisoning", "rogue dns", "fake website redirect", "redirected site" },
            TopicPharming,
            "Pharming redirects you to a fake website even when the address looks correct. " +
            "Always type sensitive URLs directly, bookmark trusted sites, and avoid suspicious DNS services."),

        (new[] { "password", "passwords", "passphrase", "credential", "weak password", "password manager" },
            TopicPassword,
            "Use long, unique passphrases for important accounts. A password manager helps you generate " +
            "and store them safely—never reuse passwords across sites or share them."),

        (new[] { "username", "login", "sign in", "authentication", "biometric", "otp", "pin", "credential" },
            TopicAuthentication,
            "Authentication is more than a password. Use strong login details, protect OTPs and PINs, " +
            "and enable MFA whenever possible."),

        (new[] { "2fa", "mfa", "two factor", "multi factor", "authenticator", "second factor" },
            TopicTwoFactor,
            "Multi-factor authentication adds a second step after your password. " +
            "Prefer an authenticator app or hardware key over SMS codes when possible."),

        (new[] { "malware", "virus", "trojan", "spyware", "keylogger", "adware", "worm" },
            TopicMalware,
            "Malware is harmful software that steals data or damages your system. " +
            "Keep your OS updated, use reputable security software, and avoid suspicious downloads."),

        (new[] { "social engineering", "pretext", "scam call", "vishing", "impersonation" },
            TopicSocialEng,
            "Social engineering manipulates people into breaking security rules. " +
            "Always verify unexpected requests through a trusted channel, and never give info to unsolicited callers."),

        (new[] { "safe browsing", "https", "check url", "suspicious link", "clickjacking" },
            TopicSafeBrowsing,
            "Check that sites use HTTPS (padlock icon), avoid unknown short links, " +
            "don't ignore browser warnings, and be cautious with downloads from unfamiliar sites."),

        (new[] { "public wifi", "free wifi", "unsecured wifi", "wifi security", "wi-fi" },
            TopicWifi,
            "On public Wi-Fi, others may intercept your traffic. Avoid sensitive accounts without a VPN, " +
            "and prefer cellular data for banking when you're unsure of the network."),

        (new[] { "vpn", "virtual private network" },
            TopicVpn,
            "A VPN encrypts traffic between your device and the VPN provider—very useful on untrusted networks. " +
            "Choose a reputable, no-logs provider. A VPN is not a substitute for HTTPS and good habits."),

        (new[] { "ransomware", "encrypt my files", "locked files", "pay bitcoin" },
            TopicRansomware,
            "Ransomware locks or steals your files and demands payment. " +
            "Keep offline backups, patch your systems, and never enable macros on unexpected documents."),

        (new[] { "update", "patch", "security update", "outdated software" },
            TopicUpdates,
            "Updates close security holes. Enable automatic updates for your OS and apps, " +
            "and restart when prompted so patches fully apply."),

        (new[] { "backup", "backups", "restore", "lost files" },
            TopicBackup,
            "Follow the 3-2-1 backup rule: three copies, two media types, one off-site. " +
            "This protects you from ransomware, theft, and accidental deletion."),

        (new[] { "privacy", "personal information", "identity", "identity theft", "location", "permissions", "cookies", "tracking", "data", "personal data", "data breach" },
            TopicPrivacy,
            "Limit what you share online, review app permissions regularly, use account privacy settings, " +
            "and use unique passwords everywhere so a breach on one site doesn't affect others."),

        (new[] { "encryption", "encrypted", "end to end" },
            TopicEncryption,
            "Encryption scrambles data so only intended parties can read it. " +
            "Use encrypted messaging apps, full-disk encryption on laptops, and always look for HTTPS."),

        (new[] { "report", "incident", "i was hacked", "report scam" },
            TopicIncident,
            "If something has gone wrong: disconnect if needed, change passwords from a clean device, " +
            "enable MFA, contact your bank or IT team, and report phishing to your email provider or authorities."),

        (new[] { "cybersecurity", "cyber security", "information security", "infosec", "online safety", "cyber", "hacker", "hacking", "cybercrime", "breach", "exploit", "vulnerability", "antivirus", "defender" },
            null,
            "Cybersecurity is about protecting your devices, accounts, and data from theft or harm. " +
            "Strong passwords, regular updates, scepticism toward unexpected messages, and backups cover most risks."),

        // --- Thanks & help ---
        (new[] { "thank you", "thanks", "thx", "appreciate it", "thank u", "much appreciated" },
            null,
            "You're welcome! Feel free to ask anytime."),

        (new[] { "help", "i need help", "assist me" },
            null,
            "I can explain password safety, phishing, safe browsing, malware, 2FA, Wi-Fi risks, " +
            "backups, and more—what would you like to know?")
    };

    // -------------------------------------------------------------------------
    // Public methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the best matching response for the given input, and outputs
    /// the matched topic name (or null if no cybersecurity topic was matched).
    /// The topic is used by ConversationManager to track conversation context.
    /// </summary>
    public static string GetResponse(string userInput, out string? matchedTopic)
    {
        string input = userInput.ToLowerInvariant().Trim();

        foreach (var entry in Entries)
        {
            foreach (var keyword in entry.Keywords)
            {
                if (input.Contains(keyword))
                {
                    matchedTopic = entry.Topic;
                    return entry.Response;
                }
            }
        }

        matchedTopic = null;
        return DefaultResponse;
    }

    /// <summary>
    /// Overload without topic output — used when context tracking is not needed.
    /// </summary>
    public static string GetResponse(string userInput)
    {
        return GetResponse(userInput, out _);
    }

    /// <summary>
    /// Returns a random tip for the given topic.
    /// Called when the user asks for "another tip" or when sentiment detection
    /// triggers an automatic follow-up after an empathy response.
    /// Returns null if no tips exist for the topic.
    /// </summary>
    public static string? GetRandomTip(string? topic)
    {
        if (topic == null || !RandomTips.ContainsKey(topic))
            return null;

        var tips = RandomTips[topic];
        int index = Random.Shared.Next(tips.Count);
        return tips[index];
    }

    /// <summary>
    /// Returns the full list of tips for a topic.
    /// Useful if you want to cycle through all tips without repeating.
    /// </summary>
    public static List<string> GetAllTips(string? topic)
    {
        if (topic != null && RandomTips.ContainsKey(topic))
            return RandomTips[topic];

        return new List<string>();
    }

    /// <summary>
    /// Checks whether a given topic has random tips available.
    /// </summary>
    public static bool HasRandomTips(string? topic)
    {
        return topic != null && RandomTips.ContainsKey(topic);
    }
}

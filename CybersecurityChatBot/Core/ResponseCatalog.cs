using System;

class ResponseCatalog
{
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

        (new[] { "vpn", "virtual private network" },using System;
using System.Collections.Generic;
public static class ResponseCatalog
{
    public const string DefaultResponse =
        "I didn't quite understand that. Could you rephrase?";
    public const string TopicPhishing        = "phishing";
    public const string TopicPassword        = "password";
    public const string TopicMalware         = "malware";
    public const string TopicPrivacy         = "privacy";
    public const string TopicSafeBrowsing    = "safe browsing";
    public const string TopicWifi            = "wifi";
    public const string TopicTwoFactor       = "2fa";
    public const string TopicRansomware      = "ransomware";
    public const string TopicVpn             = "vpn";
    public const string TopicBackup          = "backup";
    public const string TopicUpdates         = "updates";
    public const string TopicPharming        = "pharming";
    public const string TopicSocialEng       = "social engineering";
    public const string TopicEncryption      = "encryption";
    public const string TopicIncident        = "incident";

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

        (new[] { "menu", "list commands", "options" },
            null,
            "Try asking about: passwords, phishing, pharming, safe browsing, malware, " +
            "two-factor authentication, or public Wi-Fi. You can also ask how I am or what I'm for!"),

        // --- Cybersecurity topics ---
        (new[] { "phishing", "phish", "scam", "scams", "spam email", "suspicious email", "fake email", "spoofed email" },
            TopicPhishing,
            "Phishing tricks you into revealing secrets or clicking bad links. Verify the sender, " +
            "don't open unexpected attachments, and go directly to sites by typing the URL—not from urgent email links."),

        (new[] { "pharming", "dns poisoning", "rogue dns", "fake website redirect", "redirected site" },
            TopicPharming,
            "Pharming redirects you to a fake website even when the address looks correct. " +
            "Always type sensitive URLs directly, bookmark trusted sites, and avoid suspicious DNS services."),

        (new[] { "password", "passphrase", "credential", "weak password", "password manager" },
            TopicPassword,
            "Use long, unique passphrases for important accounts. A password manager helps you generate " +
            "and store them safely—never reuse passwords across sites or share them."),

        (new[] { "2fa", "mfa", "two factor", "multi factor", "authenticator", "second factor" },
            TopicTwoFactor,
            "Multi-factor authentication adds a second step after your password. " +
            "Prefer an authenticator app or hardware key over SMS codes when possible."),

        (new[] { "malware", "virus", "trojan", "spyware", "keylogger" },
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

        (new[] { "privacy", "tracking", "cookies", "personal data", "data breach" },
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

        (new[] { "cybersecurity", "cyber security", "information security", "infosec", "online safety" },
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

    public static string GetResponse(string userInput)
    {
        return GetResponse(userInput, out _);
    }

    public static string? GetRandomTip(string? topic)
    {
        if (topic == null || !RandomTips.ContainsKey(topic))
            return null;

        var tips = RandomTips[topic];
        int index = Random.Shared.Next(tips.Count);
        return tips[index];
    }
    public static List<string> GetAllTips(string? topic)
    {
        if (topic != null && RandomTips.ContainsKey(topic))
            return RandomTips[topic];

        return new List<string>();
    }
    public static bool HasRandomTips(string? topic)
    {
        return topic != null && RandomTips.ContainsKey(topic);
    }
}

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

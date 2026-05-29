using System.Collections.Generic;
using CybersecurityChatbot.Features;

class ConversationManager
{
    public string? LastTopic { get; private set; }

    private int _tipCycleIndex;

    private static readonly List<string> FollowUpPhrases = new List<string>
    {
        "tell me more",
        "explain more",
        "more details",
        "give me another tip",
        "another tip",
        "go on",
        "continue",
        "what else",
        "more please",
        "keep going",
        "elaborate",
        "and then",
        "more info",
        "more information",
        "expand on that",
        "say more"
    };

    private static readonly List<string> InterestPhrases = new List<string>
    {
        "i'm interested in",
        "i am interested in",
        "i care about",
        "i want to learn about",
        "tell me about",
        "i'd like to know about",
        "i like"
    };

    public string Process(string userName, string rawInput)
    {
        return ProcessInput(rawInput);
    }

    public string ProcessInput(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return "Please type a message — I'm here to help!";
        }

        string input = rawInput.ToLowerInvariant().Trim();

        string? interestTopic = TryExtractInterest(input);
        if (interestTopic != null)
        {
            MemoryStore.SetFavouriteTopic(interestTopic);
            UpdateTopic(interestTopic);
            return MemoryStore.GetTopicSavedConfirmation();
        }

        if (IsFollowUp(input))
        {
            return HandleFollowUp();
        }

        string? sentimentResponse = SentimentDetector.DetectAndRespond(rawInput, LastTopic);
        if (sentimentResponse != null)
        {
            return sentimentResponse;
        }

        string response = ResponseCatalog.GetResponse(input, out string? matchedTopic);

        if (matchedTopic != null)
        {
            UpdateTopic(matchedTopic);

            if (MemoryStore.HasFavouriteTopic &&
                MemoryStore.FavouriteTopic != matchedTopic)
            {
                string? recall = MemoryStore.GetTopicRecallLine();
                if (recall != null)
                {
                    response += $"\n\n🧠 {recall}";
                }
            }
        }

        return response;
    }

    private bool IsFollowUp(string input)
    {
        foreach (string phrase in FollowUpPhrases)
        {
            if (input.Contains(phrase))
            {
                return true;
            }
        }

        return false;
    }

    private string HandleFollowUp()
    {
        if (LastTopic == null)
        {
            return "We haven't discussed a specific topic yet — what would you like to know about? " +
                   "Try asking about phishing, passwords, malware, or safe browsing.";
        }

        List<string> allTips = ResponseCatalog.GetAllTips(LastTopic);

        if (allTips.Count == 0)
        {
            return ResponseCatalog.GetResponse(LastTopic, out _);
        }

        string tip = allTips[_tipCycleIndex % allTips.Count];
        _tipCycleIndex++;

        return $"Here's another tip on {LastTopic}:\n\n💡 {tip}";
    }

    private void UpdateTopic(string newTopic)
    {
        if (newTopic != LastTopic)
        {
            LastTopic = newTopic;
            _tipCycleIndex = 0;
        }
    }

    private string? TryExtractInterest(string input)
    {
        foreach (string phrase in InterestPhrases)
        {
            if (input.Contains(phrase))
            {
                int index = input.IndexOf(phrase) + phrase.Length;
                string remainder = input.Substring(index).Trim().TrimEnd('.', '!', '?');

                if (string.IsNullOrWhiteSpace(remainder))
                {
                    return null;
                }

                return MapToKnownTopic(remainder);
            }
        }

        return null;
    }

    private static string? MapToKnownTopic(string text)
    {
        var topicMap = new Dictionary<string, string>
        {
            ["phishing"] = ResponseCatalog.TopicPhishing,
            ["phish"] = ResponseCatalog.TopicPhishing,
            ["password"] = ResponseCatalog.TopicPassword,
            ["passwords"] = ResponseCatalog.TopicPassword,
            ["malware"] = ResponseCatalog.TopicMalware,
            ["virus"] = ResponseCatalog.TopicMalware,
            ["privacy"] = ResponseCatalog.TopicPrivacy,
            ["safe browsing"] = ResponseCatalog.TopicSafeBrowsing,
            ["browsing"] = ResponseCatalog.TopicSafeBrowsing,
            ["wifi"] = ResponseCatalog.TopicWifi,
            ["wi-fi"] = ResponseCatalog.TopicWifi,
            ["2fa"] = ResponseCatalog.TopicTwoFactor,
            ["two factor"] = ResponseCatalog.TopicTwoFactor,
            ["mfa"] = ResponseCatalog.TopicTwoFactor,
            ["ransomware"] = ResponseCatalog.TopicRansomware,
            ["vpn"] = ResponseCatalog.TopicVpn,
            ["backup"] = ResponseCatalog.TopicBackup,
            ["backups"] = ResponseCatalog.TopicBackup,
            ["updates"] = ResponseCatalog.TopicUpdates,
            ["patching"] = ResponseCatalog.TopicUpdates,
            ["pharming"] = ResponseCatalog.TopicPharming,
            ["social engineering"] = ResponseCatalog.TopicSocialEng,
            ["encryption"] = ResponseCatalog.TopicEncryption,
            ["incident"] = ResponseCatalog.TopicIncident,
            ["scam"] = ResponseCatalog.TopicPhishing,
            ["scams"] = ResponseCatalog.TopicPhishing,
        };

        foreach (var pair in topicMap)
        {
            if (text.Contains(pair.Key))
            {
                return pair.Value;
            }
        }

        return null;
    }
}

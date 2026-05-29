using System.Collections.Generic;

public static class SentimentDetector
{
    public enum Sentiment
    {
        None,
        Worried,
        Frustrated,
        Curious,
        Overwhelmed,
        Confident
    }

    private static readonly Dictionary<Sentiment, string[]> SentimentKeywords =
        new Dictionary<Sentiment, string[]>
        {
            [Sentiment.Worried] = new[]
            {
                "worried", "scared", "afraid", "anxious", "nervous",
                "fear", "frightened", "unsafe", "not safe", "concerned"
            },

            [Sentiment.Frustrated] = new[]
            {
                "frustrated", "annoyed", "angry", "this is useless",
                "not helpful", "don't understand", "confused", "lost",
                "makes no sense", "too complicated", "difficult"
            },

            [Sentiment.Curious] = new[]
            {
                "curious", "interested", "want to learn", "tell me more",
                "how does", "what is", "why does", "explain", "wondering"
            },

            [Sentiment.Overwhelmed] = new[]
            {
                "overwhelmed", "too much", "so much", "can't keep up",
                "don't know where to start", "a lot to remember",
                "complicated", "hard to remember"
            },

            [Sentiment.Confident] = new[]
            {
                "confident", "i know this", "got it", "understood",
                "makes sense", "clear", "easy", "simple", "no problem"
            }
        };

    private static readonly Dictionary<Sentiment, string> EmpathyLines =
        new Dictionary<Sentiment, string>
        {
            [Sentiment.Worried] =
                "It's completely understandable to feel that way — cyber threats can be unsettling. " +
                "Let me share a tip to help you feel more in control:",

            [Sentiment.Frustrated] =
                "I hear you — cybersecurity can feel overwhelming at times. " +
                "Let me try to make this clearer with a straightforward tip:",

            [Sentiment.Curious] =
                "I love the curiosity! That's exactly the right mindset for staying safe online. " +
                "Here's something useful to know:",

            [Sentiment.Overwhelmed] =
                "It's okay — you don't need to learn everything at once. " +
                "Let's focus on one simple thing you can do right now:",

            [Sentiment.Confident] =
                "That's great to hear! Confidence in cybersecurity goes a long way. " +
                "Here's something extra to keep you sharp:"
        };

    private static readonly Dictionary<Sentiment, string> DefaultTopicForSentiment =
        new Dictionary<Sentiment, string>
        {
            [Sentiment.Worried] = ResponseCatalog.TopicPhishing,
            [Sentiment.Frustrated] = ResponseCatalog.TopicPassword,
            [Sentiment.Curious] = ResponseCatalog.TopicSafeBrowsing,
            [Sentiment.Overwhelmed] = ResponseCatalog.TopicBackup,
            [Sentiment.Confident] = ResponseCatalog.TopicTwoFactor
        };

    public static Sentiment Detect(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return Sentiment.None;
        }

        string input = userInput.ToLowerInvariant();

        foreach (var pair in SentimentKeywords)
        {
            foreach (string keyword in pair.Value)
            {
                if (input.Contains(keyword))
                {
                    return pair.Key;
                }
            }
        }

        return Sentiment.None;
    }
    
    public static string? BuildResponse(Sentiment sentiment, string? preferredTopic = null)
    {
        if (sentiment == Sentiment.None)
        {
            return null;
        }

        string topic = !string.IsNullOrEmpty(preferredTopic) && ResponseCatalog.HasRandomTips(preferredTopic)
            ? preferredTopic
            : DefaultTopicForSentiment[sentiment];

        string empathy = EmpathyLines[sentiment];
        string? tip = ResponseCatalog.GetRandomTip(topic);

        if (tip == null)
        {
            tip = ResponseCatalog.GetResponse(topic, out _);
        }

        return $"{empathy}\n\n💡 {tip}";
    }
    public static string? DetectAndRespond(string userInput, string? preferredTopic = null)
    {
        Sentiment sentiment = Detect(userInput);

        if (sentiment == Sentiment.None)
        {
            return null;
        }

        return BuildResponse(sentiment, preferredTopic);
    }
}

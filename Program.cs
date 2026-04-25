using System;

class Program
{
    static void Main()
    {
        VoiceGreeting.PlayVoiceGreeting();
        AsciiArt.DisplayBanner();
        string userName = ChatBot.GetUserName();
        AsciiArt.DisplayWelcome(userName);
        ChatBot.StartChat(userName);
    }
}
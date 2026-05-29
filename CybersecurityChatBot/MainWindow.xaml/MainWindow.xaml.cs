using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

public partial class MainWindow : Window
{
    private readonly ChatBot _chatBot = new ChatBot();

    // Colour brushes — defined once and reused for every message bubble
    private static readonly SolidColorBrush BrushBotText     = new SolidColorBrush(Color.FromRgb(0x00, 0xe6, 0x76));  // green
    private static readonly SolidColorBrush BrushUserText    = new SolidColorBrush(Color.FromRgb(0xff, 0xd6, 0x00));  // yellow
    private static readonly SolidColorBrush BrushWarningText = new SolidColorBrush(Color.FromRgb(0xff, 0xab, 0x00));  // amber
    private static readonly SolidColorBrush BrushMuted       = new SolidColorBrush(Color.FromRgb(0x60, 0x7d, 0x8b));  // grey
    private static readonly SolidColorBrush BrushBotBubble   = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x2a));  // dark panel
    private static readonly SolidColorBrush BrushUserBubble  = new SolidColorBrush(Color.FromRgb(0x1a, 0x2a, 0x1a));  // dark green tint


    public MainWindow()
    {
        InitializeComponent();

        // Play WAV greeting before the window is interactive
        // PlaySync blocks briefly — acceptable for a short clip
        VoiceGreeting.PlayVoiceGreeting();

        // Populate the ASCII banner in the header
        BannerTextBlock.Text = AsciiArt.GetBannerText();

        // Focus the name input immediately so the user can type right away
        NameInputBox.Focus();
    }

    private void NameInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            SubmitName();
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        SubmitName();
    }

    private void SubmitName()
    {
        string? error = _chatBot.SubmitName(NameInputBox.Text);

        if (error != null)
        {
            // Show validation error inside the overlay card
            NameErrorLabel.Text       = error;
            NameErrorLabel.Visibility = Visibility.Visible;
            NameInputBox.Focus();
            return;
        }

        // Name accepted — hide the overlay with a quick fade
        FadeOut(NamePromptOverlay, onComplete: () =>
        {
            NamePromptOverlay.Visibility = Visibility.Collapsed;

            // Show the ASCII welcome message in the chat area
            AppendBotMessage(_chatBot.GetWelcomeMessage(), isWelcome: true);

            // Update window title with the user's name
            Title = $"🔐 Mavicks — {_chatBot.GetUserName()}";

            // Ready for input
            UserInputBox.IsEnabled = true;
            SendButton.IsEnabled   = true;
            UserInputBox.Focus();
        });
    }


    private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Send();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        Send();
    }

    private void Send()
    {
        string userText = UserInputBox.Text.Trim();

        // Silently ignore empty sends (button press with no text)
        if (string.IsNullOrWhiteSpace(userText))
        {
            UserInputBox.Focus();
            return;
        }

        // Show what the user typed
        AppendUserMessage(userText);
        UserInputBox.Clear();

        // Get the bot's response
        ChatBotResponse result = _chatBot.ProcessMessage(userText);

        // Append response with appropriate styling
        AppendBotMessage(result.Message, isWarning: result.IsWarning);

        // If the user said goodbye, disable input
        if (result.IsExit)
        {
            UserInputBox.IsEnabled = false;
            SendButton.IsEnabled   = false;
            AppendSystemMessage("Session ended. Close the window or restart to begin again.");
        }

        UserInputBox.Focus();
    }

    private void AppendUserMessage(string text)
    {
        string label = $"{_chatBot.GetUserName()} >";

        var bubble = BuildBubble(
            label:      label,
            message:    text,
            labelBrush: BrushUserText,
            textBrush:  BrushUserText,
            bgBrush:    BrushUserBubble,
            alignRight: true);

        ChatPanel.Children.Add(bubble);
        ScrollToBottom();
    }

    private void AppendBotMessage(string text,
                                  bool isWarning = false,
                                  bool isWelcome = false)
    {
        SolidColorBrush textBrush = isWarning ? BrushWarningText : BrushBotText;
        string label = "Mavicks >";

        Border bubble;

        if (isWelcome)
        {
            // Welcome message uses Consolas so the box-drawing chars align
            bubble = BuildMonoBubble(label, text, BrushBotText, BrushBotBubble);
        }
        else
        {
            bubble = BuildBubble(
                label:      label,
                message:    text,
                labelBrush: BrushBotText,
                textBrush:  textBrush,
                bgBrush:    BrushBotBubble,
                alignRight: false);
        }

        ChatPanel.Children.Add(bubble);
        ScrollToBottom();
    }

    private void AppendSystemMessage(string text)
    {
        var tb = new TextBlock
        {
            Text                = text,
            Foreground          = BrushMuted,
            FontFamily          = new FontFamily("Consolas"),
            FontSize            = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 8, 0, 8),
            TextWrapping        = TextWrapping.Wrap
        };

        ChatPanel.Children.Add(tb);
        ScrollToBottom();
    }

    private Border BuildBubble(string label,
                               string message,
                               SolidColorBrush labelBrush,
                               SolidColorBrush textBrush,
                               SolidColorBrush bgBrush,
                               bool alignRight)
    {
        var labelBlock = new TextBlock
        {
            Text       = label,
            Foreground = labelBrush,
            FontFamily = new FontFamily("Consolas"),
            FontSize   = 12,
            FontWeight = FontWeights.Bold,
            Margin     = new Thickness(0, 0, 0, 4)
        };

        var messageBlock = new TextBlock
        {
            Text         = message,
            Foreground   = textBrush,
            FontFamily   = new FontFamily("Segoe UI"),
            FontSize     = 13,
            TextWrapping = TextWrapping.Wrap,
            LineHeight   = 20
        };

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(labelBlock);
        stack.Children.Add(messageBlock);

        var border = new Border
        {
            Background          = bgBrush,
            CornerRadius        = new CornerRadius(8),
            Padding             = new Thickness(14, 10, 14, 10),
            Margin              = new Thickness(
                alignRight ? 80 : 0,   // left margin
                4,
                alignRight ? 0 : 80,   // right margin
                4),
            HorizontalAlignment = alignRight
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left,
            Child = stack
        };

        return border;
    }

    private Border BuildMonoBubble(string label,
                                   string message,
                                   SolidColorBrush textBrush,
                                   SolidColorBrush bgBrush)
    {
        var labelBlock = new TextBlock
        {
            Text       = label,
            Foreground = textBrush,
            FontFamily = new FontFamily("Consolas"),
            FontSize   = 12,
            FontWeight = FontWeights.Bold,
            Margin     = new Thickness(0, 0, 0, 4)
        };

        var messageBlock = new TextBlock
        {
            Text         = message,
            Foreground   = textBrush,
            FontFamily   = new FontFamily("Consolas"),
            FontSize     = 13,
            TextWrapping = TextWrapping.NoWrap
        };

        var stack = new StackPanel { Orientation = Orientation.Vertical };
        stack.Children.Add(labelBlock);
        stack.Children.Add(messageBlock);

        return new Border
        {
            Background          = bgBrush,
            CornerRadius        = new CornerRadius(8),
            Padding             = new Thickness(14, 10, 14, 10),
            Margin              = new Thickness(0, 4, 80, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child               = stack
        };
    }

    private void ScrollToBottom()
    {
        // Defer until after layout so the new element has been measured
        Dispatcher.InvokeAsync(() =>
        {
            ChatScrollViewer.ScrollToEnd();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void FadeOut(UIElement element, Action? onComplete)
    {
        var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        anim.Completed += (s, e) => onComplete?.Invoke();
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }
}

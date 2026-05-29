using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

/// <summary>
/// Code-behind for MainWindow.xaml.
///
/// Responsibilities:
///   - Play voice greeting on startup
///   - Show name prompt overlay and validate the name via ChatBot
///   - Route user messages to ChatBot.ProcessMessage()
///   - Render bot and user messages as styled TextBlocks in ChatPanel
///   - Auto-scroll the chat to the latest message
///
/// No chatbot logic lives here — this file only handles UI events
/// and delegates everything else to the Core layer.
/// </summary>
public partial class MainWindow : Window
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>One ChatBot instance per window = one conversation per session.</summary>
    private readonly ChatBot _chatBot = new ChatBot();

    // Colour brushes — defined once and reused for every message bubble
    private static readonly SolidColorBrush BrushBotText     = new SolidColorBrush(Color.FromRgb(0x00, 0xe6, 0x76));  // green
    private static readonly SolidColorBrush BrushUserText    = new SolidColorBrush(Color.FromRgb(0xff, 0xd6, 0x00));  // yellow
    private static readonly SolidColorBrush BrushWarningText = new SolidColorBrush(Color.FromRgb(0xff, 0xab, 0x00));  // amber
    private static readonly SolidColorBrush BrushMuted       = new SolidColorBrush(Color.FromRgb(0x60, 0x7d, 0x8b));  // grey
    private static readonly SolidColorBrush BrushBotBubble   = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x2a));  // dark panel
    private static readonly SolidColorBrush BrushUserBubble  = new SolidColorBrush(Color.FromRgb(0x1a, 0x2a, 0x1a));  // dark green tint

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Name prompt handlers
    // -------------------------------------------------------------------------

    /// <summary>Enter key inside the name box triggers the start button.</summary>
    private void NameInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            SubmitName();
    }

    /// <summary>Start button click.</summary>
    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        SubmitName();
    }

    /// <summary>
    /// Validates the name via ChatBot.SubmitName().
    /// On success: hides the overlay and opens the chat session.
    /// On failure: shows the inline error label.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Chat input handlers
    // -------------------------------------------------------------------------

    /// <summary>Enter key in the chat input box triggers Send.</summary>
    private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Send();
    }

    /// <summary>Send button click.</summary>
    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        Send();
    }

    /// <summary>
    /// Reads the input box, appends the user message, processes it through
    /// ChatBot, then appends the bot's response.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Message rendering helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Appends a user message bubble (right-aligned, yellow text).
    /// </summary>
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

    /// <summary>
    /// Appends a bot message bubble (left-aligned, green or amber text).
    /// isWelcome uses a monospaced block for the ASCII welcome box.
    /// </summary>
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

    /// <summary>
    /// Appends a centred grey system message (session end notice, etc.).
    /// </summary>
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

    /// <summary>
    /// Builds a standard message bubble with a coloured label and message text.
    /// </summary>
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

    /// <summary>
    /// Builds a monospaced bubble for the ASCII welcome box.
    /// Uses Consolas throughout so box-drawing characters align correctly.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Scroll helper
    // -------------------------------------------------------------------------

    /// <summary>Scrolls the chat ScrollViewer to the latest message.</summary>
    private void ScrollToBottom()
    {
        // Defer until after layout so the new element has been measured
        Dispatcher.InvokeAsync(() =>
        {
            ChatScrollViewer.ScrollToEnd();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    // -------------------------------------------------------------------------
    // Animation helper
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fades a UIElement to opacity 0 over 300 ms then runs onComplete.
    /// Used to dismiss the name prompt overlay smoothly.
    /// </summary>
    private void FadeOut(UIElement element, Action? onComplete)
    {
        var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        anim.Completed += (s, e) => onComplete?.Invoke();
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }
}

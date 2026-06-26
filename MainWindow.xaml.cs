using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

public partial class MainWindow : Window
{
    private readonly ChatBot _chatBot = new ChatBot();

    private static readonly SolidColorBrush BrushBotText     = new SolidColorBrush(Color.FromRgb(0x00, 0xe6, 0x76));
    private static readonly SolidColorBrush BrushUserText    = new SolidColorBrush(Color.FromRgb(0xff, 0xd6, 0x00));
    private static readonly SolidColorBrush BrushWarningText = new SolidColorBrush(Color.FromRgb(0xff, 0xab, 0x00));
    private static readonly SolidColorBrush BrushMuted       = new SolidColorBrush(Color.FromRgb(0x60, 0x7d, 0x8b));
    private static readonly SolidColorBrush BrushBotBubble   = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x2a));
    private static readonly SolidColorBrush BrushUserBubble  = new SolidColorBrush(Color.FromRgb(0x1a, 0x2a, 0x1a));

    public MainWindow()
    {
        InitializeComponent();

        VoiceGreeting.PlayVoiceGreeting();
        BannerTextBlock.Text = AsciiArt.GetBannerText();
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
            NameErrorLabel.Text       = error;
            NameErrorLabel.Visibility = Visibility.Visible;
            NameInputBox.Focus();
            return;
        }

        FadeOut(NamePromptOverlay, onComplete: () =>
        {
            NamePromptOverlay.Visibility = Visibility.Collapsed;
            AppendBotMessage(_chatBot.GetWelcomeMessage(), isWelcome: true);
            Title = $"🔐 Mavicks — {_chatBot.GetUserName()}";
            UserInputBox.IsEnabled = true;
            SendButton.IsEnabled   = true;
            RefreshSidebar();
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

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ChatPanel.Children.Clear();
        AppendSystemMessage("Chat history cleared.");
    }

    private void Send()
    {
        string userText = UserInputBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(userText))
        {
            UserInputBox.Focus();
            return;
        }

        AppendUserMessage(userText);
        UserInputBox.Clear();

        ChatBotResponse result = _chatBot.ProcessMessage(userText);
        AppendBotMessage(result.Message, isWarning: result.IsWarning);
        RefreshSidebar();

        if (result.IsExit)
        {
            AppendSystemMessage("Session ended. You can keep typing to continue the conversation.");
        }

        UserInputBox.Focus();
    }

    private void StartQuizButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("Start quiz");

    private void ViewTasksButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("View tasks");

    private void ActivityLogButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("Show activity log");

    private void RefreshTasksButton_Click(object sender, RoutedEventArgs e) =>
        RefreshSidebar();

    private void RunQuickCommand(string command)
    {
        if (!_chatBot.SessionStarted)
            return;

        AppendUserMessage(command);
        ChatBotResponse result = _chatBot.ProcessQuickCommand(command);
        AppendBotMessage(result.Message, isWarning: result.IsWarning);
        RefreshSidebar();
        UserInputBox.Focus();
    }

    private void RefreshSidebar()
    {
        TasksListView.ItemsSource = null;
        TasksListView.ItemsSource = _chatBot.GetTasks();
        QuizStatusTextBlock.Text = _chatBot.GetQuizStatusText();

        DbStatusTextBlock.Text = TaskDatabase.IsAvailable
            ? "Database: connected ✓"
            : TaskDatabase.IsLocalFallback
                ? $"Database: offline — using local task store ({TaskDatabase.LastError ?? "no error details"})"
                : $"Database: offline — {TaskDatabase.LastError}";
    }

    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        string title       = NewTaskTitleTextBox.Text.Trim();
        string description = NewTaskDescriptionTextBox.Text.Trim();
        string reminder    = NewTaskReminderTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            AppendBotMessage("Please enter a task title before adding a task.", isWarning: true);
            return;
        }

        string command = "Add task";
        if (!string.IsNullOrWhiteSpace(description))
            command += $" - {title}: {description}";
        else
            command += $" - {title}";

        if (!string.IsNullOrWhiteSpace(reminder))
            command += $" remind me {reminder}";

        AppendUserMessage(command);
        var response = _chatBot.ProcessMessage(command);
        AppendBotMessage(response.Message, isWarning: response.IsWarning);

        NewTaskTitleTextBox.Clear();
        NewTaskDescriptionTextBox.Clear();
        NewTaskReminderTextBox.Clear();
        RefreshSidebar();
    }

    private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListView.SelectedItem is CyberTask task)
        {
            string command = $"complete task {task.Id}";
            AppendUserMessage(command);
            var response = _chatBot.ProcessMessage(command);
            AppendBotMessage(response.Message, isWarning: response.IsWarning);
            RefreshSidebar();
        }
        else
        {
            AppendBotMessage("Select a task from the list first, then click Complete.", isWarning: true);
        }
    }

    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListView.SelectedItem is CyberTask task)
        {
            string command = $"delete task {task.Id}";
            AppendUserMessage(command);
            var response = _chatBot.ProcessMessage(command);
            AppendBotMessage(response.Message, isWarning: response.IsWarning);
            RefreshSidebar();
        }
        else
        {
            AppendBotMessage("Select a task from the list first, then click Delete.", isWarning: true);
        }
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
                alignRight ? 80 : 0,
                4,
                alignRight ? 0 : 80,
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

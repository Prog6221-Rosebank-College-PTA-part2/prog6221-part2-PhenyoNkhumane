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
            
            var delayTimer = new System.Timers.Timer(350)
            {
                AutoReset = false
            };
            delayTimer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    AppendBotMessage(_chatBot.GetOpeningDashboardMessage(), isWelcome: true);
                    ShowConversationSuggestions();
                    RefreshSidebar();
                });
                delayTimer.Dispose();
            };
            delayTimer.Start();

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

        // Show typing animation
        AppendTypingIndicator();

        // Process message with a slight delay for realism
        var delayTimer = new System.Timers.Timer(TypingAnimation.GetTypingDelay())
        {
            AutoReset = false
        };
        delayTimer.Elapsed += (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                // Remove typing indicator
                RemoveLastMessage();

                ChatBotResponse result = _chatBot.ProcessMessage(userText);
                
                // Check for special UI commands
                if (userText.StartsWith("check password", StringComparison.OrdinalIgnoreCase))
                {
                    string password = userText.Substring("check password".Length).Trim();
                    AppendBotMessage(_chatBot.CheckPasswordStrength(password), isWarning: false);
                }
                else if (userText.StartsWith("generate password", StringComparison.OrdinalIgnoreCase))
                {
                    AppendBotMessage($"Suggested Password\n{_chatBot.GeneratePassword()}\nStrength: ★★★★★\nCopy Password", isWarning: false);
                }
                else if (userText.StartsWith("toggle ", StringComparison.OrdinalIgnoreCase) || userText.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                {
                    string setting = userText.Replace("toggle ", "", StringComparison.OrdinalIgnoreCase).Replace("set ", "", StringComparison.OrdinalIgnoreCase).Trim();
                    AppendBotMessage(_chatBot.ToggleSettings(setting), isWarning: false);
                }
                else if (result.Message == "[DASHBOARD]")
                {
                    ShowDashboard();
                }
                else if (result.Message == "[STATISTICS]")
                {
                    ShowStatistics();
                }
                else if (result.Message == "[SETTINGS]")
                {
                    ShowSettings();
                }
                else if (result.Message == "[HELP]")
                {
                    ShowHelp();
                }
                else if (result.Message == "[SUGGESTIONS]")
                {
                    ShowConversationSuggestions();
                }
                else
                {
                    AppendBotMessage(result.Message, isWarning: result.IsWarning);
                }

                RefreshSidebar();

                if (result.IsExit)
                {
                    AppendSystemMessage("Session ended. You can keep typing to continue the conversation.");
                }

                UserInputBox.Focus();
            });
            delayTimer.Dispose();
        };
        delayTimer.Start();
    }

    private void StartQuizButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("Start quiz");

    private void RestartQuizButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_chatBot.SessionStarted)
            return;

        AppendUserMessage("Start quiz");
        ChatBotResponse result = _chatBot.ProcessQuickCommand("Start quiz");
        AppendBotMessage(result.Message, isWarning: result.IsWarning);
        RefreshSidebar();
        UserInputBox.Focus();
    }

    private void ViewTasksButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("View tasks");

    private void ActivityLogButton_Click(object sender, RoutedEventArgs e) =>
        RunQuickCommand("Show activity log");

    private void RefreshTasksButton_Click(object sender, RoutedEventArgs e) =>
        RefreshSidebar();

    private void TaskSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshSidebar();
    }

    private void TaskFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSidebar();
    }

    private void TaskSortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSidebar();
    }

    private void ExportTasksButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tasks = _chatBot.GetTasks();
            if (tasks.Count == 0)
            {
                AppendBotMessage("There are no tasks to export.", isWarning: true);
                return;
            }

            string fileName = $"MavicksTasks_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            var lines = new List<string> { "Id,Title,Description,Status,Reminder,Due,CreatedAt" };
            foreach (var task in tasks)
            {
                string safeTitle       = task.Title.Replace(",", " ").Replace("\n", " ");
                string safeDescription = task.Description.Replace(",", " ").Replace("\n", " ");
                lines.Add($"{task.Id},\"{safeTitle}\",\"{safeDescription}\",{task.StatusDisplay},\"{task.ReminderDisplay}\",\"{task.DueDisplay}\",{task.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            System.IO.File.WriteAllLines(path, lines);
            AppendBotMessage($"Tasks exported to your desktop as {fileName}.");
        }
        catch (Exception ex)
        {
            AppendBotMessage($"Could not export tasks: {ex.Message}", isWarning: true);
        }
    }

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
        try
        {
            if (TasksDataGrid != null)
            {
                TasksDataGrid.ItemsSource = null;
                TasksDataGrid.ItemsSource = _chatBot.GetTasks();
            }

            if (QuizStatusTextBlock != null)
            {
                QuizStatusTextBlock.Text = _chatBot.GetQuizStatusText();
            }

            if (DbStatusTextBlock != null)
            {
                DbStatusTextBlock.Text = TaskDatabase.IsAvailable
                    ? "Database: connected ✓"
                    : $"Database: offline — {TaskDatabase.LastError ?? "unknown error"}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshSidebar failed: {ex.Message}");
        }
    }

    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        string title       = NewTaskTitleTextBox.Text.Trim();
        string description = NewTaskDescriptionTextBox.Text.Trim();
        DateTime? reminderDate = NewTaskReminderDatePicker.SelectedDate?.Date;

        if (string.IsNullOrWhiteSpace(title))
        {
            AppendBotMessage("⚠ Please enter a task title.", isWarning: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            AppendBotMessage("⚠ Please enter a task description.", isWarning: true);
            return;
        }

        if (!reminderDate.HasValue)
        {
            AppendBotMessage("⚠ Please choose a reminder date.", isWarning: true);
            return;
        }

        AppendUserMessage($"Add task - {title}: {description}");
        string response = _chatBot.AddTask(title, description, reminderDate, null);
        AppendBotMessage(response, isWarning: response.StartsWith("⚠", StringComparison.Ordinal));

        NewTaskTitleTextBox.Clear();
        NewTaskDescriptionTextBox.Clear();
        NewTaskReminderDatePicker.SelectedDate = null;
        RefreshSidebar();
    }

    private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksDataGrid.SelectedItem is CyberTask task)
        {
            AppendUserMessage($"Complete task {task.Id}");
            string response = _chatBot.CompleteTask(task.Id);
            AppendBotMessage(response, isWarning: response.StartsWith("⚠", StringComparison.Ordinal));
            RefreshSidebar();
        }
        else
        {
            AppendBotMessage("Please select a task first.", isWarning: true);
        }
    }

    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksDataGrid.SelectedItem is not CyberTask task)
        {
            AppendBotMessage("Please select a task first.", isWarning: true);
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to delete '{task.Title}'?", "Delete task", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
            return;

        AppendUserMessage($"Delete task {task.Id}");
        string response = _chatBot.DeleteTask(task.Id);
        AppendBotMessage(response, isWarning: response.StartsWith("⚠", StringComparison.Ordinal));
        RefreshSidebar();
    }

    private void EditTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksDataGrid.SelectedItem is CyberTask task)
        {
            // Trigger edit flow via chat bot; UI could be enhanced to open an editor.
            string command = $"edit task {task.Id}";
            AppendUserMessage(command);
            var response = _chatBot.ProcessMessage(command);
            AppendBotMessage(response.Message, isWarning: response.IsWarning);
            RefreshSidebar();
        }
        else
        {
            AppendBotMessage("Select a task from the list first, then click Edit.", isWarning: true);
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

    /// <summary>
    /// Appends a typing indicator message.
    /// </summary>
    private void AppendTypingIndicator()
    {
        var tb = new TextBlock
        {
            Text                = TypingAnimation.GetTypingIndicator(),
            Foreground          = BrushBotText,
            FontFamily          = new FontFamily("Consolas"),
            FontSize            = 12,
            Margin              = new Thickness(0, 8, 80, 8),
            TextWrapping        = TextWrapping.Wrap
        };

        ChatPanel.Children.Add(tb);
        ScrollToBottom();
    }

    /// <summary>
    /// Removes the last message from the chat panel.
    /// </summary>
    private void RemoveLastMessage()
    {
        if (ChatPanel.Children.Count > 0)
            ChatPanel.Children.RemoveAt(ChatPanel.Children.Count - 1);
    }

    /// <summary>
    /// Displays the home dashboard after login.
    /// </summary>
    private void ShowDashboard()
    {
        int userId = TaskDatabase.CurrentUserId;
        var profile = TaskDatabase.GetUserProfile(userId);
        
        if (profile == null)
        {
            AppendBotMessage("Dashboard data not available.", isWarning: true);
            return;
        }

        var tasks = _chatBot.GetTasks();
        int pendingTasks = tasks.Count(t => !t.IsCompleted);
        int completedTasks = tasks.Count(t => t.IsCompleted);
        var lastTopic = MemoryStore.FavouriteTopic ?? "General";

        string dashboard = DashboardManager.GenerateDashboard(profile, pendingTasks, completedTasks, profile.QuizAttempts, lastTopic);
        AppendBotMessage(dashboard, isWelcome: true);
    }

    /// <summary>
    /// Displays the statistics panel.
    /// </summary>
    private void ShowStatistics()
    {
        int userId = TaskDatabase.CurrentUserId;
        var profile = TaskDatabase.GetUserProfile(userId);

        if (profile == null)
        {
            AppendBotMessage("Statistics not available.", isWarning: true);
            return;
        }

        string stats = DashboardManager.GenerateStatisticsPanel(profile);
        AppendBotMessage(stats, isWelcome: true);
        AppendBotMessage(DashboardManager.GetDailyTip());
        AppendBotMessage(DashboardManager.GetDailyFact());
    }

    /// <summary>
    /// Displays the settings menu.
    /// </summary>
    private void ShowSettings()
    {
        int userId = TaskDatabase.CurrentUserId;
        var settings = TaskDatabase.GetOrCreateUserSettings(userId);

        if (settings == null)
        {
            AppendBotMessage("Settings not available.", isWarning: true);
            return;
        }

        AppendBotMessage(settings.DisplaySettings(), isWelcome: true);
    }

    /// <summary>
    /// Displays the conversation suggestions.
    /// </summary>
    private void ShowConversationSuggestions()
    {
        AppendBotMessage(DashboardManager.GetConversationSuggestions());
    }

    /// <summary>
    /// Displays the help page with available commands.
    /// </summary>
    private void ShowHelp()
    {
        string help = @"❓ Help — Available Commands
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Chat Commands:
  • start quiz         → Begin a new cybersecurity quiz
  • view tasks         → Display all your tasks
  • show activity log  → Show recent activities
  • show dashboard     → Display home dashboard
  • show statistics    → View detailed statistics
  • show settings      → Open settings menu
  • help               → Show this help menu
  • generate password  → Create a strong password
  • check password     → Analyse password strength

Topics to ask about:
  • password safety    → Learn about strong passwords
  • phishing           → Understanding phishing attacks
  • 2fa                → Two-factor authentication
  • privacy            → Online privacy tips
  • malware            → Malware protection

Task Management:
  • add task [title]   → Create a new task
  • complete task [id] → Mark task as done
  • delete task [id]   → Remove a task

Fun Commands:
  • generate password  → Create a strong password
  • check password     → Analyze password strength
  • random tip         → Get a cyber security tip
  • random fact        → Learn a cyber security fact

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

        AppendBotMessage(help, isWelcome: true);
    }

    /// <summary>
    /// Handles conversation suggestion button clicks.
    /// </summary>
    private void SuggestionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            string message = tag switch
            {
                "password" => "Tell me about password safety",
                "phishing" => "Explain phishing attacks",
                "privacy" => "Tell me about online privacy",
                "statistics" => "Show statistics",
                "help" => "Help",
                _ => tag
            };

            UserInputBox.Text = message;
            Send();
        }
    }
}

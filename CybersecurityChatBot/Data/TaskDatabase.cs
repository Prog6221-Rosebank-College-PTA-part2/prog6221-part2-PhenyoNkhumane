using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using MySqlConnector;

/// <summary>
/// MySQL persistence layer for users, tasks, quiz scores, and activity logs.
/// </summary>
public static class TaskDatabase
{
    private static string? _connectionString;
    private static bool _initialized;
    private static string? _lastError;
    private static int _currentUserId;
    private static bool _usingFallbackStorage;
    private static readonly object _fallbackLock = new();
    private static readonly List<FallbackUser> _fallbackUsers = new();
    private static readonly List<FallbackTask> _fallbackTasks = new();
    private static readonly List<FallbackActivity> _fallbackActivities = new();
    private static readonly Dictionary<int, FallbackSettings> _fallbackSettings = new();
    private static int _fallbackNextUserId = 1;
    private static int _fallbackNextTaskId = 1;
    private static int _fallbackNextActivityId = 1;
    private static readonly string _fallbackDataPath = Path.Combine(AppContext.BaseDirectory, "local_tasks.json");

    public static bool IsAvailable { get; private set; }
    public static bool IsFallbackMode { get; private set; }
    public static string? LastError => _lastError;
    public static int CurrentUserId => _currentUserId;

    public static void Initialize()
    {
        if (_initialized)
            return;

        _connectionString = LoadConnectionString();
        _initialized = true;

        try
        {
            EnsureSchema();
            IsAvailable = true;
            IsFallbackMode = false;
            _usingFallbackStorage = false;
            _lastError = null;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            IsFallbackMode = true;
            _usingFallbackStorage = true;
            _lastError = $"MySQL unavailable: {ex.Message}. Using local fallback storage.";
            LoadFallbackData();
            EnsureFallbackSeeded();
            _currentUserId = _fallbackUsers.FirstOrDefault()?.Id ?? 1;
        }
    }

    public static int AddOrGetUser(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        EnsureReady();
        if (!IsAvailable)
            return 0;

        using var conn = OpenConnection();
        using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT IGNORE INTO users (name)
            VALUES (@name);";
        insertCmd.Parameters.AddWithValue("@name", name.Trim());
        insertCmd.ExecuteNonQuery();

        using var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = @"
            SELECT id FROM users WHERE name = @name LIMIT 1;";
        selectCmd.Parameters.AddWithValue("@name", name.Trim());

        object result = selectCmd.ExecuteScalar() ?? 0;
        _currentUserId = Convert.ToInt32(result);
        return _currentUserId;
    }

    public static int AddTask(string title, string description, DateTime? reminderDate, DateTime? dueDate)
    {
        EnsureReady();
        if (!IsAvailable && !_usingFallbackStorage)
            throw new InvalidOperationException("Task database is not available.");

        if (_usingFallbackStorage)
        {
            lock (_fallbackLock)
            {
                EnsureFallbackSeeded();
                var task = new FallbackTask
                {
                    Id = _fallbackNextTaskId++,
                    UserId = _currentUserId,
                    Title = title.Trim(),
                    Description = description.Trim(),
                    ReminderDate = reminderDate,
                    DueDate = dueDate,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                };
                _fallbackTasks.Add(task);
                SaveFallbackData();
                return task.Id;
            }
        }

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO tasks (user_id, title, description, reminder_date, due_date, is_completed)
            VALUES (@userId, @title, @description, @reminderDate, @dueDate, 0);
            SELECT LAST_INSERT_ID();";
        cmd.Parameters.AddWithValue("@userId", _currentUserId);
        cmd.Parameters.AddWithValue("@title", title.Trim());
        cmd.Parameters.AddWithValue("@description", description.Trim());
        cmd.Parameters.AddWithValue("@reminderDate", reminderDate.HasValue ? reminderDate.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@dueDate", dueDate.HasValue ? dueDate.Value : DBNull.Value);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static bool TaskExists(string title)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM tasks
            WHERE user_id = @userId AND title = @title LIMIT 1;";
        cmd.Parameters.AddWithValue("@userId", _currentUserId);
        cmd.Parameters.AddWithValue("@title", title.Trim());

        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public static List<CyberTask> GetAllTasks(string? search = null, bool? completedFilter = null, string? sortColumn = null, bool ascending = true)
    {
        EnsureReady();
        var tasks = new List<CyberTask>();
        if (!IsAvailable && !_usingFallbackStorage)
            return tasks;

        if (_usingFallbackStorage)
        {
            lock (_fallbackLock)
            {
                EnsureFallbackSeeded();
                tasks.AddRange(_fallbackTasks
                    .Where(t => t.UserId == _currentUserId)
                    .Where(t => string.IsNullOrWhiteSpace(search) || t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || t.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .Where(t => !completedFilter.HasValue || t.IsCompleted == completedFilter.Value)
                    .Select(t => new CyberTask
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        ReminderDate = t.ReminderDate,
                        DueDate = t.DueDate,
                        IsCompleted = t.IsCompleted,
                        CreatedAt = t.CreatedAt
                    }));
            }

            return completedFilter.HasValue || !string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(sortColumn)
                ? tasks.OrderByDescending(t => t.CreatedAt).ToList()
                : tasks;
        }

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        var whereClauses = new List<string> { "user_id = @userId" };
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClauses.Add("(title LIKE @search OR description LIKE @search)");
            cmd.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
        }

        if (completedFilter.HasValue)
        {
            whereClauses.Add("is_completed = @completed");
            cmd.Parameters.AddWithValue("@completed", completedFilter.Value ? 1 : 0);
        }

        string orderBy = sortColumn switch
        {
            "due_date" => "due_date",
            "title" => "title",
            _ => "created_at"
        };

        string sortDirection = ascending ? "ASC" : "DESC";
        cmd.CommandText = $@"
            SELECT id, title, description, reminder_date, due_date, is_completed, created_at
            FROM tasks
            WHERE {string.Join(" AND ", whereClauses)}
            ORDER BY {orderBy} {sortDirection};";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new CyberTask
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
                DueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return tasks;
    }

    public static CyberTask? GetTaskById(int id)
    {
        EnsureReady();
        if (!IsAvailable)
            return null;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, title, description, reminder_date, due_date, is_completed, created_at
            FROM tasks
            WHERE id = @id AND user_id = @userId
            LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new CyberTask
        {
            Id = reader.GetInt32("id"),
            Title = reader.GetString("title"),
            Description = reader.GetString("description"),
            ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
            DueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date"),
            IsCompleted = reader.GetBoolean("is_completed"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }

    public static bool EditTask(int id, string? title, string? description, DateTime? reminderDate, DateTime? dueDate)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        if (title == null && description == null && reminderDate == null && dueDate == null)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        var assignments = new List<string>();
        if (title != null)
        {
            assignments.Add("title = @title");
            cmd.Parameters.AddWithValue("@title", title.Trim());
        }

        if (description != null)
        {
            assignments.Add("description = @description");
            cmd.Parameters.AddWithValue("@description", description.Trim());
        }

        if (reminderDate != null)
        {
            assignments.Add("reminder_date = @reminderDate");
            cmd.Parameters.AddWithValue("@reminderDate", reminderDate.Value);
        }

        if (dueDate != null)
        {
            assignments.Add("due_date = @dueDate");
            cmd.Parameters.AddWithValue("@dueDate", dueDate.Value);
        }

        cmd.CommandText = $@"
            UPDATE tasks SET {string.Join(", ", assignments)}
            WHERE id = @id AND user_id = @userId;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool DeleteTask(int id)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM tasks
            WHERE id = @id AND user_id = @userId;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool MarkCompleted(int id)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE tasks SET is_completed = 1
            WHERE id = @id AND user_id = @userId;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool SetReminder(int id, DateTime reminderDate)
    {
        return EditTask(id, null, null, reminderDate, null);
    }

    public static bool SetDueDate(int id, DateTime dueDate)
    {
        return EditTask(id, null, null, null, dueDate);
    }

    public static bool LogActivity(string description)
    {
        EnsureReady();
        if (!IsAvailable && !_usingFallbackStorage)
            return false;

        if (_usingFallbackStorage)
        {
            lock (_fallbackLock)
            {
                EnsureFallbackSeeded();
                _fallbackActivities.Add(new FallbackActivity
                {
                    Id = _fallbackNextActivityId++,
                    UserId = _currentUserId,
                    Description = description.Trim(),
                    Timestamp = DateTime.Now
                });
                SaveFallbackData();
                return true;
            }
        }

        if (!IsAvailable || _currentUserId == 0)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO activity_logs (user_id, description)
            VALUES (@userId, @description);";
        cmd.Parameters.AddWithValue("@userId", _currentUserId);
        cmd.Parameters.AddWithValue("@description", description.Trim());

        return cmd.ExecuteNonQuery() > 0;
    }

    public static List<ActivityEntry> GetRecentActivities(int count = 12)
    {
        EnsureReady();
        var entries = new List<ActivityEntry>();
        if (!IsAvailable && !_usingFallbackStorage)
            return entries;

        if (_usingFallbackStorage)
        {
            lock (_fallbackLock)
            {
                EnsureFallbackSeeded();
                entries.AddRange(_fallbackActivities
                    .Where(a => a.UserId == _currentUserId)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .Select(a => new ActivityEntry(a.Timestamp, a.Description)));
            }

            return entries;
        }

        if (!IsAvailable || _currentUserId == 0)
            return entries;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT timestamp, description
            FROM activity_logs
            WHERE user_id = @userId
            ORDER BY timestamp DESC
            LIMIT @count;";
        cmd.Parameters.AddWithValue("@userId", _currentUserId);
        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(new ActivityEntry(
                reader.GetDateTime("timestamp"),
                reader.GetString("description")));
        }

        return entries;
    }

    public static int RecordQuizScore(string userName, int score, int totalQuestions)
    {
        if (string.IsNullOrWhiteSpace(userName))
            userName = "Guest";

        EnsureReady();
        if (!IsAvailable && !_usingFallbackStorage)
            return 0;

        int userId = AddOrGetUser(userName);

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO quiz_scores (user_id, score, total_questions)
            VALUES (@userId, @score, @totalQuestions);";
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@totalQuestions", totalQuestions);

        cmd.ExecuteNonQuery();
        return score;
    }

    public static int GetHighScore()
    {
        EnsureReady();
        if (!IsAvailable)
            return 0;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT MAX(score) FROM quiz_scores
            WHERE user_id = @userId;";
        cmd.Parameters.AddWithValue("@userId", _currentUserId);

        object result = cmd.ExecuteScalar() ?? 0;
        return Convert.ToInt32(result);
    }

    public static UserProfile? GetUserProfile(int userId)
    {
        EnsureReady();
        if (!IsAvailable)
            return null;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, name, favourite_topic, quiz_best_score, quiz_attempts, 
                   quiz_average_score, total_tasks, completed_tasks, last_login, created_at
            FROM users
            WHERE id = @userId;";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new UserProfile
        {
            UserId = reader.GetInt32("id"),
            Name = reader.GetString("name"),
            FavouriteTopic = reader.IsDBNull(reader.GetOrdinal("favourite_topic")) ? null : reader.GetString("favourite_topic"),
            QuizBestScore = reader.GetInt32("quiz_best_score"),
            QuizAttempts = reader.GetInt32("quiz_attempts"),
            QuizAverageScore = reader.GetInt32("quiz_average_score"),
            TotalTasks = reader.GetInt32("total_tasks"),
            CompletedTasks = reader.GetInt32("completed_tasks"),
            LastLogin = reader.GetDateTime("last_login"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }

    public static bool UpdateUserStatistics(int userId, string? favouriteTopic = null, int? quizScore = null, int? totalTasks = null, int? completedTasks = null)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        var updates = new List<string>();

        if (!string.IsNullOrEmpty(favouriteTopic))
        {
            updates.Add("favourite_topic = @favouriteTopic");
            cmd.Parameters.AddWithValue("@favouriteTopic", favouriteTopic);
        }

        if (quizScore.HasValue)
        {
            updates.Add("quiz_best_score = GREATEST(quiz_best_score, @quizScore)");
            updates.Add("quiz_attempts = quiz_attempts + 1");
            cmd.Parameters.AddWithValue("@quizScore", quizScore.Value);
        }

        if (totalTasks.HasValue)
        {
            updates.Add("total_tasks = @totalTasks");
            cmd.Parameters.AddWithValue("@totalTasks", totalTasks.Value);
        }

        if (completedTasks.HasValue)
        {
            updates.Add("completed_tasks = @completedTasks");
            cmd.Parameters.AddWithValue("@completedTasks", completedTasks.Value);
        }

        updates.Add("last_login = CURRENT_TIMESTAMP");

        cmd.CommandText = $@"
            UPDATE users SET {string.Join(", ", updates)}
            WHERE id = @userId;";
        cmd.Parameters.AddWithValue("@userId", userId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static AppSettings? GetOrCreateUserSettings(int userId)
    {
        EnsureReady();
        if (!IsAvailable && !_usingFallbackStorage)
            return null;

        if (_usingFallbackStorage)
        {
            lock (_fallbackLock)
            {
                var user = _fallbackUsers.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    return null;

                if (!_fallbackSettings.ContainsKey(userId))
                {
                    _fallbackSettings[userId] = new FallbackSettings
                    {
                        UserId = userId,
                        DarkMode = true,
                        EnableSounds = true,
                        VoiceGreeting = true
                    };
                    SaveFallbackData();
                }

                var settings = _fallbackSettings[userId];
                return new AppSettings
                {
                    UserId = settings.UserId,
                    DarkMode = settings.DarkMode,
                    EnableSounds = settings.EnableSounds,
                    VoiceGreeting = settings.VoiceGreeting
                };
            }
        }

        using var conn = OpenConnection();
        
        // Try to get existing settings
        using var selectCmd = conn.CreateCommand();
        selectCmd.CommandText = @"
            SELECT user_id, dark_mode, enable_sounds, voice_greeting
            FROM user_settings
            WHERE user_id = @userId;";
        selectCmd.Parameters.AddWithValue("@userId", userId);

        using var reader = selectCmd.ExecuteReader();
        if (reader.Read())
        {
            return new AppSettings
            {
                UserId = reader.GetInt32("user_id"),
                DarkMode = reader.GetBoolean("dark_mode"),
                EnableSounds = reader.GetBoolean("enable_sounds"),
                VoiceGreeting = reader.GetBoolean("voice_greeting")
            };
        }

        // Create default settings if they don't exist
        using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT IGNORE INTO user_settings (user_id, dark_mode, enable_sounds, voice_greeting)
            VALUES (@userId, 1, 1, 1);";
        insertCmd.Parameters.AddWithValue("@userId", userId);
        insertCmd.ExecuteNonQuery();

        return new AppSettings
        {
            UserId = userId,
            DarkMode = true,
            EnableSounds = true,
            VoiceGreeting = true
        };
    }

    public static bool UpdateUserSettings(int userId, AppSettings settings)
    {
        EnsureReady();
        if (!IsAvailable)
            return false;

        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE user_settings
            SET dark_mode = @darkMode,
                enable_sounds = @enableSounds,
                voice_greeting = @voiceGreeting
            WHERE user_id = @userId;";
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@darkMode", settings.DarkMode ? 1 : 0);
        cmd.Parameters.AddWithValue("@enableSounds", settings.EnableSounds ? 1 : 0);
        cmd.Parameters.AddWithValue("@voiceGreeting", settings.VoiceGreeting ? 1 : 0);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static void LoadFallbackData()
    {
        lock (_fallbackLock)
        {
            if (!File.Exists(_fallbackDataPath))
                return;

            try
            {
                string json = File.ReadAllText(_fallbackDataPath);
                var store = JsonSerializer.Deserialize<FallbackStore>(json);
                if (store == null)
                    return;

                _fallbackUsers.Clear();
                _fallbackTasks.Clear();
                _fallbackActivities.Clear();
                _fallbackSettings.Clear();

                if (store.Users != null)
                    _fallbackUsers.AddRange(store.Users);
                if (store.Tasks != null)
                    _fallbackTasks.AddRange(store.Tasks);
                if (store.Activities != null)
                    _fallbackActivities.AddRange(store.Activities);
                if (store.Settings != null)
                    foreach (var item in store.Settings)
                        _fallbackSettings[item.UserId] = item;

                _fallbackNextUserId = _fallbackUsers.Count > 0 ? _fallbackUsers.Max(u => u.Id) + 1 : 1;
                _fallbackNextTaskId = _fallbackTasks.Count > 0 ? _fallbackTasks.Max(t => t.Id) + 1 : 1;
                _fallbackNextActivityId = _fallbackActivities.Count > 0 ? _fallbackActivities.Max(a => a.Id) + 1 : 1;
            }
            catch
            {
                _fallbackUsers.Clear();
                _fallbackTasks.Clear();
                _fallbackActivities.Clear();
                _fallbackSettings.Clear();
            }
        }
    }

    private static void SaveFallbackData()
    {
        lock (_fallbackLock)
        {
            var store = new FallbackStore
            {
                Users = _fallbackUsers.ToList(),
                Tasks = _fallbackTasks.ToList(),
                Activities = _fallbackActivities.ToList(),
                Settings = _fallbackSettings.Values.ToList()
            };

            string json = JsonSerializer.Serialize(store, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_fallbackDataPath, json);
        }
    }

    private static void EnsureFallbackSeeded()
    {
        lock (_fallbackLock)
        {
            if (_fallbackUsers.Count == 0)
            {
                _fallbackUsers.Add(new FallbackUser
                {
                    Id = _fallbackNextUserId++,
                    Name = "Guest",
                    CreatedAt = DateTime.Now,
                    LastLogin = DateTime.Now
                });
                _currentUserId = 1;
                SaveFallbackData();
            }
        }
    }

    private static void EnsureSchema()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(100) NOT NULL UNIQUE,
                favourite_topic VARCHAR(100) NULL,
                quiz_best_score INT NOT NULL DEFAULT 0,
                quiz_attempts INT NOT NULL DEFAULT 0,
                quiz_average_score INT NOT NULL DEFAULT 0,
                total_tasks INT NOT NULL DEFAULT 0,
                completed_tasks INT NOT NULL DEFAULT 0,
                last_login DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            ALTER TABLE users
            ADD COLUMN IF NOT EXISTS favourite_topic VARCHAR(100) NULL,
            ADD COLUMN IF NOT EXISTS quiz_best_score INT NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS quiz_attempts INT NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS quiz_average_score INT NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS total_tasks INT NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS completed_tasks INT NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS last_login DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS tasks (
                id INT AUTO_INCREMENT PRIMARY KEY,
                user_id INT NOT NULL,
                title VARCHAR(255) NOT NULL,
                description TEXT NOT NULL,
                reminder_date DATETIME NULL,
                due_date DATETIME NULL,
                is_completed TINYINT(1) NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS quiz_scores (
                id INT AUTO_INCREMENT PRIMARY KEY,
                user_id INT NOT NULL,
                score INT NOT NULL,
                total_questions INT NOT NULL,
                taken_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS activity_logs (
                id INT AUTO_INCREMENT PRIMARY KEY,
                user_id INT NOT NULL,
                description TEXT NOT NULL,
                timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS user_settings (
                user_id INT NOT NULL PRIMARY KEY,
                dark_mode TINYINT(1) NOT NULL DEFAULT 1,
                enable_sounds TINYINT(1) NOT NULL DEFAULT 1,
                voice_greeting TINYINT(1) NOT NULL DEFAULT 1,
                updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );";
        cmd.ExecuteNonQuery();
    }

    private static void EnsureReady()
    {
        if (!_initialized)
            Initialize();

        if (!IsAvailable)
            return;
    }

    private static MySqlConnection OpenConnection()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private sealed class FallbackStore
    {
        public List<FallbackUser> Users { get; set; } = new();
        public List<FallbackTask> Tasks { get; set; } = new();
        public List<FallbackActivity> Activities { get; set; } = new();
        public List<FallbackSettings> Settings { get; set; } = new();
    }

    private sealed class FallbackUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Guest";
        public string? FavouriteTopic { get; set; }
        public int BestQuizScore { get; set; }
        public int QuizAttempts { get; set; }
        public int QuizAverageScore { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class FallbackTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ReminderDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class FallbackActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    private sealed class FallbackSettings
    {
        public int UserId { get; set; }
        public bool DarkMode { get; set; } = true;
        public bool EnableSounds { get; set; } = true;
        public bool VoiceGreeting { get; set; } = true;
    }

    private static string LoadConnectionString()
    {
        string? env = Environment.GetEnvironmentVariable("MAVICKS_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        string configPath = Path.Combine(AppContext.BaseDirectory, "dbconfig.json");
        if (File.Exists(configPath))
        {
            using var stream = File.OpenRead(configPath);
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionString", out var value))
                {
                    string? cs = value.GetString();
                    if (!string.IsNullOrWhiteSpace(cs))
                        return cs;
                }
            }
            catch
            {
                // Fall back to default connection string.
            }
        }

        return "Server=localhost;Port=3306;Database=mavicks_tasks;User=root;Password=;SslMode=none;";
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

    public static bool IsAvailable { get; private set; }
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
            _lastError = null;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            _lastError = ex.Message;
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
        if (!IsAvailable)
            throw new InvalidOperationException("Task database is not available.");

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
        if (!IsAvailable)
            return tasks;

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
        if (!IsAvailable)
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

    private static void EnsureSchema()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                id INT AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(100) NOT NULL UNIQUE,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MySqlConnector;

/// <summary>
/// Persistence layer for cybersecurity tasks.
/// Uses MySQL when available; falls back to a local JSON file when the database is not accessible.
/// </summary>
public static class TaskDatabase
{
    private static string? _connectionString;
    private static bool _initialized;
    private static string? _lastError;

    private const string LocalFallbackFileName = "local_tasks.json";

    public static bool IsAvailable { get; private set; }
    public static bool IsLocalFallback => !_initialized || !IsAvailable;
    public static string? LastError => _lastError;

    private static string LocalFallbackPath => Path.Combine(AppContext.BaseDirectory, LocalFallbackFileName);

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

        if (!IsAvailable)
        {
            // Ensure a local fallback store exists so the app can continue.
            SaveLocalTasks(new List<CyberTask>());
        }
    }

    public static int AddTask(string title, string description, DateTime? reminderDate)
    {
        EnsureReady();

        if (IsAvailable)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO tasks (title, description, reminder_date, is_completed)
                VALUES (@title, @description, @reminderDate, 0);
                SELECT LAST_INSERT_ID();
                """;
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@reminderDate", reminderDate.HasValue ? reminderDate.Value : DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        return AddTaskLocal(title, description, reminderDate);
    }

    public static List<CyberTask> GetAllTasks()
    {
        EnsureReady();

        if (IsAvailable)
        {
            var tasks = new List<CyberTask>();
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, title, description, reminder_date, is_completed, created_at
                FROM tasks
                ORDER BY is_completed ASC, created_at DESC;
                """;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new CyberTask
                {
                    Id = reader.GetInt32("id"),
                    Title = reader.GetString("title"),
                    Description = reader.GetString("description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                        ? null
                        : reader.GetDateTime("reminder_date"),
                    IsCompleted = reader.GetBoolean("is_completed"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }

            return tasks;
        }

        return LoadLocalTasks();
    }

    public static bool DeleteTask(int id)
    {
        EnsureReady();

        if (IsAvailable)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM tasks WHERE id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        return DeleteTaskLocal(id);
    }

    public static bool MarkCompleted(int id)
    {
        EnsureReady();

        if (IsAvailable)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE tasks SET is_completed = 1 WHERE id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        return UpdateLocalTask(id, task => task.IsCompleted = true);
    }

    public static bool SetReminder(int id, DateTime reminderDate)
    {
        EnsureReady();

        if (IsAvailable)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE tasks SET reminder_date = @date WHERE id = @id;";
            cmd.Parameters.AddWithValue("@date", reminderDate);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        return UpdateLocalTask(id, task => task.ReminderDate = reminderDate);
    }

    public static CyberTask? GetTaskById(int id)
    {
        EnsureReady();

        if (IsAvailable)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT id, title, description, reminder_date, is_completed, created_at
                FROM tasks WHERE id = @id LIMIT 1;
                """;
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new CyberTask
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                    ? null
                    : reader.GetDateTime("reminder_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            };
        }

        return LoadLocalTasks().FirstOrDefault(task => task.Id == id);
    }

    private static void EnsureSchema()
    {
        using var conn = OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS tasks (
                id INT AUTO_INCREMENT PRIMARY KEY,
                title VARCHAR(255) NOT NULL,
                description TEXT NOT NULL,
                reminder_date DATETIME NULL,
                is_completed TINYINT(1) NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static void EnsureReady()
    {
        if (!_initialized)
            Initialize();

        // Allow operations to continue with the local fallback when the
        // database is unavailable.
        if (!IsAvailable)
            return;
    }

    private static MySqlConnection OpenConnection()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private static int AddTaskLocal(string title, string description, DateTime? reminderDate)
    {
        List<CyberTask> tasks = LoadLocalTasks();
        int newId = tasks.Count == 0 ? 1 : tasks.Max(t => t.Id) + 1;
        var task = new CyberTask
        {
            Id = newId,
            Title = title,
            Description = description,
            ReminderDate = reminderDate,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        tasks.Insert(0, task);
        SaveLocalTasks(tasks);
        return newId;
    }

    private static List<CyberTask> LoadLocalTasks()
    {
        try
        {
            if (!File.Exists(LocalFallbackPath))
                return new List<CyberTask>();

            string json = File.ReadAllText(LocalFallbackPath);
            var tasks = JsonSerializer.Deserialize<List<CyberTask>>(json);
            return tasks ?? new List<CyberTask>();
        }
        catch
        {
            return new List<CyberTask>();
        }
    }

    private static void SaveLocalTasks(List<CyberTask> tasks)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(LocalFallbackPath, json);
        }
        catch
        {
            // If local persistence fails, there is nothing else we can do.
        }
    }

    private static bool DeleteTaskLocal(int id)
    {
        var tasks = LoadLocalTasks();
        int removed = tasks.RemoveAll(task => task.Id == id);
        if (removed > 0)
            SaveLocalTasks(tasks);
        return removed > 0;
    }

    private static bool UpdateLocalTask(int id, Action<CyberTask> update)
    {
        var tasks = LoadLocalTasks();
        CyberTask? task = tasks.FirstOrDefault(t => t.Id == id);
        if (task == null)
            return false;

        update(task);
        SaveLocalTasks(tasks);
        return true;
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
            var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("ConnectionString", out JsonElement value))
            {
                string? cs = value.GetString();
                if (!string.IsNullOrWhiteSpace(cs))
                    return cs;
            }
        }

        return "Server=localhost;Port=3306;Database=mavicks_tasks;User=root;Password=;SslMode=none;";
    }
}

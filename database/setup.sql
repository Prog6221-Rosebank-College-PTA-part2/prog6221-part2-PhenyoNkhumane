-- Run this script in MySQL to create the database for Mavicks Task Assistant.
-- Example: mysql -u root -p < database/setup.sql

CREATE DATABASE IF NOT EXISTS mavicks_tasks;
USE mavicks_tasks;

-- Users table with profile and statistics
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    favourite_topic VARCHAR(255) NULL,
    quiz_best_score INT NOT NULL DEFAULT 0,
    quiz_attempts INT NOT NULL DEFAULT 0,
    quiz_average_score INT NOT NULL DEFAULT 0,
    total_tasks INT NOT NULL DEFAULT 0,
    completed_tasks INT NOT NULL DEFAULT 0,
    last_login DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Tasks table with user relationship
CREATE TABLE IF NOT EXISTS tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    reminder_date DATETIME NULL,
    due_date DATETIME NULL,
    is_completed TINYINT(1) NOT NULL DEFAULT 0,
    category VARCHAR(100) NULL DEFAULT 'General',
    priority VARCHAR(50) NULL DEFAULT 'Medium',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at DATETIME NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Activity log for tracking user actions
CREATE TABLE IF NOT EXISTS activity_log (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    action VARCHAR(255) NOT NULL,
    description TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Quiz history for progress tracking
CREATE TABLE IF NOT EXISTS quiz_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    score INT NOT NULL,
    total_questions INT NOT NULL DEFAULT 12,
    completed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Application settings per user
CREATE TABLE IF NOT EXISTS user_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    dark_mode TINYINT(1) NOT NULL DEFAULT 1,
    enable_sounds TINYINT(1) NOT NULL DEFAULT 1,
    voice_greeting TINYINT(1) NOT NULL DEFAULT 1,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Conversation memory for context tracking
CREATE TABLE IF NOT EXISTS conversation_memory (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    session_date DATE NOT NULL,
    topics_discussed TEXT,
    messages_count INT NOT NULL DEFAULT 0,
    last_topic VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

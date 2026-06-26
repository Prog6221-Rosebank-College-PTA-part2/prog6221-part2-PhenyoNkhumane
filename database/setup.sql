-- Run this script in MySQL to create the database for Mavicks Task Assistant.
-- Example: mysql -u root -p < database/setup.sql

CREATE DATABASE IF NOT EXISTS mavicks_tasks;
USE mavicks_tasks;

CREATE TABLE IF NOT EXISTS tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    reminder_date DATETIME NULL,
    is_completed TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

# Mavicks Cybersecurity Chatbot

## Project name

Mavicks Cybersecurity Chatbot

## Brief description

Mavicks is a Windows desktop chatbot built with .NET 10 and WPF. It helps users learn cybersecurity best practices through conversation, manages security-related tasks, offers a quiz mini-game, and tracks actions in an activity log.

## How to open and run the project

1. Open `prog6221-part2-PhenyoNkhumane.sln` in Visual Studio 2022/2023 or Visual Studio Code with .NET support.
2. Restore NuGet packages.
3. Build the solution.
4. Run the `prog6221-part2-PhenyoNkhumane` project.

Alternative command line:

```powershell
cd c:\Users\pheny\Videos\prog6221-part2-PhenyoNkhumane
dotnet build prog6221-part2-PhenyoNkhumane.sln
dotnet run --project prog6221-part2-PhenyoNkhumane.csproj
```

## Software required

- Windows
- .NET 10 SDK / .NET 10 runtime
- MySQL server (local or remote)
- Visual Studio or VS Code for editing and running

## Database setup instructions

The app stores cybersecurity tasks in a MySQL database.

1. Ensure MySQL is installed and running.
2. Create the database and table using the included script:

```powershell
mysql -u root -p < database/setup.sql
```

3. Verify or update `dbconfig.json` in the project root with your connection string.

Example default connection string:

```json
{
  "ConnectionString": "Server=localhost;Port=3306;Database=mavicks_tasks;User=root;Password=;SslMode=none;"
}
```

4. You can also override the connection string with the environment variable `MAVICKS_DB_CONNECTION`.

The app will auto-create the `tasks` table if it is missing.

## How to use the Task Assistant

Use natural commands in the chat input to manage cybersecurity tasks.

Examples:

- `Add task - Enable two-factor authentication`
- `View tasks`
- `Complete task 1`
- `Delete task 2`
- `Remind me to update my password tomorrow`

Task-related phrases are recognised by the app using NLP-style intent detection, so you can usually say the same thing in several different ways.

## How to access the Quiz / Mini-Game

Start the quiz by entering a quiz command or clicking the `Start quiz` button.

Supported commands:

- `Start quiz`
- `Play quiz`
- `Begin quiz`
- `Mini game`

Answer quiz questions using:

- `A`, `B`, `C`, `D`
- `1`, `2`, `3`, `4`
- `True` / `False` for true/false questions

The quiz status is shown in the sidebar while the game is active.

## How to test the NLP Simulation

The chatbot uses keyword-based intent detection for commands such as:

- `add task`
- `view tasks`
- `delete task`
- `complete task`
- `remind me to ...`
- `start quiz`
- `show activity log`

Try mixed phrasing like:

- `Show my tasks`
- `Set a reminder to change my password tomorrow`
- `Play the quiz`
- `What have you done?`

This demonstrates the app's NLP simulation and intent routing.

## How to view the Activity Log

View the logged actions by typing or clicking the activity command:

- `Show activity log`
- `Show more activity log` to reveal older entries

The activity log records key events such as task creation, reminders, quiz start/completion, and other user actions.

## Login details / important notes

- No login credentials are required.
- The initial name prompt is only for personalization and chat memory.
- If the database connection fails, task persistence may not be available. Check `dbconfig.json` or set `MAVICKS_DB_CONNECTION`.
- The UI is a WPF app targeting `net10.0-windows`.

## Video presentation link

No video presentation link is included in this repository.

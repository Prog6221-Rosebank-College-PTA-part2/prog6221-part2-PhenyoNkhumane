using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class ConsoleUi
{
    private const string Divider = "===========================================";
    private const int DefaultTypingDelayMs = 8;

    public static void WriteDivider(ConsoleColor color = ConsoleColor.DarkGray)
    {
        WriteLineColored(Divider, color);
    }

    public static void WriteSectionHeader(string title, ConsoleColor color = ConsoleColor.Cyan)
    {
        WriteDivider(color);
        WriteLineColored($" {title}", color);
        WriteDivider(color);
    }

    public static void WriteBoxedLines(IEnumerable<string> lines, ConsoleColor color)
    {
        List<string> lineList = lines.ToList();
        if (lineList.Count == 0)
        {
            return;
        }

        int innerWidth = lineList.Max(l => l.Length);
        string top = "╔" + new string('═', innerWidth + 2) + "╗";
        string bottom = "╚" + new string('═', innerWidth + 2) + "╝";
        IEnumerable<string> body = lineList.Select(l => "║ " + l.PadRight(innerWidth) + " ║");

        WriteLineColored(top, color);
        foreach (string row in body)
        {
            WriteLineColored(row, color);
        }

        WriteLineColored(bottom, color);
    }

    public static void WriteLineColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteTextColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void WriteBotLine(string message, bool typingEffect = true)
    {
        WriteTextColored("Bot > ", ConsoleColor.Green);
        if (typingEffect)
        {
            TypeLine(message, ConsoleColor.Green, DefaultTypingDelayMs);
        }
        else
        {
            WriteLineColored(message, ConsoleColor.Green);
        }
    }

    public static void WriteUserPrompt(string userName)
    {
        WriteTextColored($"{userName} > ", ConsoleColor.Yellow);
    }

    public static void TypeLine(string message, ConsoleColor color, int delayMsPerCharacter = 10)
    {
        Console.ForegroundColor = color;
        foreach (char character in message)
        {
            Console.Write(character);
            if (!char.IsWhiteSpace(character))
            {
                Thread.Sleep(delayMsPerCharacter);
            }
        }

        Console.WriteLine();
        Console.ResetColor();
    }

    public static void WriteWarningLine(string message)
    {
        WriteLineColored($"! {message}", ConsoleColor.DarkYellow);
    }

    public static void WriteErrorLine(string message)
    {
        WriteLineColored($"x {message}", ConsoleColor.Red);
    }

    public static void WriteExchangeSpacer()
    {
        Console.WriteLine();
        WriteDivider();
        Console.WriteLine();
    }
}

using NLog;
using NLog.Config;
using NLog.Targets;

namespace PckTool.Core;

/// <summary>
///     Global logger for the application.
/// </summary>
public static class Log
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    static Log()
    {
        var config = new LoggingConfiguration();

        var consoleTarget = new ColoredConsoleTarget("console")
        {
            Layout =
                "${longdate} [${level:uppercase=true}] ${message}${onexception:${newline}${exception:format=tostring}}"
        };

        // Configure colors for each log level
        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Trace", ForegroundColor = ConsoleOutputColor.DarkGray
            });

        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Debug", ForegroundColor = ConsoleOutputColor.Gray
            });

        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Info", ForegroundColor = ConsoleOutputColor.White
            });

        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Warn", ForegroundColor = ConsoleOutputColor.Yellow
            });

        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Error", ForegroundColor = ConsoleOutputColor.Red
            });

        consoleTarget.RowHighlightingRules.Add(
            new ConsoleRowHighlightingRule
            {
                Condition = "level == LogLevel.Fatal",
                ForegroundColor = ConsoleOutputColor.Red,
                BackgroundColor = ConsoleOutputColor.White
            });

        config.AddTarget(consoleTarget);
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);

        LogManager.Configuration = config;
    }

    public static void Trace(string message)
    {
        Logger.Trace(message);
    }

    public static void Trace(string message, params object[] args)
    {
        Logger.Trace(message, args);
    }

    public static void Debug(string message)
    {
        Logger.Debug(message);
    }

    public static void Debug(string message, params object[] args)
    {
        Logger.Debug(message, args);
    }

    public static void Info(string message)
    {
        Logger.Info(message);
    }

    public static void Info(string message, params object[] args)
    {
        Logger.Info(message, args);
    }

    public static void Warn(string message)
    {
        Logger.Warn(message);
    }

    public static void Warn(string message, params object[] args)
    {
        Logger.Warn(message, args);
    }

    public static void Error(string message)
    {
        Logger.Error(message);
    }

    public static void Error(string message, params object[] args)
    {
        Logger.Error(message, args);
    }

    public static void Error(Exception ex, string message)
    {
        Logger.Error(ex, message);
    }

    public static void Fatal(string message)
    {
        Logger.Fatal(message);
    }

    public static void Fatal(string message, params object[] args)
    {
        Logger.Fatal(message, args);
    }

    public static void Fatal(Exception ex, string message)
    {
        Logger.Fatal(ex, message);
    }
}

using System;

namespace TetriON.Shared.Utilities;

/// <summary>
/// Logging utility for debugging and monitoring
/// </summary>
public static class Logger {

    public static void Log(string message, LogLevel level = LogLevel.Info) {
        var prefix = level switch {
            LogLevel.Debug => "[DEBUG]",
            LogLevel.Info => "[INFO]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Error => "[ERROR]",
            _ => "[LOG]"
        };

        Console.WriteLine($"{prefix} {message}");
    }

    public static void LogException(Exception ex) {
        Log($"Exception: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
    }

    public static void DebugLog(string message) {
        Log(message, LogLevel.Debug);
    }

    public enum LogLevel {
        Debug,
        Info,
        Warning,
        Error
    }
}

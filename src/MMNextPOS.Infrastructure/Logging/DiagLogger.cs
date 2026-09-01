using System;

namespace MMNextPOS.Infrastructure.Logging
{
    /// <summary>
    /// Minimal logging abstraction used by the persistence layer so that the Infrastructure
    /// project doesn't depend on a logging framework. Logging is implemented in the WinForms
    /// E2E layer using Serilog wiring.
    /// </summary>
    public static class DiagLogger
    {
        public const string DefaultLogDirName = "logs";
        public const string LogFileTemplate = "MMNextPOS-.log";

        /// <summary>Current sink – if null, log calls are no-ops.</summary>
        public static Action<string, string>? Sink { get; set; }

        public static void Info(string message) => Sink?.Invoke("INFO", message);
        public static void Warn(string message) => Sink?.Invoke("WARN", message);
        public static void Error(string message) => Sink?.Invoke("ERROR", message);
        public static void Error(Exception ex, string message) => Sink?.Invoke("ERROR", $"{message}: {ex}");
    }
}

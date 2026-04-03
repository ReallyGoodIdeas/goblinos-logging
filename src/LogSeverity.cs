namespace Goblinos.Logging;
public enum LogSeverity
{
    Extra = -1,    // Extremely spammy logs that will dominate the console
    Trace = 0,     // Minor info
    Info = 1,      // Basic info level
    Warn = 2,   // Potential issues, non-gamebreaking
    Error = 3,     // Major issues that cause unintended side effects
    Critical = 4   // Severe game-breaking bugs
}

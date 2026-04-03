using System;

// ReSharper disable once CheckNamespace
namespace Goblinos.Logging;

/// <summary>
/// Lightweight, per-component logger that binds a component identity once,
/// so call sites do not need to pass a component parameter for each log.
///
/// <para>
/// <see cref="Logger"/> instances are created via <see cref="LogManager.For{T}"/> and
/// automatically include the component name in all log output.
/// </para>
///
/// <para>
/// This class is a thin facade over <see cref="LogManager"/> and respects all global
/// logging configuration, including severity thresholds, category filters, and
/// component filters.
/// </para>
/// </summary>
public sealed class Logger
{
    private readonly Type _componentType;
    private readonly string _componentName;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once ConvertToConstant.Global
    public bool Enabled { get; private set; } = true;

    /// <summary>
    /// Creates a logger bound to a specific component type.
    /// Intended to be constructed only by <see cref="LogManager"/>.
    /// </summary>
    internal Logger(Type componentType, string componentName)
    {
        _componentName = componentName;
        _componentType = componentType;
    }

    // ---------------------------------------------------------------------
    // Core logging
    // ---------------------------------------------------------------------

    /// <summary>
    /// Logs a message with an explicit severity and string-based category key.
    /// This overload is intended for dynamic or externally-defined categories.
    /// </summary>
    public void Log(string message, LogSeverity severity, string categoryKey)
    {
        if (!Enabled)
            return;
        LogManager.LogInternal(_componentType, _componentName, message, severity, categoryKey);
    }

    /// <summary>
    /// Logs a message with an explicit severity and enum-based category.
    /// This is the preferred overload when using project-defined categories.
    /// </summary>
    public void Log(string message, LogSeverity severity, LogCategory category)
    {
        LogManager.LogInternal(_componentType, _componentName, message, severity, category);
    }

    // ---------------------------------------------------------------------
    // Severity convenience helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Logs an extremely verbose diagnostic message.
    /// Intended for short-lived instrumentation only.
    /// </summary>
    public void Extra(string message, string categoryKey)
    {
        Log(message, LogSeverity.Extra, categoryKey);
    }

    /// <inheritdoc cref="Extra(string,string)"/>
    public void Extra(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Extra, category);
    }

    /// <summary>
    /// Logs a low-level trace message.
    /// </summary>
    public void Trace(string message, string categoryKey)
    {
        Log(message, LogSeverity.Trace, categoryKey);
    }

    /// <inheritdoc cref="Trace(string,string)"/>
    public void Trace(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Trace, category);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void Info(string message, string categoryKey)
    {
        Log(message, LogSeverity.Info, categoryKey);
    }

    /// <inheritdoc cref="Info(string,string)"/>
    public void Info(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Info, category);
    }

    /// <summary>
    /// Logs a warning-level message indicating a potential issue.
    /// </summary>
    public void Warn(string message, string categoryKey)
    {
        Log(message, LogSeverity.Warn, categoryKey);
    }

    /// <inheritdoc cref="Warning(string,string)"/>
    public void Warn(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Warn, category);
    }

    /// <summary>
    /// Logs an error-level message indicating incorrect or unexpected behavior.
    /// </summary>
    public void Error(string message, string categoryKey)
    {
        Log(message, LogSeverity.Error, categoryKey);
    }

    /// <inheritdoc cref="Error(string,string)"/>
    public void Error(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Error, category);
    }

    /// <summary>
    /// Logs a critical error indicating a severe or game-breaking condition.
    /// </summary>
    public void Critical(string message, string categoryKey)
    {
        Log(message, LogSeverity.Critical, categoryKey);
    }

    /// <inheritdoc cref="Critical(string,string)"/>
    public void Critical(string message, LogCategory category = LogCategory.None)
    {
        Log(message, LogSeverity.Critical, category);
    }

    /// <summary>
    /// Sets the component type associated with this logger as enabled
    /// </summary>
    public void Enable()
    {
        Enabled = true;
        // LogManager.SetComponentEnabled(_componentType, true);
    }
    
    /// <summary>
    /// Sets the component type associated with this logger as disabled
    /// </summary>
    public void Disable()
    {
        Enabled = false;
        // LogManager.SetComponentEnabled(_componentType, false);
    }
}

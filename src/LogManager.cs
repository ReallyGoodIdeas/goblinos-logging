using System;
using System.Collections.Generic;
using Godot;

namespace Goblinos.Logging;

/// <summary>
/// Centralized logging utility used across the project to control
/// log verbosity, routing, categorization, and component-level filtering.
///
/// <para>
/// <see cref="LogManager"/> is responsible for:
/// <list type="bullet">
/// <item>Global enable/disable of logging</item>
/// <item>Minimum severity filtering</item>
/// <item>Category-based log filtering (enum and string-based)</item>
/// <item>Optional component-level filtering</item>
/// <item>Routing logs to the appropriate Godot output channel</item>
/// </list>
/// </para>
///
/// <para>
/// Most gameplay code should not call <see cref="LogManager"/> directly.
/// Instead, obtain a per-class logger via <see cref="For{T}"/> and log
/// through <see cref="Logger"/>.
/// </para>
/// </summary>
public class LogManager
{
    /// <summary>
    /// Globally enables or disables all logging.
    /// When false, all log calls are discarded.
    /// </summary>
    public static bool LoggingEnabled { get; set; } = true;

    /// <summary>
    /// Logs below this severity level are discarded.
    /// </summary>
    public static LogSeverity MinimumLoggingSeverity { get; set; } = LogSeverity.Trace;

    /// <summary>
    /// Controls whether unknown category keys encountered at runtime
    /// should be automatically registered.
    /// </summary>
    public static bool ShouldRegisterNewCategories { get; set; } = false;

    /// <summary>
    /// Controls whether auto-registered categories are enabled by default.
    /// Only applies when <see cref="ShouldRegisterNewCategories"/> is true.
    /// </summary>
    public static bool ShouldEnableAutoRegisteredCategories { get; set; } = false;

    // ---------------------------------------------------------------------
    // Category state
    // ---------------------------------------------------------------------

    /// <summary>
    /// Stores enabled/disabled state for each log category key.
    /// Keys are case-insensitive to reduce accidental duplication.
    /// </summary>
    private static readonly Dictionary<string, bool> LoggingEnabledByCategoryKey =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Core & Engine-Level
            { nameof(LogCategory.None), true },
            { nameof(LogCategory.Initialization), true },
            { nameof(LogCategory.Exit), true },
            { nameof(LogCategory.Error), true },
            { nameof(LogCategory.Warning), true },
            { nameof(LogCategory.Signal), true },

            // Input & Cursor
            { nameof(LogCategory.Input), true },
            { nameof(LogCategory.UiNavigation), true },

            // Battle & Gameplay Flow
            { nameof(LogCategory.BattleState), true },
            { nameof(LogCategory.CombatResolution), true },

            // Units & AI
            { nameof(LogCategory.UnitLifecycle), true },
            { nameof(LogCategory.UnitStats), false },
            { nameof(LogCategory.AiDecision), true },
            { nameof(LogCategory.AiMovement), false },

            // Data & Resources
            { nameof(LogCategory.DataLoading), true },
            { nameof(LogCategory.Serialization), true },
            { nameof(LogCategory.Validation), true },

            // Performance / Diagnostics
            { nameof(LogCategory.Performance), false },
            { nameof(LogCategory.DebugOnly), false }
        };

    /// <summary>
    /// Optional per-component log filter.
    /// When empty, all components are allowed.
    /// </summary>
    private static readonly Dictionary<Type, bool> LoggingEnabledByComponent = new();

    /// <summary>
    /// Tracks categories that have already emitted a warning for being unregistered,
    /// to avoid spamming the console.
    /// </summary>
    private static readonly HashSet<string> WarnedUnregisteredCategories =
        new(StringComparer.OrdinalIgnoreCase);

    // ---------------------------------------------------------------------
    // Logger creation
    // ---------------------------------------------------------------------

    /// <summary>
    /// Creates a per-class logger bound to the specified component type.
    /// This is the preferred entry point for logging from gameplay code.
    /// </summary>
    public static Logger For<T>()
    {
        return new Logger(typeof(T), typeof(T).Name);
    }

    // ---------------------------------------------------------------------
    // Public logging entry points
    // ---------------------------------------------------------------------

    /// <summary>
    /// Logs a message with an explicit severity and string-based category key.
    /// Intended for dynamic or externally-defined categories.
    /// </summary>
    public static void Log(string str, LogSeverity severity, string categoryKey)
    {
        LogInternal(typeof(LogManager), nameof(LogManager), str, severity, categoryKey);
    }

    /// <summary>
    /// Logs a message with an explicit severity and enum-based category.
    /// </summary>
    public static void Log(
        string str,
        LogSeverity severity = LogSeverity.Trace,
        LogCategory category = LogCategory.None)
    {
        LogInternal(typeof(LogManager), nameof(LogManager), str, severity, category);
    }

    // ---------------------------------------------------------------------
    // Internal logging implementation
    // ---------------------------------------------------------------------

    /// <summary>
    /// Core logging implementation used by <see cref="Logger"/>.
    /// Applies global, category, and component filters before emitting output.
    /// </summary>
    internal static void LogInternal(
        Type componentType,
        string componentName,
        string message,
        LogSeverity severity,
        string categoryKey)
    {
        if (!LoggingEnabled || severity < MinimumLoggingSeverity)
            return;

        if (!IsCategoryEnabled(categoryKey))
            return;

        if (!IsComponentEnabled(componentType))
            return;

        var formatted = $"[{componentName}] {message}";

        switch (severity)
        {
            case >= LogSeverity.Critical:
                GD.PushError($"[CRITICAL] {formatted}");
                break;
            case >= LogSeverity.Error:
                GD.PushError(formatted);
                break;
            case >= LogSeverity.Warning:
                GD.PushWarning(formatted);
                break;
            default:
                GD.Print(formatted);
                break;
        }
    }

    internal static void LogInternal(
        Type componentType,
        string componentName,
        string message,
        LogSeverity severity,
        LogCategory category)
    {
        LogInternal(componentType, componentName, message, severity, ToCategoryKey(category));
    }

    // ---------------------------------------------------------------------
    // Category & component control
    // ---------------------------------------------------------------------

    /// <summary>
    /// Clears all component-level filters, allowing logs from all components.
    /// </summary>
    public static void ClearComponentFilter()
    {
        LoggingEnabledByComponent.Clear();
    }

    /// <summary>
    /// Disables all log categories.
    /// </summary>
    public static void DisableAllCategories()
    {
        SetAllCategories(false);
    }

    /// <summary>
    /// Enables all registered log categories.
    /// </summary>
    public static void EnableAllCategories()
    {
        SetAllCategories(true);
    }

    /// <summary>
    /// Enables only the specified string-based category keys.
    /// All other categories are disabled.
    /// </summary>
    public static void EnableOnlyCategories(params string[] categories)
    {
        SetAllCategories(false);

        foreach (var categoryKey in categories)
        {
            if (!string.IsNullOrWhiteSpace(categoryKey))
                SetCategoryEnabled(categoryKey, true);
        }
    }

    /// <summary>
    /// Enables only the specified enum-based categories.
    /// All other categories are disabled.
    /// </summary>
    public static void EnableOnlyCategories(params LogCategory[] categories)
    {
        SetAllCategories(false);

        foreach (var category in categories)
            SetCategoryEnabled(category, true);
    }

    /// <summary>
    /// Enables logging only for the specified component types.
    /// </summary>
    public static void EnableOnlyComponents(params Type[] components)
    {
        LoggingEnabledByComponent.Clear();

        foreach (var component in components)
            LoggingEnabledByComponent[component] = true;
    }

    // ---------------------------------------------------------------------
    // Category queries & mutation
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns whether the specified category key is currently enabled.
    /// May auto-register the category depending on configuration.
    /// </summary>
    public static bool IsCategoryEnabled(string categoryKey)
    {
        bool exists = LoggingEnabledByCategoryKey.TryGetValue(categoryKey, out bool enabled);

        if (!exists && !ShouldRegisterNewCategories)
        {
            if (WarnedUnregisteredCategories.Add(categoryKey))
                GD.PushWarning(
                    $"[DebugUtil] Unregistered category [{categoryKey}]. Log will be hidden.");

            return false;
        }

        if (!exists && ShouldRegisterNewCategories)
        {
            if (MinimumLoggingSeverity <= LogSeverity.Trace)
                GD.Print(
                    $"[DebugUtil] Unregistered category [{categoryKey}]. Auto registering.");

            RegisterCategory(categoryKey, ShouldEnableAutoRegisteredCategories);
            return ShouldEnableAutoRegisteredCategories;
        }

        return enabled;
    }

    public static bool IsCategoryEnabled(LogCategory category)
    {
        return IsCategoryEnabled(ToCategoryKey(category));
    }

    /// <summary>
    /// Returns whether logging is enabled for the specified component type.
    /// </summary>
    public static bool IsComponentEnabled(Type componentType)
    {
        if (LoggingEnabledByComponent.Count == 0)
            return true;

        return LoggingEnabledByComponent.TryGetValue(componentType, out bool enabled) && enabled;
    }

    /// <summary>
    /// Registers a new category key.
    /// Intended for external tools or reusable libraries.
    /// </summary>
    public static void RegisterCategory(string categoryKey, bool enabledByDefault = true)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
            throw new ArgumentException("Category key must be non-empty.", nameof(categoryKey));

        LoggingEnabledByCategoryKey.TryAdd(categoryKey, enabledByDefault);
    }

    public static void RegisterCategory(
        LogCategory category,
        bool enabledByDefault = true)
    {
        RegisterCategory(ToCategoryKey(category), enabledByDefault);
    }

    /// <summary>
    /// Sets the enabled state of a category key.
    /// </summary>
    public static void SetCategoryEnabled(string categoryKey, bool enabled)
    {
        if (!LoggingEnabledByCategoryKey.ContainsKey(categoryKey)
            && !ShouldRegisterNewCategories)
        {
            if (WarnedUnregisteredCategories.Add(categoryKey))
                GD.PushWarning(
                    $"[DebugUtil] Unregistered category [{categoryKey}]. Will not be enabled.");

            return;
        }

        if (!LoggingEnabledByCategoryKey.ContainsKey(categoryKey)
            && ShouldRegisterNewCategories)
        {
            if (MinimumLoggingSeverity <= LogSeverity.Trace)
                GD.Print(
                    $"[DebugUtil] Unregistered category [{categoryKey}]. Auto registering.");

            RegisterCategory(categoryKey, ShouldEnableAutoRegisteredCategories);
        }

        LoggingEnabledByCategoryKey[categoryKey] = enabled;
    }

    public static void SetCategoryEnabled(LogCategory category, bool enabled)
    {
        SetCategoryEnabled(ToCategoryKey(category), enabled);
    }

    /// <summary>
    /// Enables or disables logging for a specific component type.
    /// </summary>
    public static void SetComponentEnabled<T>(bool enabled)
    {
        LoggingEnabledByComponent[typeof(T)] = enabled;
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private static void SetAllCategories(bool enabled)
    {
        var keys = new List<string>(LoggingEnabledByCategoryKey.Keys);

        foreach (var key in keys)
            LoggingEnabledByCategoryKey[key] = enabled;
    }

    private static string ToCategoryKey(LogCategory category)
    {
        return category.ToString();
    }
}





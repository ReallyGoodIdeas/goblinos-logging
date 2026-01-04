namespace Goblinos.Logging;
public enum LogCategory
{
    // Core & Engine-Level
    None,               // Default / uncategorized
    Initialization,     // Node setup, _Ready, dependency wiring
    Exit,               // Shutdown, cleanup, scene exit
    Error,              // Non-fatal errors, recoverable failures
    Warning,            // Suspicious but allowed states
    Signal,              // Godot Signals
    
    // Input & Cursor
    Input,              // Raw input events, actions
    UiNavigation,       // Menu focus, UI selection, UI cursor
    
    // Battle & Gameplay flow
    BattleState,        // State machine transitions, turn start/end, phase changes
    CombatResolution,  //  Attacks, abilities, items, damage, hit/miss, crits, status effects
    
    // Units & AI
    UnitLifecycle,      // Spawn, death, removal
    UnitStats,          // HP, buffs, debuffs, stat changes
    AiDecision,         // AI evaluation & choice
    AiMovement,         // Pathing decisions, movement intent
    
    // Data & Resources
    DataLoading,        // Loading resources, JSON, configs
    Serialization,     // Save/load
    Validation,        // Data sanity checks
    
    // Performance / Diagnostics
    Performance,       // Timing, frame-sensitive diagnostics
    DebugOnly           // Temporary or experimental logs
}

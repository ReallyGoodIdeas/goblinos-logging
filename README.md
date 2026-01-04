# Goblinos Logging

A lightweight, extensible logging framework for **Godot 4.x C# projects**, focused on clarity, control, and low-noise debugging.

Goblinos Logging is designed to scale with project complexity without polluting gameplay code or overwhelming the console.

---

## Features

* Severity-based filtering (`Trace` → `Critical`)
* Category-based filtering (enum and string-based)
* Optional per-component filtering
* Godot-aware output routing (`GD.Print`, `GD.PushWarning`, `GD.PushError`)
* Per-class loggers to reduce call-site noise
* Safe handling of unknown or external categories

---

## Design Philosophy

* **Logs should explain *why*, not just *that*, something happened**
* **Subsystems, not severities**, determine log categories
* **Low friction at call sites** is critical for adoption
* Logging must be **easy to silence** when debugging unrelated systems

This library is intentionally opinionated and optimized for real-world debugging in complex Godot projects.

---

## Installation

Goblinos Logging is distributed as **source**, not a binary.

### Option 1: Copy Source

Copy the contents of the `src/` directory into your Godot project (for example: `res://Scripts/ThirdParty/Goblinos.Logging/`).

### Option 2: Git Submodule

Add the repository as a submodule:

```bash
git submodule add https://github.com/ReallyGoodIdeas/goblinos-logging External/goblinos-logging
```

Ensure the `.cs` files are included in your Godot C# project.

---

## Basic Usage

### Create a per-class logger (recommended)

```csharp
using Goblinos.Logging;

private static readonly Logger Log = LogManager.For<GridCursor>();
```

```csharp
Log.Trace("Cursor moved", LogCategory.UiNavigation);
Log.Info("Turn started", LogCategory.BattleState);
Log.Warning("Grid reference missing", LogCategory.Initialization);
```

Per-class loggers bind the component identity once, keeping call sites concise.

---

## Severity Filtering

Discard logs below a minimum severity:

```csharp
LogManager.MinimumLoggingSeverity = LogSeverity.Warning;
```

This allows only:

* `Warning`
* `Error`
* `Critical`

Regardless of category.

---

## Category Filtering

Categories represent **stable subsystems**, not message importance.

### Enable only specific categories

```csharp
LogManager.EnableOnlyCategories(
    LogCategory.UiNavigation,
    LogCategory.Input
);
```

Or using string keys:

```csharp
LogManager.EnableOnlyCategories("UiNavigation", "Input");
```

All other categories will be disabled.

---

## Enum vs String Categories

### Enum categories

* Preferred for project-owned code
* Refactor-safe
* Discoverable

```csharp
Log.Info("Phase changed", LogCategory.BattleState);
```

### String categories

* Intended for external tools, plugins, or reusable libraries
* Can be dynamically registered

```csharp
Log.Trace("Path recalculated", "NavMesh");
```

---

## Dynamic Category Registration

By default, unknown categories are hidden and emit a warning once.

To allow automatic registration:

```csharp
LogManager.ShouldRegisterNewCategories = true;
LogManager.ShouldEnableAutoRegisteredCategories = false; // or true
```

This is useful when integrating third-party code.

---

## Component-Level Filtering

Restrict logging to specific components:

```csharp
LogManager.EnableOnlyComponents(typeof(GridCursor));
```

Disable logging for a component:

```csharp
LogManager.SetComponentEnabled<GridCursor>(false);
```

If no component filters are set, all components are allowed.

---

## Output Routing

Severity determines how logs are routed:

| Severity         | Godot Output     |
| ---------------- | ---------------- |
| Trace / Info     | `GD.Print`       |
| Warning          | `GD.PushWarning` |
| Error / Critical | `GD.PushError`   |

Routing is centralized inside `LogManager`.

---

## Versioning

This project follows **semantic versioning**.

* `0.x` releases may change APIs
* `1.0.0` will indicate API stability

---

## License

MIT License © ReallyGoodIdeas

---

## Scope

Goblinos Logging is currently:

* Godot-first
* Source-distributed
* Optimized for game projects

Future releases may introduce engine-agnostic sinks and binary distribution, but these are not goals for v0.x.

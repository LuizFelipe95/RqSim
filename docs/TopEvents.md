# TopEvents - Physics Verification Events System

## Overview

The TopEvents system provides comprehensive tracking and visualization of physics verification events during RQ-Hypothesis simulation. It enables real-time monitoring of key emergent phenomena that validate the simulation's alignment with physical constants and theoretical predictions.

## Architecture

### Core Components

```
???????????????????????????????????????????????????????????????????
?                    PhysicsEventStore                             ?
?  ????????????????????????????????????????????????????????????   ?
?  ?  Thread-safe ConcurrentQueue<PhysicsVerificationEvent>    ?   ?
?  ?  - FIFO eviction (max 10,000 events)                     ?   ?
?  ?  - EventAdded / EventsCleared notifications              ?   ?
?  ????????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????????
                                 ?
                                 ?
???????????????????????????????????????????????????????????????????
?                    Form_Main.TopEvents.cs                        ?
?  ????????????????????????????????????????????????????????????   ?
?  ?  UI: TabPage with ListView + Toolbar                      ?   ?
?  ?  - Filter by event type (with descriptive names)          ?   ?
?  ?  - Live update toggle                                     ?   ?
?  ?  - Export to JSON                                         ?   ?
?  ????????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????????
```

### Event Types (PhysicsEventType enum)

| Event Type | ComboBox Display | Description | Key Metric |
|------------|------------------|-------------|------------|
| `MassGap` | MassGap - Yang-Mills spectral gap | Yang-Mills spectral gap detection | ?? - ?? eigenvalue separation |
| `SpectralDimension` | SpectralDimension - d_S ? 4D | Dimensional emergence measurement | d_S convergence toward 4D |
| `SpeedOfLightIsotropy` | SpeedOfLight - Lieb-Robinson | Lieb-Robinson bounds verification | Signal velocity variance |
| `RicciFlatness` | RicciFlatness - Ricci ? 0 | Vacuum curvature measurement | Average Ricci ? 0 |
| `HolographicAreaLaw` | HolographicAreaLaw - S ~ Area | Entropy scaling verification | S ~ Area vs S ~ Volume |
| `HausdorffDimension` | HausdorffDimension - d_H | Geometric dimension via ball growth | d_H measurement |
| `ClusterTransition` | ClusterTransition - phase | Giant cluster phase transitions | Cluster ratio thresholds |
| `EnergyViolation` | EnergyViolation - constraint | Energy conservation violation | Wheeler-DeWitt constraint |
| `AutoTuningAdjustment` | AutoTuning - parameter adj | Parameter optimization events | Before/after values |
| `Milestone` | Milestone - simulation | General simulation milestone | Step marker |

### Event Record Structure

```csharp
public sealed record PhysicsVerificationEvent(
    PhysicsEventType EventType,    // Classification
    long Timestamp,                 // Simulation step
    string Description,             // Human-readable summary
    double Value,                   // Primary measurement
    double? SecondaryValue,         // Optional comparison value
    Dictionary<string, object>? Parameters  // Extended data
)
{
    public Guid Id { get; init; }           // Unique identifier
    public DateTime RecordedAt { get; init; } // Wall-clock time
    public int Severity { get; init; }      // 0=info, 1=warning, 2=critical
}
```

## Usage

### Logging Events from Simulation

```csharp
// Log a mass gap measurement
form.LogMassGapEvent(step: 1000, gapValue: 0.0523, targetGap: 0.05);

// Log spectral dimension
form.LogSpectralDimensionEvent(step: 1500, dS: 3.98, confidence: 0.95);

// Log speed of light isotropy
form.LogSpeedOfLightEvent(step: 2000, velocity: 0.997, variance: 0.003);

// Log Ricci flatness
form.LogRicciFlatnessEvent(step: 2500, avgCurvature: -0.0012);

// Log holographic entropy
form.LogHolographicAreaLawEvent(step: 3000, entropy: 245.6, area: 100.0, volume: 1000.0);

// Log auto-tuning adjustment
form.LogAutoTuningEvent(step: 3500, "Temperature", oldValue: 0.5, newValue: 0.48);
```

### Direct Store Access

```csharp
// Get event store for external processing
PhysicsEventStore store = form.PhysicsEvents;

// Query events by type
var massGapEvents = store.GetByType(PhysicsEventType.MassGap);

// Get recent events
var recent = store.GetRecent(100);

// Get event counts by type
Dictionary<PhysicsEventType, int> summary = store.GetEventCounts();

// Export to JSON
await store.ExportToFileAsync("events.json");
```

## UI Features

### TabPage Layout

```
???????????????????????????????????????????????????????????????????
? [Event Type ?] [? Live Update] [Refresh] [Clear] [Export JSON]  ?
???????????????????????????????????????????????????????????????????
? Event Type     ? Description                  ? Parameters      ?
???????????????????????????????????????????????????????????????????
? MassGap        ? Mass gap ? = 0.052300        ? lambda1=0.0523  ?
? SpectralDim... ? d_S = 3.980 (conf: 95%)      ? dS=3.98, ...    ?
? RicciFlatness  ? Avg Ricci = -0.001200        ? avgRicci=-0.001 ?
? ...            ? ...                          ? ...             ?
???????????????????????????????????????????????????????????????????
```

### Color Coding

| Severity | Color | Meaning |
|----------|-------|---------|
| 0 | White | Normal/informational |
| 1 | Light Yellow (#FFFFC8) | Warning - deviation detected |
| 2 | Light Red (#FFC8C8) | Critical - significant anomaly |

### Controls

- **Event Type Dropdown**: Filter by specific physics event type
- **Live Update Checkbox**: Toggle real-time updates from simulation
- **Refresh Button**: Manually reload event list
- **Clear Button**: Remove all events (with confirmation)
- **Export JSON Button**: Save filtered events to file

## Integration with Pipeline

### Custom Pipeline Hook Points

The TopEvents system integrates with the RQ simulation pipeline at these stages:

1. **Spectral Analysis Phase**
   - `GpuSpectralEngine.EstimateMassGap()` ? `MassGap` events
   - `SpectralWalkEngine.MeasureSignalVelocity()` ? `SpeedOfLightIsotropy` events

2. **Curvature Measurement Phase**
   - `VacuumEnergyManager.CalculateAverageRicciCurvature()` ? `RicciFlatness` events

3. **Auto-Tuning Phase**
   - `AutoTuningEngine.AdjustParameters()` ? `AutoTuningAdjustment` events

### Extensibility for New Physics Modules

To add new event types:

1. **Add to PhysicsEventType enum**:
```csharp
// In PhysicsEventType.cs
public enum PhysicsEventType
{
    // ...existing types...
    
    /// <summary>
    /// Your new physics verification event.
    /// </summary>
    YourNewEventType,
}
```

2. **Add factory method to PhysicsVerificationEvent**:
```csharp
// In PhysicsVerificationEvent.cs
public static PhysicsVerificationEvent YourNewEvent(long step, double value, /* params */)
    => new(
        PhysicsEventType.YourNewEventType,
        step,
        $"Description: {value:F4}",
        value,
        null,
        new Dictionary<string, object> { ["key"] = value });
```

3. **Add logging helper to Form_Main.TopEvents.cs**:
```csharp
public void LogYourNewEvent(long step, double value)
{
    AddPhysicsEvent(PhysicsVerificationEvent.YourNewEvent(step, value));
}
```

## Known Constants Tracking

The system is designed to track emergence of known physical constants:

| Constant | Event Types | Target Value |
|----------|-------------|--------------|
| Speed of Light | `SpeedOfLightIsotropy` | Isotropic, variance ? 0 |
| Mass Gap (YM) | `MassGap` | ?? > 0 (? > 0) |
| Spacetime Dimension | `SpectralDimension` | d_S ? 4 |
| Flatness | `RicciFlatness` | ?R? ? 0 |
| Holographic Principle | `HolographicAreaLaw` | S/A = const |

## Performance Considerations

- **Thread Safety**: `PhysicsEventStore` uses `ConcurrentQueue<T>` for lock-free operations
- **Memory Limit**: Automatic FIFO eviction at 10,000 events
- **UI Throttling**: ListView limited to 500 displayed items
- **Batched Updates**: `BeginUpdate()`/`EndUpdate()` for bulk operations
- **Background Export**: Async file export with cancellation support

## File Locations

| File | Purpose |
|------|---------|
| `RqSimEngineApi/Events/PhysicsEventType.cs` | Event type enumeration |
| `RqSimEngineApi/Events/PhysicsVerificationEvent.cs` | Event record definition |
| `RqSimEngineApi/Events/PhysicsEventStore.cs` | Thread-safe event storage |
| `RqSimUI/Forms/MainForm/Form_Main.TopEvents.cs` | UI integration |

## JSON Export Format

```json
[
  {
    "EventType": "MassGap",
    "Timestamp": 1000,
    "Description": "Mass gap ? = 0.052300",
    "Value": 0.0523,
    "SecondaryValue": 0.05,
    "Parameters": {
      "lambda1": 0.0523
    },
    "Id": "a1b2c3d4-...",
    "RecordedAt": "2024-01-15T14:32:00Z",
    "Severity": 0
  },
  // ... more events
]

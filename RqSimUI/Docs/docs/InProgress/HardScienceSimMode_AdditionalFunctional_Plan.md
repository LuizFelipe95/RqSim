# Hard Science Simulation Mode - Extended Functional Plan

## ?? Статус реализации: ? COMPLETED

**Дата создания:** 2024
**Последнее обновление:** Implementation complete
**Локация:** `RqSimGraphEngine\RQSimulation\Core\StrongScienceSimulationConfig\`

---

## 1. Принцип «Чистой Комнаты» (Scientific Integrity)

В текущем виде (если судить по наличию PhysicsConstants.Fitted.cs и PhysicsConstants.Fundamental.cs в одном проекте) существует ненулевая вероятность «загрязнения» эксперимента.

### Сценарий риска
Вы запускаете симуляцию для проверки гипотезы о массе гравитона. Ваш код случайно подтягивает значение G из визуального конфига, которое было подогнано, чтобы граф красиво вращался.

### ? Решение (РЕАЛИЗОВАНО)
`StrictScienceProfile` физически запрещает доступ к любым константам из `Fitted`. Он требует явной инъекции зависимостей (Dependency Injection) только от `Fundamental` констант.

---

## 2. Разные цели — Разные Конфигурации

SimulationEngine обслуживает два противоречивых хозяина:

### Визуальный режим (Demo/Sandbox) ? `VisualSandboxProfile`

| Параметр | Значение |
|----------|----------|
| Приоритет | 60 FPS, отсутствие "взрывов" вершин, плавность |
| Допущения | Можно использовать Clamp, Tanh, искусственную вязкость |
| Конфиг | Разрешает изменение параметров на лету (слайдеры UI) |
| Soft Walls | ? Enabled |
| Artificial Viscosity | ? Enabled |
| Precision | Single (float) |

### Научный режим (Strong Science/HPC) ? `StrictScienceProfile`

| Параметр | Значение |
|----------|----------|
| Приоритет | Точность (Double/Quad), сохранение унитарности, гамильтонова инвариантность |
| Допущения | Если Вселенная взрывается — это результат! Никакого сглаживания |
| Конфиг | Immutable после старта. Хеш конфига сохраняется вместе с данными |
| Soft Walls | ? Disabled |
| Artificial Viscosity | ? Disabled |
| Precision | Double/Quad |

---

## 3. ? Реализованная Архитектура

### Структура папки `StrongScienceSimulationConfig/`

```
RqSimGraphEngine/
??? RQSimulation/
    ??? Core/
        ??? StrongScienceSimulationConfig/
            ??? ISimulationProfile.cs           # Базовый контракт профиля
            ??? IPhysicalConstants.cs           # Strategy для констант + cbuffer
            ??? PlanckScaleConstants.cs         # Фундаментальные константы (CODATA 2022)
            ??? LatticeUnitsConstants.cs        # Безразмерные единицы решетки
            ??? StrictScienceProfile.cs         # Научный профиль + Protocol Hash
            ??? VisualSandboxProfile.cs         # Песочница + FittedConstants
            ??? ScientificMalpracticeException.cs # Исключение для нарушений
```

### 3.1 Контракт `ISimulationProfile`

```csharp
public interface ISimulationProfile
{
    string ProfileName { get; }
    bool IsStrictValidationEnabled { get; }    // Падать при NaN и нарушениях
    IPhysicalConstants Constants { get; }       // Стратегия констант
    bool AllowInteractiveRewiring { get; }      // Разрешено ли UI вмешательство
    bool UseSoftWalls { get; }                  // Clamp веса рёбер
    bool UseArtificialViscosity { get; }        // GeometryInertia damping
    NumericalPrecision Precision { get; }       // Single/Double/Quad
    
    string GetConfigurationHash();              // SHA256 для воспроизводимости
    void Validate();                            // Проверка целостности
}
```

### 3.2 Контракт `IPhysicalConstants`

```csharp
public interface IPhysicalConstants
{
    string Name { get; }
    string Description { get; }
    
    // Фундаментальные (c=?=G=k_B=1 в Планковских единицах)
    double C { get; }
    double HBar { get; }
    double G { get; }
    double KBoltzmann { get; }
    
    // Калибровочные константы (безразмерные, CODATA 2022)
    double FineStructureConstant { get; }       // ? = 1/137.036
    double StrongCouplingConstant { get; }      // ?_s = 0.118
    double WeakMixingAngle { get; }             // sin??_W = 0.231
    
    // Симуляционные параметры
    double GravitationalCoupling { get; }       // G_eff (может отличаться от G)
    double CosmologicalConstant { get; }        // ?
    double VacuumEnergyDensity { get; }         // ?_vac
    double InformationFlowRate { get; }         // v ? c
    double MetricRelaxationRate { get; }        // M_geom (ex-GeometryInertiaMass)
    
    // Документация и GPU поддержка
    string RescalingDocumentation { get; }
    ConstantBufferData GetConstantBuffer();     // float4-aligned для HLSL
}
```

### 3.3 Реализации констант

#### `PlanckScaleConstants` — для строгих научных симуляций

```csharp
public sealed class PlanckScaleConstants : IPhysicalConstants
{
    // c = ? = G = k_B = 1 (Планковские единицы)
    public double C => 1.0;
    public double HBar => 1.0;
    public double G => 1.0;
    
    // CODATA 2022 значения
    public double FineStructureConstant => 1.0 / 137.035999084;
    
    // ФИЗИЧЕСКИЕ значения (не подогнанные!)
    public double CosmologicalConstant => 1.1e-122;  // Реальное ?
    public double VacuumEnergyDensity => 5.96e-127;  // Реальное ?_vac
}
```

#### `LatticeUnitsConstants` — для решёточных симуляций

```csharp
public sealed class LatticeUnitsConstants : IPhysicalConstants
{
    private readonly double _latticeSpacing;
    private readonly double _scaleFactor;
    
    public LatticeUnitsConstants(double latticeSpacing = 1.0, int graphSize = 1000)
    {
        _latticeSpacing = latticeSpacing;
        // Finite-size scaling: G_eff = G ? ?(N_crit/N)
        _scaleFactor = Math.Sqrt(1000.0 / graphSize);
    }
    
    // Rescaled для конечного размера графа
    public double GravitationalCoupling => 1.0 * _scaleFactor;
    public double CosmologicalConstant => 1.0e-4;  // Visible on small graphs
}
```

#### `FittedConstants` — для визуальной песочницы (MUTABLE!)

```csharp
public sealed class FittedConstants : IPhysicalConstants
{
    // MUTABLE — можно менять через UI слайдеры
    public double GravitationalCoupling { get; set; } = 0.1;
    public double CosmologicalConstant { get; set; } = 1.0e-4;
    public double VacuumEnergyDensity { get; set; } = 1000.0;
    public double InformationFlowRate { get; set; } = 0.5;
    public double MetricRelaxationRate { get; set; } = 10.0;  // ex-GeometryInertiaMass
}
```

### 3.4 Профили симуляции

#### `StrictScienceProfile`

```csharp
public sealed class StrictScienceProfile : ISimulationProfile
{
    public bool IsStrictValidationEnabled => true;  // Падаем при NaN
    public bool AllowInteractiveRewiring => false;  // Руками не трогать!
    public bool UseSoftWalls => false;              // Пусть взрывается
    public bool UseArtificialViscosity => false;    // Чистый Гамильтониан
    
    public StrictScienceProfile(IPhysicalConstants constants, string experimentId)
    {
        // ВАЛИДАЦИЯ: Запрещаем FittedConstants
        if (constants is FittedConstants)
            throw new ScientificMalpracticeException("...");
            
        // ВАЛИДАЦИЯ: Проверяем ? не подогнан
        if (constants.CosmologicalConstant > 1e-10)
            throw new ScientificMalpracticeException("Detected fitted Lambda!");
    }
    
    // Хеш конфига для воспроизводимости
    public string GetConfigurationHash() => ComputeSHA256Hash();
    
    // Протокол эксперимента для архива
    public ExperimentProtocol GetProtocol() => new ExperimentProtocol { ... };
}
```

#### `VisualSandboxProfile`

```csharp
public sealed class VisualSandboxProfile : ISimulationProfile
{
    public bool IsStrictValidationEnabled => false; // NaN ? reset
    public bool AllowInteractiveRewiring => true;   // Слайдеры работают
    public bool UseSoftWalls => true;               // Clamp веса
    public bool UseArtificialViscosity => true;     // Damping включен
    
    public FittedConstants MutableConstants { get; } // Для UI binding
}
```

---

## 4. Unitless Lattice Units (Обезразмеривание)

### Принцип

В физике на решётке ставим: **c=1, ?=1, G=1, a=1**

Все "магические числа" типа `1.0e-4` — это коэффициенты в единицах решётки.

### ? Реализовано

#### Явный Rescaling

Документация включает формулу перевода:
```
Value_sim = Value_real ? ScaleFactor
```

**Пример из `LatticeUnitsConstants.RescalingDocumentation`:**
```
RESCALING FORMULAS (sim ? physical):
------------------------------------
Length:      L_phys = L_sim ? a ? l_P = L_sim ? 1.616e-35 m
Time:        t_phys = t_sim ? a ? t_P = t_sim ? 5.391e-44 s
Mass:        m_phys = m_sim ? m_P/a   = m_sim ? 2.176e-8 kg
Energy:      E_phys = E_sim ? E_P/a   = E_sim ? 1.956e9 J
```

#### Dependency Injection через cbuffer

```csharp
// Структура для HLSL cbuffer (float4-aligned)
public readonly struct ConstantBufferData
{
    // float4[0]: Fundamental constants
    public readonly float C, HBar, G_Newton, KBoltzmann;
    
    // float4[1]: Coupling constants
    public readonly float FineStructure, StrongCoupling, WeakMixing, GravitationalCoupling;
    
    // float4[2]: Simulation parameters
    public readonly float CosmologicalConstant, VacuumEnergyDensity, InformationFlowRate, MetricRelaxationRate;
    
    public static int SizeInBytes => 48; // 3 ? float4
}

// Использование в шейдере:
cbuffer PhysicsConstants : register(b0)
{
    float4 Fundamental;  // c, ?, G, k_B
    float4 Couplings;    // ?, ?_s, sin??_W, G_eff
    float4 Simulation;   // ?, ?_vac, v_info, M_metric
};
```

#### Терминология

| Старое имя | Новое имя | Физический смысл |
|------------|-----------|------------------|
| `GeometryInertiaMass` | `MetricRelaxationRate` | Hamiltonian Momentum Term |
| `GeometryInertia` | `MetricRelaxationRate` | ADM mass in kinetic term |

---

## 5. Usage Examples

### Научный эксперимент (Lattice QCD)

```csharp
// Создание строгого научного профиля
var profile = StrictScienceProfile.CreatePlanckScale("LQCD_Experiment_2024_001");

// Валидация автоматически проверяет:
// - Нет FittedConstants
// - ? < 10??? (физическое значение)
// - ? в диапазоне [0.007, 0.008]

// Получение протокола для архива
ExperimentProtocol protocol = profile.GetProtocol();
protocol.SaveToFile("experiment_protocol.json");

Console.WriteLine($"Config hash: {profile.GetConfigurationHash()}");
// Output: Config hash: 7A3F...B2C1 (SHA256)
```

### Визуальная демонстрация

```csharp
// Создание профиля песочницы
var sandbox = new VisualSandboxProfile(constants =>
{
    constants.GravitationalCoupling = 0.15;  // Немного сильнее
    constants.MetricRelaxationRate = 8.0;    // Быстрее реакция
});

// UI binding
slider_Gravity.DataContext = sandbox.MutableConstants;

// Параметры можно менять на лету
sandbox.MutableConstants.CosmologicalConstant = 5e-4;
```

### Интеграция с SimulationEngine

```csharp
public sealed class SimulationEngine
{
    private readonly ISimulationProfile _profile;
    
    public SimulationEngine(SimulationConfig cfg, ISimulationProfile profile)
    {
        _profile = profile;
        
        // Используем константы из профиля
        var G = _profile.Constants.GravitationalCoupling;
        var lambda = _profile.Constants.CosmologicalConstant;
        
        // Настройка поведения в зависимости от режима
        _useSoftWalls = _profile.UseSoftWalls;
        _validateEnergy = _profile.IsStrictValidationEnabled;
    }
    
    public void RunStep(double dt)
    {
        // Проверка интерактивного вмешательства
        if (!_profile.AllowInteractiveRewiring && _uiModified)
            throw ScientificMalpracticeException
                .InteractiveModificationAttempted("EdgeWeight");
        
        // Физический шаг
        Pipeline.ExecuteFrame(_graph, dt);
        
        // Валидация (только в строгом режиме)
        if (_profile.IsStrictValidationEnabled)
        {
            ValidateEnergyConservation();
            ValidateGaugeConstraints();
        }
    }
}
```

---

## 6. ScientificMalpracticeException

Специальное исключение для явного обнаружения нарушений научной целостности:

```csharp
public enum ScientificMalpracticeType
{
    FittedConstantsContamination,      // Fitted в Science mode
    InvalidPhysicalConstants,          // Константы вне допуска
    ConfigurationHashMismatch,         // Хеш не совпадает
    InteractiveModificationInScienceMode, // UI изменения в Science mode
    EnergyConservationViolation,       // Нарушение сохранения энергии
    GaugeConstraintViolation,          // Нарушение калибровки
    HamiltonianInvarianceViolation     // Нарушение Гамильтоновой инвариантности
}
```

**Пример вывода:**
```
?? CRITICAL: Scientific Integrity Violation
========================================
Type: FittedConstantsContamination

Detected suspiciously large ? = 1.00E-04. Physical value is ~10????. This looks like a fitted value.

RECOMMENDED ACTIONS:
1. Use PlanckScaleConstants or LatticeUnitsConstants instead of FittedConstants
2. Verify no static references to PhysicsConstants.Fitted.* in science code
3. Run simulation with StrictScienceProfile, not VisualSandboxProfile
```

---

## 7. Migration Guide

### Шаг 1: Определите режим использования

```csharp
// БЫЛО:
var engine = new SimulationEngine(config);

// СТАЛО (наука):
var profile = StrictScienceProfile.CreateLatticeUnits("MyExperiment", graphSize: 500);
var engine = new SimulationEngine(config, profile);

// СТАЛО (демо):
var profile = new VisualSandboxProfile();
var engine = new SimulationEngine(config, profile);
```

### Шаг 2: Замените прямые обращения к константам

```csharp
// БЫЛО:
double g = PhysicsConstants.GravitationalCoupling;
double lambda = PhysicsConstants.Fitted.CosmologicalConstant;

// СТАЛО:
double g = _profile.Constants.GravitationalCoupling;
double lambda = _profile.Constants.CosmologicalConstant;
```

### Шаг 3: Используйте cbuffer для GPU

```csharp
// БЫЛО:
shader.SetConstant("G", PhysicsConstants.GravitationalCoupling);

// СТАЛО:
var cbuffer = _profile.Constants.GetConstantBuffer();
shader.UpdateConstantBuffer(cbuffer);
```

---

## 8. Checklist для Code Review

- [ ] Нет прямых ссылок на `PhysicsConstants.Fitted.*` в научном коде
- [ ] Все физические ядра получают константы через `IPhysicalConstants`
- [ ] GPU шейдеры используют cbuffer, а не hardcoded значения
- [ ] `StrictScienceProfile` используется для публикуемых результатов
- [ ] Хеш конфигурации сохраняется вместе с данными симуляции
- [ ] Терминология соответствует научной (`MetricRelaxationRate`, не `GeometryInertiaMass`)
- [ ] Документация rescaling включена в output







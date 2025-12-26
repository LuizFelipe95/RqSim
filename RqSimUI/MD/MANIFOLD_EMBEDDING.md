# Manifold Embedding (Emergent Geometry Visualization)

## Обзор

Manifold Embedding — это визуализация топологии графа на основе RQ-гипотезы, где **расстояние не предзадано, а возникает из силы взаимодействия** (веса рёбер).

### Физико-математическое обоснование

В RQ-теории "близко" означает "сильно связано". Вместо жёстких координат $(x, y, z)$ мы вводим **динамические координаты** $\vec{r}_i$, которые минимизируют "потенциальную энергию укладки":

$$
E = \sum_{(i,j) \in \text{edges}} w_{ij} \cdot |\vec{r}_i - \vec{r}_j|^2 + k \sum_{i \neq j} \frac{1}{|\vec{r}_i - \vec{r}_j|}
$$

Где:
- $w_{ij}$ — вес ребра (сила связи / спутанность)
- Первое слагаемое — **пружинное притяжение** (сильнее связь = короче расстояние)
- Второе слагаемое — **глобальное отталкивание** (предотвращает коллапс)

### Результат визуализации

| Топология графа | Визуальный результат |
|-----------------|----------------------|
| 1D цепочка | Линейный филамент |
| 2D решётка | Плоская мембрана |
| 3D bulk | Сферическое распределение |
| Квантовая пена | Пульсирующая сложная структура |

---

## Архитектура

### Компоненты

```
???????????????????????????????????????????????????????????????????
?                     Manifold Embedding System                    ?
???????????????????????????????????????????????????????????????????
?  UI Layer (PartialForm3D.cs)                                    ?
?  ?? CheckBox: "Enable Manifold Embedding"                       ?
?  ?? ComboBox: Color Mode включает "Manifold"                    ?
?  ?? Visual indicators (orange border when active)               ?
???????????????????????????????????????????????????????????????????
?  CPU Implementation (PartialForm3D.cs)                          ?
?  ?? ApplyManifoldEmbedding() — force-directed layout            ?
?  ?? Persistent positions (_embeddingPositionX/Y/Z)              ?
?  ?? Velocity integration with damping                           ?
???????????????????????????????????????????????????????????????????
?  GPU Shaders (ManifoldEmbeddingShader.cs)                       ?
?  ?? ManifoldEmbeddingShader — основной compute shader           ?
?  ?? CenterOfMassKernel — вычисление центра масс                 ?
?  ?? ManifoldColorMapperShader — stability-based coloring        ?
???????????????????????????????????????????????????????????????????
?  Plugin Module (GpuManifoldEmbeddingModule.cs)                  ?
?  ?? IPhysicsModule implementation                               ?
?  ?? CPU fallback when GPU unavailable                           ?
?  ?? ExecutionStage.PostProcess                                  ?
???????????????????????????????????????????????????????????????????
```

### Файлы

| Файл | Назначение |
|------|------------|
| `RqSimUI/Forms/3DVisual/GDI+/PartialForm3D.cs` | UI + CPU реализация |
| `RqSimGraphEngine/RQSimulation/GPUOptimized/Rendering/ManifoldEmbeddingShader.cs` | GPU compute shaders |
| `RqSim.PluginManager.UI/IncludedPlugins/GPUOptimizedCSR/GpuManifoldEmbeddingModule.cs` | Plugin для pipeline |

---

## Алгоритм Force-Directed Layout

### Шаг 1: Глобальное отталкивание от центра масс

Предотвращает коллапс графа в точку:

```csharp
// Вычисляем центр масс
float comX = ?(x_i) / n;
float comY = ?(y_i) / n;
float comZ = ?(z_i) / n;

// Сила отталкивания обратно пропорциональна квадрату расстояния
F_repulsion = k_rep / |r_i - COM|?
```

### Шаг 2: Пружинное притяжение вдоль рёбер

Закон Гука с жёсткостью пружины пропорциональной весу ребра:

```csharp
// Целевая длина обратно пропорциональна весу
L_target = 1 / (w_ij + ?)

// Сила пружины
F_spring = k_spring * w_ij * (|r_i - r_j| - L_target)
```

### Шаг 3: Интеграция с затуханием

Semi-implicit Euler с damping для сходимости:

```csharp
velocity = (velocity + force * dt) * damping
position += velocity * dt
```

### Параметры

| Параметр | Значение | Описание |
|----------|----------|----------|
| `ManifoldRepulsionFactor` | 0.5 | Сила отталкивания от центра |
| `ManifoldSpringFactor` | 0.8 | Жёсткость пружин |
| `ManifoldDamping` | 0.85 | Коэффициент затухания (0-1) |
| `ManifoldDeltaTime` | 0.05 | Шаг интеграции |

---

## Использование

### Включение в UI

1. Откройте вкладку **3D Visual**
2. Установите галочку **"Enable Manifold Embedding"**
3. (Опционально) Выберите **Color Mode: "Manifold"** для stability-based coloring

### Визуальные индикаторы

- **Оранжевая рамка** — manifold embedding активен
- **Надпись "MANIFOLD EMBEDDING ACTIVE"** — в правом верхнем углу
- **Статус в метке** — `[Manifold: ON]`

### Цветовая схема (режим "Manifold")

| Цвет | Значение |
|------|----------|
| ?? Красный | Высокая связность / сингулярность (время заморожено) |
| ?? Жёлтый | Искривлённое пространство-время |
| ?? Синий | Плоский вакуум (нормальное течение времени) |

---

## GPU Реализация

### ManifoldEmbeddingShader

```csharp
[ThreadGroupSize(DefaultThreadGroupSizes.X)]
[GeneratedComputeShaderDescriptor]
[RequiresDoublePrecisionSupport]
public readonly partial struct ManifoldEmbeddingShader : IComputeShader
{
    public readonly ReadWriteBuffer<Double4> positions;
    public readonly ReadWriteBuffer<Double4> velocities;
    public readonly ReadOnlyBuffer<int> rowPtr;      // CSR
    public readonly ReadOnlyBuffer<int> colIdx;      // CSR
    public readonly ReadOnlyBuffer<double> weights;  // CSR
    // ... parameters
}
```

### Особенности

1. **Double precision** для накопления (критично для точности физики)
2. **CSR формат** для разреженных графов
3. **Параллельная обработка** — каждый thread обрабатывает один узел
4. **Dimension reduction** — опциональное "сплющивание" в 2D/1D

---

## Интеграция с Pipeline

### GpuManifoldEmbeddingModule

```csharp
public sealed class GpuManifoldEmbeddingModule : GpuPluginBase
{
    public override string Name => "Manifold Embedding";
    public override ExecutionStage Stage => ExecutionStage.PostProcess;
    public override int Priority => 200;
    
    public bool ManifoldEnabled { get; set; }
    
    public override void ExecuteStep(RQGraph graph, double dt)
    {
        if (!ManifoldEnabled) return;
        ExecuteCpuStep(graph, dt);  // или GPU версия
    }
}
```

### Регистрация

```csharp
// В IncludedPluginsRegistry или при инициализации pipeline
pipeline.RegisterModule(new GpuManifoldEmbeddingModule());
```

---

## Связь с RQ-гипотезой

### Emergent Geometry

Manifold Embedding демонстрирует ключевую идею RQ-гипотезы:

> **Геометрия не предзадана, а возникает из квантовых корреляций**

- Сильные рёбра (высокий $w_{ij}$) соответствуют "близким" точкам
- Слабые рёбра — "далёким" точкам
- Визуализация показывает **эмерджентную метрику** графа

### Spectral Dimension $d_S$

Manifold embedding дополняет спектральную размерность:
- $d_S \approx 4$ ? 3D Bulk (целевое пространство-время)
- $d_S < 2$ ? 1D филаменты (что видно как линии)
- $d_S \approx 3$ ? 2D мембраны (плоские структуры)

---

## Troubleshooting

### Ноды не двигаются

**Причина**: Позиции сбрасывались каждый кадр из spectral coordinates.

**Решение**: Используются persistent позиции (`_embeddingPositionX/Y/Z`), которые сохраняются между кадрами.

### Слишком медленное движение

**Решение**: Увеличить `ManifoldDeltaTime` (по умолчанию 0.05).

### Граф коллапсирует в точку

**Причина**: Недостаточное отталкивание.

**Решение**: Увеличить `ManifoldRepulsionFactor`.

### Граф "взрывается"

**Причина**: Слишком сильные силы или отсутствие damping.

**Решение**: Уменьшить `ManifoldSpringFactor` или увеличить `ManifoldDamping`.

---

## Будущие улучшения

1. **GPU-ускорение** — полная реализация на GPU через ManifoldEmbeddingShader
2. **Dimension reduction** — интерактивный выбор целевой размерности (1D/2D/3D)
3. **Hierarchical embedding** — для очень больших графов (N > 100k)
4. **Real-time curvature coloring** — цвет на основе Ollivier-Ricci кривизны
5. **Export** — сохранение embedded координат для анализа

---

*Последнее обновление: 2025-01*

# Hidden Surface Removal (HSR) - Модернизация DX12 рендеринга

## Обзор

Этот документ описывает реализацию алгоритмов Hidden Surface Removal (удаление скрытых поверхностей) для DX12 рендеринга графов в RqSimulator. Оптимизации направлены на решение проблемы "волосяного кома" (hairball problem) при визуализации графов с сотнями тысяч рёбер.

## Проблема

При визуализации крупных графов возникают следующие проблемы:

1. **Overdraw** - многократная отрисовка пикселей для перекрывающихся рёбер
2. **Z-fighting** - мерцание при близких значениях глубины
3. **Алиасинг** - артефакты на мелких рёбрах
4. **Производительность** - GPU обрабатывает вершины даже невидимых рёбер

## Реализованные оптимизации

### 1. Reverse Z-Buffering

**Проблема:** Стандартный Z-буфер имеет нелинейное распределение точности - высокая точность вблизи камеры и низкая вдалеке, что вызывает Z-fighting на больших масштабах.

**Решение:** Reverse-Z инвертирует отображение глубины:
- Near plane ? 1.0 (вместо 0.0)
- Far plane ? 0.0 (вместо 1.0)
- Comparison function: `ComparisonFunction.Greater` (вместо Less)
- Clear depth value: 0.0 (вместо 1.0)

**Файлы:**
| Файл | Описание |
|------|----------|
| `CameraMatrixHelper.cs` | Функция `CreatePerspectiveReverseZ()` и константа `ReverseZClearDepth = 0.0f` |
| `Dx12RenderHost.cs` | Создание DSV heap и depth buffer с очисткой 0.0 |
| `SphereRenderer.cs` | PSO с `DepthFunc = ComparisonFunction.Greater` |
| `LineRenderer.cs` | PSO с `DepthFunc = ComparisonFunction.Greater` |

**Преимущества:**
- Равномерное распределение точности глубины
- Поддержка бесконечного far plane (`float.PositiveInfinity`)
- Устранение Z-fighting на космологических масштабах

### 2. Early-Z Optimization (Строгий порядок отрисовки)

**Проблема:** GPU выполняет пиксельный шейдер даже для фрагментов, которые будут отброшены тестом глубины.

**Решение:** Early-Z - аппаратная оптимизация GPU, которая отбрасывает фрагменты ДО выполнения пиксельного шейдера, если они не проходят тест глубины.

**Порядок отрисовки:**
1. **Nodes (сферы)** - рисуются первыми, заполняют Z-буфер как большие окклюдеры
2. **Edges (линии)** - рисуются вторыми, GPU автоматически отбрасывает фрагменты за сферами

**Файлы:**
| Файл | Описание |
|------|----------|
| `Dx12SceneRenderer.cs` | Управляет порядком отрисовки: `Render()` вызывает spheres ? edges |
| `SphereRenderer.cs` | Рендеринг узлов (первый проход), `DepthWriteMask.All` |
| `LineRenderer.cs` | Рендеринг рёбер (второй проход), `DepthWriteMask.All` |

### 3. GPU Frustum Culling (Compute Shader)

**Проблема:** Все рёбра отправляются на GPU, даже если они за пределами камеры.

**Решение:** Compute shader отфильтровывает невидимые рёбра до растеризации.

**Алгоритм:**
1. Проверка попадания в пирамиду видимости (Frustum Culling)
2. Проверка минимального размера на экране (Subpixel Culling)
3. Запись видимых рёбер в выходной буфер
4. Отрисовка через ExecuteIndirect

**Файлы:**
| Файл | Описание |
|------|----------|
| `Dx12CullingShaders.cs` | HLSL compute shaders: `EdgeCullingCs`, `ResetIndirectArgsCs` |
| `GpuEdgeCuller.cs` | C# инфраструктура: буферы, pipeline state, ExecuteIndirect |
| `LineRenderer.cs` | Метод `DrawIndirect()` для отрисовки отфильтрованных рёбер |

### 4. Subpixel Culling

**Проблема:** Очень мелкие рёбра (< 1 пикселя) создают алиасинг и шум, не неся полезной информации.

**Решение:** Отсечение рёбер, проецируемый размер которых меньше порога.

**Параметры:**
- `MinProjectedEdgeSize` - минимальный размер в NDC (по умолчанию 0.002 ? 1-2 пикселя при 1080p)

### 5. Адаптивный MSAA

**Проблема:** Не все адаптеры (особенно WARP) поддерживают MSAA 4x для всех форматов.

**Решение:** Автоматическое определение поддерживаемого уровня MSAA при инициализации.

**Файлы:**
| Файл | Описание |
|------|----------|
| `Dx12RenderHost.cs` | Метод `DetermineMsaaSampleCount()` - проверяет 4x ? 2x ? 1x |

### 6. Явная инициализация PSO

**Проблема:** Предустановленные значения (`BlendDescription.Opaque`, `RasterizerDescription.CullCounterClockwise`) могут быть неполностью инициализированы на некоторых адаптерах (особенно WARP).

**Решение:** Все состояния PSO создаются явно со всеми полями:

```csharp
// Пример: RasterizerDescription в SphereRenderer
var rasterizerState = new RasterizerDescription
{
    FillMode = FillMode.Solid,
    CullMode = CullMode.Back,
    FrontCounterClockwise = true,
    DepthBias = 0,
    DepthBiasClamp = 0.0f,
    SlopeScaledDepthBias = 0.0f,
    DepthClipEnable = true,
    MultisampleEnable = sampleDescription.Count > 1,
    AntialiasedLineEnable = false,
    ForcedSampleCount = 0,
    ConservativeRaster = ConservativeRasterizationMode.Off
};
```

**Важно:** В Vortice порядок параметров `InputElementDescription`:
```csharp
// (semanticName, semanticIndex, format, offset, slot, slotClass, stepRate)
new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
//                                                                 ^offset ^slot
```
  
## Архитектура файлов

```
RqSimRenderingEngine/
??? Rendering/
    ??? Backend/
        ??? DX12/
            ??? Dx12RenderHost.cs              # Главный класс рендеринга
            ?   ??? DSV Heap + Depth Buffer    # Reverse-Z буфер глубины
            ?   ??? DetermineMsaaSampleCount() # Проверка поддержки MSAA
            ?   ??? RenderScene()              # Публичный метод отрисовки
            ?   ??? GpuEdgeCullingEnabled      # Настройка GPU culling
            ?   ??? MinProjectedEdgeSize       # Порог subpixel culling
            ?
            ??? Rendering/
                ??? CameraMatrixHelper.cs      # Матрицы камеры
                ?   ??? CreatePerspectiveReverseZ()  # Reverse-Z проекция
                ?   ??? CreateOrbitCamera()    # Орбитальная камера
                ?   ??? ReverseZClearDepth     # Значение очистки (0.0)
                ?
                ??? Dx12SceneRenderer.cs       # Управление порядком отрисовки
                ?   ??? Initialize()           # Инициализация рендереров
                ?   ??? Render()               # Nodes ? Edges (Early-Z)
                ?   ??? GpuCullingEnabled      # Включение GPU culling
                ?   ??? MinProjectedEdgeSize   # Порог subpixel
                ?
                ??? Dx12CullingShaders.cs      # HLSL compute shaders
                ?   ??? EdgeCullingCs          # Основной shader culling
                ?   ??? ResetIndirectArgsCs    # Сброс indirect args
                ?
                ??? GpuEdgeCuller.cs           # Инфраструктура GPU culling
                ?   ??? Initialize()           # Создание pipelines
                ?   ??? ExecuteCulling()       # Запуск compute shader
                ?   ??? CullingConstants       # Структура констант
                ?   ??? DrawArguments          # Структура для ExecuteIndirect
                ?
                ??? SphereRenderer.cs          # Рендеринг узлов (первый проход)
                ?   ??? Explicit RasterizerDescription
                ?   ??? Explicit BlendDescription  
                ?   ??? Explicit DepthStencilDescription (Reverse-Z)
                ?
                ??? LineRenderer.cs            # Рендеринг рёбер (второй проход)
                ?   ??? Draw()                 # Прямая отрисовка
                ?   ??? DrawIndirect()         # Через ExecuteIndirect
                ?   ??? AntialiasedLineEnable = true
                ?   ??? Alpha blending для полупрозрачных рёбер
                ?
                ??? Dx12DepthStencilConfigs.cs # (Legacy) Готовые конфигурации depth-stencil
```

## Использование

### Базовый рендеринг

```csharp
// Инициализация
var renderHost = new Dx12RenderHost();
renderHost.Initialize(new RenderHostInitOptions(hwnd, width, height));

// Render loop
renderHost.BeginFrame();

// Установка камеры с Reverse-Z
var view = CameraMatrixHelper.CreateOrbitCamera(target, distance, yaw, pitch);
var proj = CameraMatrixHelper.CreatePerspectiveReverseZ(fov, aspect, nearPlane, float.PositiveInfinity);
renderHost.SetCameraMatrices(view, proj);

// Установка данных
renderHost.SetNodeInstances(nodes, nodeCount);
renderHost.SetEdgeVertices(edges, edgeCount);

// Отрисовка сцены (автоматически: nodes ? edges)
renderHost.RenderScene();

renderHost.EndFrame();
```

### С GPU Culling (для >10,000 рёбер)

```csharp
// Включение GPU culling
renderHost.GpuEdgeCullingEnabled = true;
renderHost.MinProjectedEdgeSize = 0.002f; // 1-2 пикселя

// Остальной код без изменений
renderHost.RenderScene(); // Автоматически использует compute culling
```

## Производительность

| Оптимизация | Влияние | Когда использовать |
|-------------|---------|-------------------|
| Reverse-Z | Устраняет Z-fighting | Всегда |
| Early-Z (порядок отрисовки) | До 50% сокращение pixel shader | Всегда |
| GPU Frustum Culling | До 90% сокращение вершин | >10,000 рёбер |
| Subpixel Culling | Устраняет шум + доп. оптимизация | Большие графы |
| Адаптивный MSAA | Совместимость с WARP | Автоматически |
| Explicit PSO init | Совместимость с WARP | Автоматически |

## Технические детали

### InputElementDescription в Vortice

Порядок параметров конструктора:
```csharp
InputElementDescription(
    string semanticName,
    int semanticIndex, 
    Format format,
    int offset,        // Смещение в байтах внутри вершины
    int slot,          // Номер input slot (0 = vertex, 1 = instance)
    InputClassification slotClass,
    int stepRate       // 0 для per-vertex, 1 для per-instance
)
```

**Пример для инстансированного рендеринга:**
```csharp
// Slot 0: Per-vertex данные
new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),

// Slot 1: Per-instance данные
new InputElementDescription("INSTANCEPOS", 0, Format.R32G32B32_Float, 0, 1, InputClassification.PerInstanceData, 1),
new InputElementDescription("INSTANCERADIUS", 0, Format.R32_Float, 12, 1, InputClassification.PerInstanceData, 1),
new InputElementDescription("INSTANCECOLOR", 0, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1)
```

### MSAA Detection

```csharp
private static int DetermineMsaaSampleCount(ID3D12Device device, Format rtFormat, Format depthFormat)
{
    uint[] sampleCounts = [4, 2, 1];
    
    foreach (uint count in sampleCounts)
    {
       

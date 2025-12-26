# План модернизации графического конвейера RqSim

> **Версия:** 1.0  
> **Дата создания:** 2025
> **Статус:** В разработке

## Обзор

Данный документ описывает план внедрения трёх ключевых оптимизаций графического конвейера:

1. **Packed Data Structures** — оптимизация структур данных для GPU
2. **Vertex Pulling (Quads)** — процедурная генерация геометрии в шейдере
3. **Occlusion Culling** — отсечение невидимых рёбер через Depth Pre-Pass

---

## Архитектура

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         RENDER FRAME PIPELINE                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────┐                                                    │
│  │ STEP 1: DEPTH       │  • Рендеринг только сфер (узлов)                   │
│  │ PRE-PASS            │  • ColorWriteEnable = false                        │
│  │                     │  • Заполняет Z-буфер для Early-Z                   │
│  └──────────┬──────────┘                                                    │
│             │                                                               │
│             ▼                                                               │
│  ┌─────────────────────┐                                                    │
│  │ STEP 2: COMPUTE     │  • Читает Depth Buffer как SRV                     │
│  │ EDGE CULLING        │  • Проверяет видимость каждого ребра               │
│  │                     │  • Пишет в AppendStructuredBuffer                  │
│  └──────────┬──────────┘                                                    │
│             │                                                               │
│             ▼                                                               │
│  ┌─────────────────────┐                                                    │
│  │ STEP 3: INDIRECT    │  • Vertex Pulling: 6 вершин на ребро               │
│  │ EDGE DRAW (QUADS)   │  • Billboard-квады с толщиной от Tension           │
│  │                     │  • ExecuteIndirect с данными из Compute            │
│  └─────────────────────┘                                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Компонент 1: Packed Data Structures

### Цель
Снизить нагрузку на шину PCIe и кэш GPU за счёт оптимизации размера структур.

### Текущее состояние

**Файл:** `RqSimGraphEngine\RQSimulation\GPUOptimized\Rendering\RenderDataTypes.cs`

```csharp
// Текущие структуры (64-80 байт на узел)
public struct PhysicsNodeState  // 56 байт, без alignment
{
    public double X, Y, Z;       // 24 байта
    public double PsiReal, PsiImag; // 16 байт
    public double Potential;     // 8 байт
    public double Mass;          // 8 байт
}

public struct RenderNodeVertex  // 32 байта
{
    public float X, Y, Z;        // 12 байт
    public float R, G, B, A;     // 16 байт
    public float Size;           // 4 байта
}
```

### Целевое состояние

```csharp
// Оптимизированные структуры (32 байта на узел)
[StructLayout(LayoutKind.Sequential, Pack = 16)] 
public struct PackedNodeData
{
    public Float3 Position;      // 12 bytes: xyz
    public float Scale;          // 4 bytes: w (упаковано для GPU)
    
    public uint ColorEncoded;    // 4 bytes: RGBA8
    public float Energy;         // 4 bytes: Glow intensity
    public uint Flags;           // 4 bytes: Bitmask
    public float Padding;        // 4 bytes: Alignment
}

// Оптимизированные рёбра (16 байт)
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct PackedEdgeData
{
    public int NodeIndexA;       // 4 bytes
    public int NodeIndexB;       // 4 bytes
    public float Weight;         // 4 bytes: Метрическое расстояние
    public float Tension;        // 4 bytes: Натяжение
}
```

### Примечание о double precision

Для сохранения совместимости с double precision (физические расчёты):

```csharp
// Режим высокой точности (optional)
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct PackedNodeDataDouble
{
    public Double3 Position;     // 24 bytes
    public double Scale;         // 8 bytes
    public uint ColorEncoded;    // 4 bytes
    public uint Flags;           // 4 bytes
    public double Energy;        // 8 bytes
    // Total: 48 bytes (padding handled by GPU)
}
```

### Чек-лист

- [x] Создать бэкап `RenderDataTypes.cs` → `Alternate/RenderDataTypes.original.txt`
- [x] Добавить `PackedNodeData` и `PackedEdgeData` 
- [x] Добавить `PackedNodeDataDouble` для режима высокой точности
- [x] Добавить helper-методы для кодирования/декодирования цвета (RGBA8)
- [x] Обновить `RenderMapperShader.cs` для работы с новыми типами
- [x] Добавить флаг `UseDoublePrecision` в конфигурацию рендера

---

## Компонент 2: Vertex Pulling (Quads)

### Цель
Заменить линии (1px) на объёмные "струны" с переменной толщиной, используя процедурную геометрию.

### Текущее состояние

**Файл:** `RqSimRenderingEngine\Rendering\Backend\DX12\Rendering\Dx12SceneRenderer.cs`

- Использует `PrimitiveTopology.LineList`
- Вертексный буфер с `Dx12LineVertex`
- Нет процедурной генерации

### Целевое состояние

- `PrimitiveTopology.TriangleList`
- **Без вертексного буфера** для геометрии линий
- Генерация 6 вершин (2 треугольника) на ребро в Vertex Shader
- Billboard-ориентация для поворота к камере

### HLSL Шейдер: `EdgeQuadShader.hlsl`

```hlsl
// EdgeQuadShader.hlsl
// Генерируем 6 вершин (2 треугольника) на каждое ребро
// Вызывается через DrawInstanced(6, edgeCount, ...)

cbuffer CameraData : register(b0)
{
    float4x4 ViewProj;
    float3 CameraPos;
    float Padding;
    float BaseThickness;
};

struct PackedEdgeData {
    uint NodeIndexA;
    uint NodeIndexB;
    float Weight;
    float Tension;
};

struct PackedNodeData {
    float3 Position;
    float Scale;
    uint ColorEncoded;
    float Energy;
    uint Flags;
    float Padding;
};

StructuredBuffer<PackedEdgeData> Edges : register(t0);
StructuredBuffer<PackedNodeData> Nodes : register(t1);

struct PSInput
{
    float4 Pos : SV_POSITION;
    float2 UV : TEXCOORD0;
    float Tension : TEXCOORD1;
};

PSInput VSMain(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
{
    PSInput output;

    // 1. Читаем данные ребра
    PackedEdgeData edge = Edges[instanceID];
    float3 p0 = Nodes[edge.NodeIndexA].Position;
    float3 p1 = Nodes[edge.NodeIndexB].Position;

    // 2. Вычисляем базис для billboard-а
    float3 edgeDir = p1 - p0;
    float3 edgeDirNorm = normalize(edgeDir);
    float3 viewDir = normalize(CameraPos - (p0 + p1) * 0.5);
    float3 right = normalize(cross(edgeDirNorm, viewDir));

    // 3. UV координаты для вершин квада
    float2 uv;
    uint v = vertexID % 6;
    if (v == 0) uv = float2(0, 0);
    else if (v == 1) uv = float2(1, 0);
    else if (v == 2) uv = float2(0, 1);
    else if (v == 3) uv = float2(0, 1);
    else if (v == 4) uv = float2(1, 0);
    else uv = float2(1, 1);

    // 4. Толщина на основе натяжения
    float thickness = BaseThickness * (1.0 + edge.Tension * 0.5);

    // 5. Позиция вершины
    float3 pos = lerp(p0, p1, uv.y);
    pos += right * (uv.x - 0.5) * thickness;

    output.Pos = mul(float4(pos, 1.0), ViewProj);
    output.UV = uv;
    output.Tension = edge.Tension;

    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    // Анти-алиасинг по краям
    float dist = abs(input.UV.x - 0.5) * 2.0;
    float alpha = 1.0 - smoothstep(0.8, 1.0, dist);
    
    // Цвет зависит от натяжения (Blue → Red)
    float3 color = lerp(float3(0, 0, 1), float3(1, 0, 0), input.Tension);
    
    return float4(color, alpha);
}
```

### Чек-лист

- [x] Создать бэкап `LineRenderer.cs` → `Alternate/LineRenderer.original.txt`
- [x] Создать `EdgeQuadShader.hlsl` в папке шейдеров
- [x] Создать `EdgeQuadRenderer.cs` с поддержкой Vertex Pulling
- [x] Обновить PSO для `TriangleList` без Input Layout
- [x] Добавить буферы `StructuredBuffer<PackedEdgeData>` и `StructuredBuffer<PackedNodeData>`
- [x] Реализовать fallback на стандартные линии при отсутствии поддержки

---

## Компонент 3: Occlusion Culling (Depth Pre-Pass)

### Цель
Решить проблему "волосяного кома" — отсечение невидимых рёбер, скрытых за узлами.

### Архитектура

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       OCCLUSION CULLING PIPELINE                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INPUT                                                                      │
│  ├── AllEdges[] (StructuredBuffer)                                          │
│  ├── AllNodes[] (StructuredBuffer)                                          │
│  └── DepthBuffer (Texture2D SRV)                                            │
│                                                                             │
│  COMPUTE SHADER (EdgeOcclusionCull.hlsl)                                    │
│  ├── Для каждого ребра (i, j):                                              │
│  │   ├── Вычислить screenPosA = Project(Nodes[i].Position)                  │
│  │   ├── Вычислить screenPosB = Project(Nodes[j].Position)                  │
│  │   ├── Сэмплировать depthA = DepthBuffer[screenPosA.xy]                   │
│  │   ├── Сэмплировать depthB = DepthBuffer[screenPosB.xy]                   │
│  │   └── IF (screenPosA.z > depthA AND screenPosB.z > depthB)               │
│  │       └── CULL (оба конца за непрозрачной геометрией)                    │
│  │   ELSE                                                                   │
│  │       └── Append to VisibleEdges                                         │
│  └── Increment counter                                                      │
│                                                                             │
│  OUTPUT                                                                     │
│  ├── VisibleEdges[] (AppendStructuredBuffer)                                │
│  └── CounterBuffer (для ExecuteIndirect)                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### HLSL Compute Shader: `EdgeOcclusionCull.hlsl`

```hlsl
// EdgeOcclusionCull.hlsl

cbuffer CullingConstants : register(b0)
{
    float4x4 ViewProj;
    float2 ScreenSize;
    float NearZ;          // Для Reverse-Z: дальняя плоскость = 0
    float FarZ;           // Для Reverse-Z: ближняя плоскость = 1
    uint TotalEdgeCount;
    float DepthBias;      // Небольшой bias для избежания z-fighting
    float2 Padding;
};

struct PackedEdgeData {
    uint NodeIndexA;
    uint NodeIndexB;
    float Weight;
    float Tension;
};

struct PackedNodeData {
    float3 Position;
    float Scale;
    uint ColorEncoded;
    float Energy;
    uint Flags;
    float Padding;
};

StructuredBuffer<PackedEdgeData> AllEdges : register(t0);
StructuredBuffer<PackedNodeData> AllNodes : register(t1);
Texture2D<float> DepthBuffer : register(t2);
SamplerState PointSampler : register(s0);

AppendStructuredBuffer<PackedEdgeData> VisibleEdges : register(u0);

float3 ProjectToScreen(float3 worldPos)
{
    float4 clip = mul(float4(worldPos, 1.0), ViewProj);
    float3 ndc = clip.xyz / clip.w;
    
    // NDC → Screen coordinates
    float2 screen;
    screen.x = (ndc.x * 0.5 + 0.5) * ScreenSize.x;
    screen.y = (1.0 - (ndc.y * 0.5 + 0.5)) * ScreenSize.y; // Y flip
    
    return float3(screen, ndc.z);
}

bool IsOccluded(float3 screenPos)
{
    // Bounds check
    if (screenPos.x < 0 || screenPos.x >= ScreenSize.x ||
        screenPos.y < 0 || screenPos.y >= ScreenSize.y)
        return false; // Off-screen edges are not occluded
    
    float2 uv = screenPos.xy / ScreenSize;
    float depthSample = DepthBuffer.SampleLevel(PointSampler, uv, 0);
    
    // Reverse-Z: larger depth = closer to camera
    // Edge is occluded if its depth is LESS than the buffer (behind geometry)
    return (screenPos.z + DepthBias) < depthSample;
}

[numthreads(256, 1, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    uint edgeIndex = DTid.x;
    if (edgeIndex >= TotalEdgeCount)
        return;
    
    PackedEdgeData edge = AllEdges[edgeIndex];
    
    float3 posA = AllNodes[edge.NodeIndexA].Position;
    float3 posB = AllNodes[edge.NodeIndexB].Position;
    
    float3 screenA = ProjectToScreen(posA);
    float3 screenB = ProjectToScreen(posB);
    
    bool occludedA = IsOccluded(screenA);
    bool occludedB = IsOccluded(screenB);
    
    // Консервативная стратегия: отсекаем только если ОБА конца скрыты
    if (occludedA && occludedB)
        return; // Полностью скрыто
    
    // Ребро видимо — добавляем в выходной буфер
    VisibleEdges.Append(edge);
}
```

### Интеграция в RenderFrame

```csharp
public void RenderFrame(SimulationContext context)
{
    // =========================================================
    // ШАГ 1: DEPTH PRE-PASS
    // =========================================================
    
    // Привязываем ТОЛЬКО Depth Buffer (без Render Target)
    commandList.OMSetRenderTargets(0, null, true, &dsvHandle);
    commandList.ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 0.0f, 0, 0, null);
    
    _sphereRenderer.RenderDepthOnly(commandList, context.Nodes);

    // Барьер: Depth Write → SRV
    var barrierRead = CD3DX12_RESOURCE_BARRIER.Transition(
        _depthStencilTexture,
        ResourceStates.DepthWrite,
        ResourceStates.NonPixelShaderResource);
    commandList.ResourceBarrier(1, &barrierRead);

    // =========================================================
    // ШАГ 2: COMPUTE CULLING
    // =========================================================

    // Сброс счетчика
    commandList.CopyBufferRegion(_counterBuffer, 0, _zeroBuffer, 0, 4);
    
    commandList.SetComputeRootSignature(_computeRootSig);
    commandList.SetPipelineState(_cullEdgesPSO);
    
    _gpuEdgeCuller.Dispatch(commandList, context.TotalEdges);

    // Барьер UAV
    var barrierUAV = CD3DX12_RESOURCE_BARRIER.UAV(_visibleEdgesBuffer);
    commandList.ResourceBarrier(1, &barrierUAV);

    // =========================================================
    // ШАГ 3: INDIRECT DRAW (QUADS)
    // =========================================================

    // Depth Buffer → Read-Only
    var barrierDepthRead = CD3DX12_RESOURCE_BARRIER.Transition(
        _depthStencilTexture,
        ResourceStates.NonPixelShaderResource,
        ResourceStates.DepthRead);
    commandList.ResourceBarrier(1, &barrierDepthRead);

    commandList.OMSetRenderTargets(1, &rtvHandle, true, &dsvHandle);
    commandList.SetPipelineState(_edgeQuadPSO);
    commandList.IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    commandList.ExecuteIndirect(
        _indirectCommandSignature,
        1,
        _indirectArgsBuffer,
        0,
        _counterBuffer,
        0);

    // =========================================================
    // UI & Present
    // =========================================================
    _imGuiRenderer.Render(commandList);
}
```

### Чек-лист

- [x] Создать бэкап `GpuEdgeCuller.cs` → `Alternate/GpuEdgeCuller.original.txt`
- [x] Создать `EdgeOcclusionCull.hlsl`
- [x] Добавить Depth Pre-Pass в `SphereRenderer` (новый метод `RenderDepthOnly`)
- [x] Создать PSO для Depth-Only рендеринга (ColorWriteEnable = false)
- [x] Обновить `GpuEdgeCuller` для работы с Depth Buffer как SRV
- [x] Настроить `CommandSignature` для `ExecuteIndirect`
- [x] Добавить fallback на прямой draw при неподдержке GPU culling
- [x] Настроить Reverse-Z в проекционной матрице (NearZ = 1, FarZ = 0)

---

## Новые файлы

| Файл | Описание |
|------|----------|
| `RenderDataTypes.cs` | Обновление с `PackedNodeData`, `PackedEdgeData` |
| `EdgeQuadShader.hlsl` | Vertex Pulling шейдер для квадов |
| `EdgeOcclusionCull.hlsl` | Compute shader для occlusion culling |
| `EdgeQuadRenderer.cs` | Новый рендерер для квадов |
| `OcclusionCullingPipeline.cs` | Координатор Depth Pre-Pass и Culling |

---

## Файлы для бэкапа

| Оригинальный файл | Бэкап |
|-------------------|-------|
| `RenderDataTypes.cs` | `Alternate/RenderDataTypes.original.txt` |
| `Dx12SceneRenderer.cs` | `Alternate/Dx12SceneRenderer.original.txt` |
| `LineRenderer.cs` | `Alternate/LineRenderer.original.txt` |
| `GpuEdgeCuller.cs` | `Alternate/GpuEdgeCuller.original.txt` |
| `SphereRenderer.cs` | `Alternate/SphereRenderer.original.txt` |

---

## Порядок внедрения

### Фаза 1: Packed Data Structures (1-2 дня)
1. Бэкап существующих файлов
2. Добавление новых структур данных
3. Обновление маппера
4. Тестирование совместимости

### Фаза 2: Vertex Pulling (2-3 дня)
1. Создание HLSL шейдера
2. Создание нового рендерера
3. Интеграция с существующим пайплайном
4. Тестирование на разных сценах

### Фаза 3: Occlusion Culling (3-4 дня)
1. Depth Pre-Pass (модификация сферного рендерера)
2. Compute Shader для culling
3. ExecuteIndirect интеграция
4. Профилирование и оптимизация

---

## Метрики успеха

| Метрика | Текущее | Целевое |
|---------|---------|---------|
| Память на узел | 56-80 байт | 32 байта |
| Память на ребро | 32 байта | 16 байт |
| Визуальное качество рёбер | 1px линии | Объёмные струны |
| FPS при 100K рёбер | ~30 FPS | >60 FPS |
| "Волосяной ком" | Присутствует | Устранён |

---

## Примечания

### Совместимость с double precision

Все оптимизации сохраняют возможность использования `double` для физических расчётов:

```csharp
public enum RenderPrecisionMode
{
    /// <summary>Стандартный режим: float для рендеринга</summary>
    Float,
    
    /// <summary>Высокая точность: double для позиций</summary>
    Double
}
```

### Fallback стратегия

При отсутствии поддержки (WARP, старое оборудование):
- Vertex Pulling → стандартные линии
- Occlusion Culling → прямой draw без culling
- Packed Structures → стандартные структуры

---

## История изменений

| Дата | Версия | Изменения |
|------|--------|-----------|
| 2024-XX-XX | 1.0 | Первоначальный план |

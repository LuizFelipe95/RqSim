# План Модернизации RQSimulation - Краткий Порядок Выполнения

## Рекомендуемый порядок реализации этапов

| # | Этап | Описание | Сложность | Риск | Статус |
|---|------|----------|-----------|------|--------|
| 1 | **Этап 6** | Quantum Edges - Квантовые амплитуды рёбер | Низкая | Низкий | ✅ Готово |
| 2 | **Этап 3** | Wilson Loops - Калибровочная инвариантность | Низкая | Низкий | ✅ Готово |
| 3 | **Этап 5** | Spectral Action - Спектральное действие | Средняя | Средний | ✅ Готово |
| 4 | **Этап 2** | Wheeler-DeWitt - Строгий гамильтониан связей | Средняя | Средний | ✅ Готово |
| 5 | **Этап 4** | Internal Observer - Устранение внешнего наблюдателя | Средняя | Низкий | ✅ Готово |
| 6 | **Этап 1** | MCMC Sampling - Переход к MCMC-сэмплированию | Высокая | Высокий | ✅ Готово |

---

## Этап 6: Quantum Edges (Quantum Graphity) ✅ РЕАЛИЗОВАНО

**Цель:** Ребро находится в суперпозиции существования с комплексной амплитудой.

**Файлы:**
- `Core/Infrastructure/ComplexEdge.cs` - расширена структура
- `Quantum/QuantumEdges/RQGraph.QuantumEdges.cs` - новый partial class

**Ключевые изменения:**
- `ComplexEdge.Amplitude` - квантовая амплитуда существования ребра
- `ComplexEdge.ExistenceProbability` - вероятность |α|²
- `ComplexEdge.Superposition()` - создание суперпозиции
- `ComplexEdge.Measure()` - коллапс волновой функции
- `RQGraph.EnableQuantumEdges()` - активация квантового режима
- `RQGraph.CollapseQuantumEdges()` - коллапс всех рёбер
- `RQGraph.EvolveQuantumEdges()` - унитарная эволюция амплитуд
- `RQGraph.GetQuantumEdgePurity()` - мера "классичности" состояния

**Тесты:**
- `ComplexEdge_QuantumAmplitude_Tests.cs` - 17 тестов
- `RQGraph_QuantumEdges_Tests.cs` - 16 тестов
- Всего: 33 теста, все проходят

**Обратная совместимость:**
- `_quantumEdges = null` по умолчанию - классическое поведение сохранено
- Все существующие методы работают без изменений

---

## Этап 3: Wilson Loops (Калибровочная инвариантность) ✅ РЕАЛИЗОВАНО

**Цель:** Запретить удаление рёбер, разрывающих калибровочный поток.

**Файлы:**
- `Physics/Interactions/GaugeAwareTopology.cs`
- `Physics/CorePhysics/RQGraph.LocalAction.Metropolis.cs`
- `Core/Constants/PhysicsConstants.RQHypothesis.cs`

---

## Этап 5: Spectral Action ✅ РЕАЛИЗОВАНО

**Цель:** Размерность 4D как энергетический минимум спектрального действия.

**Файлы:**
- `Core/Constants/PhysicsConstants.RQHypothesis.cs` - добавлен SpectralActionConstants
- `Core/Numerics/SpectralAction.cs` - новый статический класс
- `Core/Numerics/RQGraph.PhysicsUtils.cs` - методы-обёртки

**Ключевые изменения:**
- `SpectralActionConstants` - константы Chamseddine-Connes спектрального действия:
  - `LambdaCutoff` - UV отсечка (Планковский масштаб)
  - `F0_Cosmological` - космологический член
  - `F2_Einштейна-Гильберта` - член Эйнштейна-Гильберта
  - `F4_Weyl` - член Вейля
  - `TargetSpectralDimension = 4.0` - целевая размерность
  - `DimensionPotentialStrength` - сила потенциала стабилизации
  - `EnableSpectralActionMode` - флаг активации

**Новые методы в SpectralAction:**
- `ComputeSpectralAction()` - полное спектральное действие S
- `ComputeDimensionPotential()` - Mexican hat потенциал для d_S
- `ComputeEffectiveVolume()` - эффективный объём графа
- `ComputeAverageCurvature()` - средняя скалярная кривизна
- `ComputeWeylSquared()` - квадрат тензора Вейля (через дисперсию кривизны)
- `EstimateSpectralDimensionFast()` - быстрая оценка d_S по средней степени
- `ComputeActionGradient()` - градиент действия по весу ребра
- `IsNearActionMinimum()` - проверка близости к минимуму

**Обёртки в RQGraph:**
- `RQGraph.EstimateSpectralDimensionFast()` - делегирует к SpectralAction
- `RQGraph.ComputeSpectralAction()` - делегирует к SpectralAction
- `RQGraph.IsNearSpectralActionMinimum()` - делегирует к SpectralAction

**Тесты:**
- `SpectralAction_Tests.cs` - 21 тест, все проходят
  - Тесты констант
  - Тесты вычисления объёма
  - Тесты средней кривизны
  - Тесты Weyl²
  - Тесты быстрой оценки d_S
  - Тесты dimension potential
  - Тесты полного спектрального действия
  - Тесты интеграции с RQGraph
  - Тесты градиента

**Обратная совместимость:**
- `EnableSpectralActionMode = true` по умолчанию
- Старый DimensionPenalty может быть отключён через `EnableNaturalDimensionEmergence`

---

## Этап 2: Wheeler-DeWitt Constraint  ✅ РЕАЛИЗОВАНО

**Цель:** Ввести проверку ограничения H_total ≈ 0.

**Файлы:**
- `Topology/TopologicalProtection/RQGraph.Hamiltonian.cs`
- `Core/Numerics/EnergyLedger.cs`

---

## Этап 4: Internal Observer ✅ РЕАЛИЗОВАНО

**Цель:** Наблюдатель как подсистема графа.

**Файлы:**
- `Quantum/Measurement/InternalObserver.cs` - новый класс внутреннего наблюдателя
- `Quantum/Measurement/RQGraph.InternalObserver.cs` - новый partial class для интеграции

**Ключевые изменения:**
- `InternalObserver` - класс внутреннего наблюдателя:
  - `MeasureObservableInternal()` - слабое измерение через запутывание
  - `MeasureSweep()` - серия измерений целевых узлов
  - `GetObserverExpectationValue()` - ожидаемое значение наблюдателя
  - `GetObserverTotalPhase()` - суммарная фаза в подсистеме наблюдателя
  - `GetCorrelationWithRegion()` - корреляция с целевым регионом
  - `GetMutualInformation()` - взаимная информация наблюдатель-цель
  - `GetStatistics()` - статистика наблюдений

- `ObservationRecord` - запись одного наблюдения
- `ObservationStatistics` - сводная статистика

**Новые методы в RQGraph:**
- `ConfigureInternalObserver()` - настройка подсистемы наблюдателя
- `ConfigureInternalObserverAuto()` - автоматический выбор узлов
- `DisableInternalObserver()` - отключение режима
- `GetInternallyObservedEnergy()` - RQ-совместимое измерение энергии
- `GetInternallyObservedEnergyOfRegion()` - измерение энергии региона
- `ShiftNodePhase()` - сдвиг фазы волновой функции узла
- `GetNodeWavefunction()` - получение волновой функции узла
- `SetNodeWavefunction()` - установка волновой функции узла
- `GetObserverMutualInformation()` - взаимная информация
- `GetObservationStatistics()` - статистика наблюдений

**Тесты:**
- `InternalObserver_Tests.cs` - 25 тестов, все проходят
  - Тесты конструктора
  - Тесты измерений
  - Тесты сдвига фазы
  - Тесты интеграции с RQGraph
  - Тесты корреляций и взаимной информации
  - Тесты статистики
  - Тесты запутывания

**Обратная совместимость:**
- `_internalObserver = null` по умолчанию - легаси-поведение сохранено
- `GetInternallyObservedEnergy()` без наблюдателя возвращает `ComputeNetworkHamiltonian()`
- Все существующие методы статистики работают без изменений

---

## Этап 1: MCMC Sampling ✅ РЕАЛИЗОВАНО

**Цель:** Заменить цикл эволюции на MCMC-сэмплирование.

**Файлы:**
- `Core/SimulationConfig/MCMCSampler.cs` (новый)
- `Core/SimulationConfig/SimulationEngine.cs` (обновлен)

**Ключевые изменения:**
- `MCMCSampler` - класс для сэмплирования конфигураций графа
  - `CalculateEuclideanAction()` - вычисление евклидова действия (S_gravity + S_matter + S_gauge)
  - `ProposeMove()` - предложение топологического изменения (добавление/удаление/изменение веса)
  - `SampleConfigurationSpace()` - основной цикл Metropolis-Hastings
- `SimulationConfig` - добавлены параметры MCMC:
  - `UseMCMCSampling` - флаг включения режима
  - `MCMCSamples` - количество сэмплов
  - `MCMCThermalizationSweeps` - количество проходов термализации
- `SimulationEngine` - добавлен метод `RunMCMCSampling()`

**Тесты:**
- `MCMCSampler_Tests.cs` - 3 теста, все проходят
  - `CalculateEuclideanAction_ReturnsFiniteValue`
  - `ProposeMove_ReturnsValidDeltaAndActions`
  - `SampleConfigurationSpace_UpdatesStatistics`

---

## Проверка после каждого этапа

```bash
dotnet build RqSimGraphEngine/RqSimGraphEngine.csproj
dotnet test RqSimGPUCPUTests/RqSimGPUCPUTests.csproj

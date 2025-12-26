 ратко:
Х	ƒобавлена опци€ UseOllivierRicciForWeyl в GpuSpectralActionEngine (по умолчанию false).
Х	ƒобавлен буфер _edgeCurvatures и логика в BuildCsrFromGraph(RQGraph) дл€ автоматического вычислени€ кривизн ребер с помощью ComputeOllivierRicciJaccard(RQGraph, int, int) при включенной опции.
Х	ComputeWeylSquared() использует пер-реберные кривизны дл€ расчЄта анизотропии (variance по инцидентным ребрам) и возвращает среднюю анизотропию по узлам; если автоматическое вычисление не было выполнено, метод аккуратно откатитс€ к нулю или к существующему поведению.




========================= TO DO ======================================


Х	ƒобавить UI-переключатель дл€ UseOllivierRicciForWeyl в проект UI на панель и в plugins (IncludedPluginsRegistry.cs и UniPipelineForm).
Х	ƒобавить GPU-ускорЄнную версию формировани€ per-node anisotropy (вызов EdgeAnisotropyKernelDouble) и интеграцию с ComputeSharp 



==================


то Ќ≈ удалось доделать сейчас (и почему):
A) Standalone DX12: Унет waiting for session dataФ + Унет сбора метрик в dx12 во внешнем окнеФ
Х	Ќужно править Form_Rsim3DForm.Rendering.cs.
Х	‘айл читаетс€, но правки через editor-операции у мен€ в этой сессии стабильно не примен€ютс€ (УCould not get text viewЕФ). ѕоэтому € не смог внести патч в standalone пр€мо сейчас. ѕланируемый минимальный фикс там:
Х	в DrawGraph() сделать проверку Унет данныхФ более жЄсткой: (_nodeCount <= 0 || _nodeX/_nodeY/_nodeZ is null) чтобы сообщение не пропадало при stale _nodeCount.
Х	текст заменить на УWaiting for session dataЕФ (как ты просишь).
B) Уќтсутствует панель с ползунками ниже _tlp_UniPipeline_MainФ
Х	Form_Main_UniPipelineState.cs сейчас пустой.
Х	„тобы восстановить панель корректно, надо пон€ть где она была: в Designer (Form_Main.cs/InitializeComponent()) или добавл€лась кодом в Form_Main.UiInit.cs/других partial.
Х	Ёто требует отдельного прохода по layout-инициализации (и возможно правки designer-кода), сейчас не сделано.
C) У¬изуализаци€ во внутреннем и внешнем dx12 окне различна (фиолетовые 3 структуры vs 1 желтый кластер)Ф
Х	ѕосле правок выше различи€ должны уменьшитьс€ (цвета/edge-style унифицированы).
Х	Ќо полностью У1 vs 3 кластераФ почти наверн€ка св€зано не с рендерингом, а с разными источниками данных/разной кластеризацией:
Х	embedded берЄт ActiveGraph / Graph / external shared memory nodes,
Х	standalone получает GraphRenderData через _getGraphData. Ёто надо отдельно сравнивать на одном и том же датасете.
namespace Dx12WinForm.ProcessesDispatcher.Contracts;

public enum SimCommandType
{
    Handshake = 0,
    Start = 1,
    Pause = 2,
    Step = 3,
    UpdateSettings = 4,
    GetMultiGpuStatus = 10,
    Shutdown = 99,
    Stop = 100
}

public sealed class SimCommand
{
    public SimCommandType Type { get; init; }

    /// <summary>
    /// Optional JSON payload with command parameters/settings.
    /// </summary>
    public string? PayloadJson { get; init; }
}

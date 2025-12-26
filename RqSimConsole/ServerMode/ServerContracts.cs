using System.Runtime.InteropServices;

namespace RqSimConsole.ServerMode;

internal enum SimCommandType
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

internal sealed class SimCommand
{
    public SimCommandType Type { get; init; }
    public string? PayloadJson { get; init; }
}

internal sealed class ServerModeSettingsDto
{
    public int NodeCount { get; init; }
    public int TargetDegree { get; init; }
    public int Seed { get; init; }
    public double Temperature { get; init; }

    public static ServerModeSettingsDto Default { get; } = new()
    {
        NodeCount = 1000,
        TargetDegree = 4,
        Seed = 42,
        Temperature = 1.0
    };
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SharedHeader
{
    public long Iteration;
    public int NodeCount;
    public int EdgeCount;
    public double SystemEnergy;
    public int StateCode;
    public long LastUpdateTimestampUtcTicks;

    // Multi-GPU cluster info
    public int GpuClusterSize;
    public int BusySpectralWorkers;
    public int BusyMcmcWorkers;
    public double LatestSpectralDimension;
    public double LatestMcmcEnergy;
    public int TotalSpectralResults;
    public int TotalMcmcResults;
    
    // Extended simulation metrics
    public int ExcitedCount;
    public double HeavyMass;
    public int LargestCluster;
    public int StrongEdgeCount;
    public double QNorm;
    public double Entanglement;
    public double Correlation;
    public double NetworkTemperature;
    public double EffectiveG;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RenderNode
{
    public float X;
    public float Y;
    public float Z;
    public float R;
    public float G;
    public float B;
    public int Id;
}

/// <summary>
/// Multi-GPU cluster status for IPC/serialization.
/// </summary>
internal sealed class MultiGpuStatusDto
{
    public int TotalDevices { get; init; }
    public string PhysicsDeviceName { get; init; } = "";
    public int SpectralWorkerCount { get; init; }
    public int McmcWorkerCount { get; init; }
    public int BusySpectralWorkers { get; init; }
    public int BusyMcmcWorkers { get; init; }
    public bool IsDoublePrecisionSupported { get; init; }
    public long TotalVramMb { get; init; }
    public WorkerStatusDto[] SpectralWorkers { get; init; } = [];
    public WorkerStatusDto[] McmcWorkers { get; init; } = [];
}

/// <summary>
/// Individual worker status for IPC/serialization.
/// </summary>
internal sealed class WorkerStatusDto
{
    public int WorkerId { get; init; }
    public string DeviceName { get; init; } = "";
    public bool IsBusy { get; init; }
    public long LastResultTick { get; init; }
    public double? Beta { get; init; }
    public double? Temperature { get; init; }
}

/// <summary>
/// Spectral dimension result for IPC/serialization.
/// </summary>
internal sealed class SpectralResultDto
{
    public double SpectralDimension { get; init; }
    public long TickId { get; init; }
    public int WorkerId { get; init; }
    public double ComputeTimeMs { get; init; }
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public bool IsValid { get; init; }
}

/// <summary>
/// MCMC result for IPC/serialization.
/// </summary>
internal sealed class McmcResultDto
{
    public double MeanEnergy { get; init; }
    public double StdEnergy { get; init; }
    public double MeanAcceptanceRate { get; init; }
    public long TickId { get; init; }
    public int WorkerId { get; init; }
    public double Beta { get; init; }
    public double Temperature { get; init; }
    public double ComputeTimeMs { get; init; }
}

using System.Runtime.InteropServices;

namespace Dx12WinForm.ProcessesDispatcher.Contracts;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SharedHeader
{
    public long Iteration;
    public int NodeCount;
    public int EdgeCount;
    public double SystemEnergy;
    public int StateCode; // cast to SimulationStatus
    public long LastUpdateTimestampUtcTicks;

    // Multi-GPU cluster info - must match Console's SharedHeader
    public int GpuClusterSize;
    public int BusySpectralWorkers;
    public int BusyMcmcWorkers;
    public double LatestSpectralDimension;
    public double LatestMcmcEnergy;
    public int TotalSpectralResults;
    public int TotalMcmcResults;
    
    // Extended simulation metrics - must match Console's SharedHeader
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
public struct RenderNode
{
    public float X;
    public float Y;
    public float Z;
    public float R;
    public float G;
    public float B;
    public int Id;
}

public static class SharedMemoryLayout
{
    public static int HeaderSize { get; } = Marshal.SizeOf<SharedHeader>();
}

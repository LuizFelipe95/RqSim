using RQSimulation;

namespace RqSim.PluginManager.UI.IncludedPlugins.CPU;

/// <summary>
/// CPU module for MCMC sampler for path integral quantum gravity.
/// Samples configurations satisfying Wheeler-DeWitt constraint.
/// 
/// PHYSICS:
/// - Instead of time evolution, we sample configurations from the partition function
/// - Z = ? D[g] exp(-S_E[g]) where S_E is the Euclidean action
/// - Metropolis-Hastings acceptance: P_accept = exp(-(S_new - S_old))
/// 
/// Based on original MCMCSampler implementation.
/// </summary>
public sealed class MCMCSamplerCpuModule : CpuPluginBase
{
    private RQGraph? _graph;
    private Random _rng = new(42);
    private double _currentAction;

    public override string Name => "MCMC Sampler (CPU)";
    public override string Description => "CPU-based Markov Chain Monte Carlo for path integral quantum gravity";
    public override string Category => "MCMC";
    public override int Priority => 45;

    /// <summary>
    /// Number of MCMC samples per simulation step.
    /// </summary>
    public int SamplesPerStep { get; set; } = 10;

    /// <summary>
    /// Inverse temperature (beta = 1/kT).
    /// </summary>
    public double Beta { get; set; } = 1.0;

    /// <summary>
    /// Weight perturbation magnitude for change moves.
    /// </summary>
    public double WeightPerturbation { get; set; } = 0.1;

    /// <summary>
    /// Minimum weight threshold below which edge is removed.
    /// </summary>
    public double MinWeight { get; set; } = 0.01;

    // Statistics
    public int AcceptedMoves { get; private set; }
    public int RejectedMoves { get; private set; }
    public double AcceptanceRate => AcceptedMoves + RejectedMoves > 0
        ? (double)AcceptedMoves / (AcceptedMoves + RejectedMoves) : 0.0;

    public override void Initialize(RQGraph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _rng = new Random(42);
        _currentAction = CalculateEuclideanAction();
        AcceptedMoves = 0;
        RejectedMoves = 0;
    }

    /// <summary>
    /// Calculate Euclidean action for current configuration.
    /// S_E = S_gravity + S_matter + S_gauge
    /// </summary>
    public double CalculateEuclideanAction()
    {
        if (_graph is null) return 0.0;

        // Use the constraint-weighted Hamiltonian as the effective Euclidean action
        return _graph.ComputeConstraintWeightedHamiltonian();
    }

    public override void ExecuteStep(RQGraph graph, double dt)
    {
        if (_graph is null) return;

        // Perform MCMC sampling
        SampleConfigurationSpace(SamplesPerStep, null);
    }

    /// <summary>
    /// Sample configuration space using Metropolis-Hastings.
    /// </summary>
    public void SampleConfigurationSpace(int samples, Action<int, RQGraph>? onSample = null)
    {
        if (_graph is null) return;

        for (int i = 0; i < samples; i++)
        {
            var (deltaAction, applyMove, _) = ProposeMove();

            // Metropolis acceptance criterion
            // P_accept = min(1, exp(-beta * deltaS))
            bool accept = false;
            if (deltaAction <= 0)
            {
                accept = true;
            }
            else
            {
                double p = Math.Exp(-Beta * deltaAction);
                if (_rng.NextDouble() < p)
                {
                    accept = true;
                }
            }

            if (accept)
            {
                applyMove();
                _currentAction += deltaAction;
                AcceptedMoves++;
            }
            else
            {
                RejectedMoves++;
            }

            onSample?.Invoke(i, _graph);
        }
    }

    /// <summary>
    /// Propose topology change (edge addition/removal/weight change).
    /// Returns proposed action change without applying.
    /// </summary>
    public (double deltaAction, Action applyMove, Action revertMove) ProposeMove()
    {
        if (_graph is null)
            return (0, () => { }, () => { });

        // Select random move type: Add Edge, Remove Edge, Change Weight
        int moveType = _rng.Next(3);

        int i = _rng.Next(_graph.N);
        int j = _rng.Next(_graph.N);
        while (i == j) j = _rng.Next(_graph.N);

        // Ensure i < j for consistency
        if (i > j) (i, j) = (j, i);

        bool edgeExists = _graph.Edges[i, j];
        double currentWeight = _graph.Weights[i, j];

        // Adjust move type based on existence
        if (moveType == 0 && edgeExists) moveType = 2; // Can't add, so change weight
        if (moveType == 1 && !edgeExists) moveType = 0; // Can't remove, so add

        double newWeight = currentWeight;
        bool newExists = edgeExists;

        if (moveType == 0) // Add Edge
        {
            newExists = true;
            newWeight = _rng.NextDouble(); // Random weight 0-1
        }
        else if (moveType == 1) // Remove Edge
        {
            newExists = false;
            newWeight = 0.0;
        }
        else // Change Weight
        {
            // Small perturbation
            newWeight = currentWeight + (_rng.NextDouble() - 0.5) * WeightPerturbation;
            newWeight = Math.Clamp(newWeight, 0.0, 1.0);
            if (newWeight < MinWeight) // Threshold for removal
            {
                newExists = false;
                newWeight = 0.0;
            }
        }

        // Apply move temporarily
        _graph.Edges[i, j] = newExists;
        _graph.Edges[j, i] = newExists;
        _graph.Weights[i, j] = newWeight;
        _graph.Weights[j, i] = newWeight;

        double newAction = CalculateEuclideanAction();
        double delta = newAction - _currentAction;

        // Revert immediately so we return the "proposal"
        _graph.Edges[i, j] = edgeExists;
        _graph.Edges[j, i] = edgeExists;
        _graph.Weights[i, j] = currentWeight;
        _graph.Weights[j, i] = currentWeight;

        // Capture values for closures
        int ci = i, cj = j;
        bool cNewExists = newExists, cEdgeExists = edgeExists;
        double cNewWeight = newWeight, cCurrentWeight = currentWeight;

        Action apply = () =>
        {
            _graph.Edges[ci, cj] = cNewExists;
            _graph.Edges[cj, ci] = cNewExists;
            _graph.Weights[ci, cj] = cNewWeight;
            _graph.Weights[cj, ci] = cNewWeight;
        };

        Action revert = () =>
        {
            _graph.Edges[ci, cj] = cEdgeExists;
            _graph.Edges[cj, ci] = cEdgeExists;
            _graph.Weights[ci, cj] = cCurrentWeight;
            _graph.Weights[cj, ci] = cCurrentWeight;
        };

        return (delta, apply, revert);
    }

    /// <summary>
    /// Reset MCMC statistics.
    /// </summary>
    public void ResetStatistics()
    {
        AcceptedMoves = 0;
        RejectedMoves = 0;
    }

    /// <summary>
    /// Set random seed for reproducibility.
    /// </summary>
    public void SetSeed(int seed)
    {
        _rng = new Random(seed);
    }

    public override void Cleanup()
    {
        _graph = null;
        ResetStatistics();
    }
}

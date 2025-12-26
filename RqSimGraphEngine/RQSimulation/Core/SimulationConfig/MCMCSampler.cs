using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RQSimulation
{
    /// <summary>
    /// MCMC sampler for path integral quantum gravity.
    /// Samples configurations satisfying Wheeler-DeWitt constraint.
    /// 
    /// PHYSICS:
    /// - Instead of time evolution, we sample configurations from the partition function
    /// - Z = ? D[g] exp(-S_E[g]) where S_E is the Euclidean action
    /// - Metropolis-Hastings acceptance: P_accept = exp(-(S_new - S_old))
    /// </summary>
    public sealed class MCMCSampler
    {
        private readonly RQGraph _graph;
        private readonly Random _rng;
        private double _currentAction;
        
        // Statistics
        public int AcceptedMoves { get; private set; }
        public int RejectedMoves { get; private set; }
        public double AcceptanceRate => AcceptedMoves + RejectedMoves > 0 
            ? (double)AcceptedMoves / (AcceptedMoves + RejectedMoves) : 0.0;
        
        public MCMCSampler(RQGraph graph, int seed = 42)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _rng = new Random(seed);
            _currentAction = CalculateEuclideanAction();
        }
        
        /// <summary>
        /// Calculate Euclidean action for current configuration.
        /// S_E = S_gravity + S_matter + S_gauge
        /// </summary>
        public double CalculateEuclideanAction()
        {
            // Use the constraint-weighted Hamiltonian as the effective Euclidean action
            // This ensures we sample configurations near the constraint surface H ? 0
            return _graph.ComputeConstraintWeightedHamiltonian();
        }
        
        /// <summary>
        /// Sample configuration space using Metropolis-Hastings.
        /// </summary>
        public void SampleConfigurationSpace(int samples, Action<int, RQGraph>? onSample = null)
        {
            for (int i = 0; i < samples; i++)
            {
                var (deltaAction, applyMove, revertMove) = ProposeMove();
                
                // Metropolis acceptance criterion
                // P_accept = min(1, exp(-deltaS))
                // If deltaS < 0 (action decreases), exp(-deltaS) > 1 -> always accept
                // If deltaS > 0 (action increases), accept with probability exp(-deltaS)
                
                bool accept = false;
                if (deltaAction <= 0)
                {
                    accept = true;
                }
                else
                {
                    double p = Math.Exp(-deltaAction);
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
                    // Move was already applied in ProposeMove? 
                    // Wait, ProposeMove returns actions to apply/revert.
                    // If I apply it inside ProposeMove to calculate delta, I need to revert if rejected.
                    // Or ProposeMove calculates delta WITHOUT applying?
                    // Calculating delta usually requires applying and checking, or calculating local change.
                    // Let's assume ProposeMove returns the delta and the actions.
                    // If ProposeMove calculates delta *without* applying, then I call applyMove if accepted.
                    // If ProposeMove *applies* to calculate delta, then I call revertMove if rejected.
                    
                    // Let's implement ProposeMove such that it calculates delta efficiently (local update)
                    // without applying global changes if possible, or applies and returns a revert action.
                    
                    // However, the signature says: (double deltaAction, Action applyMove, Action revertMove)
                    // This suggests the move is NOT applied yet, or at least the `applyMove` action is what applies it.
                    // But how to calculate deltaAction without applying?
                    // We need local action calculation.
                    
                    // If ProposeMove returns `applyMove`, then `applyMove` must be called to change the state.
                    // But `deltaAction` must be known before deciding.
                    // So `ProposeMove` must calculate the potential change in action.
                    
                    // If I can't calculate delta without applying, I might need to apply, calc, then revert.
                    // But `ProposeMove` signature implies it returns the *potential* delta.
                    
                    // Let's stick to: ProposeMove calculates delta (locally), returns actions to commit or nothing.
                    // But wait, if I don't apply, I don't need revert.
                    // The signature in the prompt is:
                    // (double deltaAction, Action applyMove, Action revertMove) ProposeMove();
                    
                    // This implies I get the delta, and I can choose to apply.
                    // Why revertMove? Maybe if I apply and then later decide to revert?
                    // Or maybe ProposeMove applies it tentatively?
                    
                    // Let's assume ProposeMove does NOT apply the move, but calculates the delta.
                    // Then `applyMove` applies it. `revertMove` might be null or unused if we don't apply.
                    // But if we accept, we call `applyMove`.
                    
                    // Wait, if ProposeMove doesn't apply, how do we get delta?
                    // We calculate H_new - H_old locally.
                    
                    // If rejected, we do nothing (since we didn't apply).
                    // So `revertMove` is only needed if `ProposeMove` APPLIES the change.
                    
                    // Let's implement it such that ProposeMove calculates delta locally without modifying state.
                    // Then `applyMove` modifies state. `revertMove` is not needed in this flow, but I'll keep the signature.
                    
                    // Actually, calculating delta exactly might require applying for complex Hamiltonians.
                    // Let's try to implement local calculation.
                }
                
                if (!accept)
                {
                    RejectedMoves++;
                    // If we didn't apply, we don't need to revert.
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
            // 1. Select random move type: Add Edge, Remove Edge, Change Weight
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
                newWeight = currentWeight + (_rng.NextDouble() - 0.5) * 0.1;
                newWeight = Math.Clamp(newWeight, 0.0, 1.0);
                if (newWeight < 0.01) // Threshold for removal
                {
                    newExists = false;
                    newWeight = 0.0;
                }
            }
            
            // Calculate delta action locally
            // S = H_standard + lambda * constraint
            // We need delta H_standard and delta constraint
            
            // H_standard has H_links and H_nodes.
            // H_links depends on edges. H_nodes depends on mass (which might depend on topology if correlation mass).
            // Assuming H_nodes is constant for topology change (or we neglect it for now/it's expensive).
            // Actually H_matter (correlation mass) depends on topology.
            // But let's assume we only calculate delta for H_links and Constraint.
            
            // Calculating exact delta constraint is expensive (requires recalculating curvature).
            // We can use ComputeLocalConstraintContribution if available.
            
            // Let's calculate delta by temporarily applying the change?
            // If I apply, I must revert if rejected.
            // This is safer for correctness.
            
            // Apply move
            _graph.Edges[i, j] = newExists;
            _graph.Edges[j, i] = newExists;
            _graph.Weights[i, j] = newWeight;
            _graph.Weights[j, i] = newWeight;
            
            // Recalculate local properties if needed (e.g. degrees)
            // RQGraph might need a method to update local state efficiently.
            // For now, let's assume we can just set them.
            // But wait, curvature depends on neighbors.
            // If I change edge, I change curvature at i and j and their neighbors.
            
            // To get EXACT delta, I need to recompute action.
            // Since this is MCMC, exactness is key.
            // But full recompute is O(N) or O(N^2).
            // For now, let's do full recompute of action (slow but correct).
            // Optimization can be done later (Stage 1 doesn't specify optimization).
            
            double newAction = CalculateEuclideanAction();
            double delta = newAction - _currentAction;
            
            // Revert immediately so we return the "proposal"
            _graph.Edges[i, j] = edgeExists;
            _graph.Edges[j, i] = edgeExists;
            _graph.Weights[i, j] = currentWeight;
            _graph.Weights[j, i] = currentWeight;
            
            Action apply = () => 
            {
                _graph.Edges[i, j] = newExists;
                _graph.Edges[j, i] = newExists;
                _graph.Weights[i, j] = newWeight;
                _graph.Weights[j, i] = newWeight;
                // Note: If we had cached properties (degrees, etc), we should update them here.
                // Ideally RQGraph should have UpdateEdge(i, j, weight) method.
            };
            
            Action revert = () =>
            {
                _graph.Edges[i, j] = edgeExists;
                _graph.Edges[j, i] = edgeExists;
                _graph.Weights[i, j] = currentWeight;
                _graph.Weights[j, i] = currentWeight;
            };
            
            return (delta, apply, revert);
        }
    }
}

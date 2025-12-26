using System;
using System.Collections.Generic;
using System.Linq;

namespace RQSimulation
{
    public partial class RQGraph
    {
        /// <summary>
        /// Calculate graph curvature for edge (i,j) using Ollivier-Ricci or Forman-Ricci approximation
        /// Positive curvature indicates clustering, negative indicates tree-like structure
        /// </summary>
        public double CalculateGraphCurvature(int i, int j)
        {
            // RQ-HYPOTHESIS FIX (Item 5): Use Ollivier-Ricci curvature by default
            // Forman-Ricci is too coarse for deformed lattices.
            // We delegate to the GPU-optimized implementation (which runs on CPU if GPU not avail).
            return GPUOptimized.OllivierRicciCurvature.ComputeOllivierRicciJaccard(this, i, j);
        }

        /// <summary>
        /// Calculate local volume (weighted degree) of a node.
        /// Used for volume constraint in gravity.
        /// </summary>
        public double GetLocalVolume(int i)
        {
            double vol = 0.0;
            foreach (int j in Neighbors(i))
            {
                vol += Weights[i, j];
            }
            return vol;
        }

        /// <summary>
        /// Calculate Ollivier-Ricci curvature for edge (i,j)
        /// This is the NEW implementation (CHECKLIST ITEM 4)
        /// More sensitive to geometry than Forman-Ricci
        /// </summary>
        public double CalculateOllivierRicciCurvature(int i, int j)
        {
            // Delegate to GPU-optimized implementation
            return GPUOptimized.OllivierRicciCurvature.ComputeOllivierRicciJaccard(this, i, j);
        }

        /// <summary>
        /// Compute average curvature of the network
        /// </summary>
        public double ComputeAverageCurvature()
        {
            if (Edges == null || Weights == null)
                return 0.0;

            double totalCurvature = 0.0;
            int edgeCount = 0;

            for (int i = 0; i < N; i++)
            {
                foreach (int j in Neighbors(i))
                {
                    if (j <= i) continue; // Count each edge once

                    totalCurvature += CalculateGraphCurvature(i, j);
                    edgeCount++;
                }
            }

            return edgeCount > 0 ? totalCurvature / edgeCount : 0.0;
        }

        /// <summary>
        /// Compute curvature scalar (sum of all edge curvatures)
        /// Analogous to Ricci scalar R in GR
        /// </summary>
        public double ComputeCurvatureScalar()
        {
            if (Edges == null || Weights == null)
                return 0.0;

            double scalar = 0.0;

            for (int i = 0; i < N; i++)
            {
                foreach (int j in Neighbors(i))
                {
                    if (j <= i) continue;

                    scalar += CalculateGraphCurvature(i, j);
                }
            }

            return scalar;
        }
    }
}

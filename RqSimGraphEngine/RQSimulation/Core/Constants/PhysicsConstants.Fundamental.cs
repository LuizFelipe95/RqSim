using System;

namespace RQSimulation
{
    /// <summary>
    /// Physics constants and configuration parameters.
    /// 
    /// DIMENSIONAL ANALYSIS (Checklist G.3):
    /// =====================================
    /// All constants are expressed in natural Planck units where:
    ///   c = 1 (speed of light)
    ///   ? = 1 (reduced Planck constant)
    ///   G = 1 (gravitational constant)
    ///   k_B = 1 (Boltzmann constant)
    /// 
    /// In these units:
    ///   - Length: l_P = ?(?G/c?) = 1.616 ? 10??? m ? 1
    ///   - Time:   t_P = ?(?G/c?) = 5.391 ? 10??? s ? 1
    ///   - Mass:   m_P = ?(?c/G) = 2.176 ? 10?? kg ? 1
    ///   - Energy: E_P = m_P c? = 1.956 ? 10? J ? 1
    ///   - Temperature: T_P = E_P/k_B = 1.417 ? 10?? K ? 1
    /// 
    /// Physical coupling constants (CODATA 2022):
    ///   - Fine structure constant: ? = e?/(4????c) = 1/137.035999084(21)
    ///   - Strong coupling: ?_s(M_Z) = 0.1180(9)
    ///   - Electroweak mixing: sin??_W(M_Z) = 0.23121(4)
    ///   - Fermi constant: G_F/(?c)? = 1.1663787(6) ? 10?? GeV??
    /// </summary>
    public static partial class PhysicsConstants
    {
        // ============================================================
        // FUNDAMENTAL CONSTANTS (Planck units: c = ? = G = k_B = 1)
        // ============================================================

        /// <summary>Speed of light in Planck units (c = 1)</summary>
        public const double C = 1.0;

        /// <summary>Reduced Planck constant in Planck units (? = 1)</summary>
        public const double HBar = 1.0;

        /// <summary>Gravitational constant in Planck units (G = 1)</summary>
        public const double G = 1.0;

        /// <summary>Boltzmann constant in Planck units (k_B = 1)</summary>
        public const double KBoltzmann = 1.0;

        /// <summary>Planck length (fundamental length scale) = 1 in natural units</summary>
        public const double PlanckLength = 1.0;

        /// <summary>Planck time (fundamental time scale) = 1 in natural units</summary>
        public const double PlanckTime = 1.0;

        /// <summary>Planck mass (fundamental mass scale) = 1 in natural units</summary>
        public const double PlanckMass = 1.0;

        /// <summary>Planck energy (fundamental energy scale) = 1 in natural units</summary>
        public const double PlanckEnergy = 1.0;

        /// <summary>Planck temperature = E_P/k_B = 1 in natural units</summary>
        public const double PlanckTemperature = 1.0;

        // ============================================================
        // GAUGE COUPLING CONSTANTS (dimensionless, from CODATA 2022)
        // ============================================================

        /// <summary>
        /// Electromagnetic fine structure constant ? = e?/(4????c).
        /// CODATA 2022 value: ? = 1/137.035999084(21)
        /// This is the fundamental measure of electromagnetic coupling.
        /// </summary>
        public const double FineStructureConstant = 1.0 / 137.035999084;

        /// <summary>
        /// Electromagnetic coupling constant squared: ? ? 0.00729735...
        /// Often used directly in QED calculations.
        /// </summary>
        public static readonly double AlphaEM = FineStructureConstant;

        /// <summary>
        /// Strong coupling constant ?_s at Z mass scale (M_Z ? 91.2 GeV).
        /// PDG 2022 value: ?_s(M_Z) = 0.1180 ± 0.0009
        /// Note: ?_s runs with energy scale (asymptotic freedom).
        /// </summary>
        public const double StrongCouplingConstant = 0.1180;

        /// <summary>
        /// Electroweak mixing angle (Weinberg angle) sin??_W at M_Z.
        /// PDG 2022 value: sin??_W = 0.23121 ± 0.00004
        /// Determines W/Z mass ratio: M_W/M_Z = cos ?_W
        /// </summary>
        public const double WeakMixingAngle = 0.23121;

        /// <summary>
        /// Weak coupling constant g_W derived from ? and ?_W.
        /// g_W = e / sin ?_W = ?(4??) / sin ?_W
        /// At M_Z: g_W ? 0.653
        /// </summary>
        public static readonly double WeakCouplingConstant =
            Math.Sqrt(4 * Math.PI * FineStructureConstant) / Math.Sqrt(WeakMixingAngle);

        /// <summary>
        /// Hypercharge coupling g' = e / cos ?_W
        /// g' ? 0.357 at M_Z
        /// </summary>
        public static readonly double HyperchargeCoupling =
            Math.Sqrt(4 * Math.PI * FineStructureConstant) / Math.Sqrt(1 - WeakMixingAngle);

        /// <summary>
        /// Strong coupling constant g_s = ?(4??_s)
        /// At M_Z: g_s ? 1.22
        /// </summary>
        public static readonly double StrongCoupling = Math.Sqrt(4 * Math.PI * StrongCouplingConstant);

        // ============================================================
        // MASS RATIOS (dimensionless, from PDG 2022)
        // ============================================================

        /// <summary>
        /// Electron mass in Planck mass units: m_e/m_P ? 4.18 ? 10???
        /// This is the fundamental mass scale for QED effects.
        /// </summary>
        public const double ElectronMassRatio = 4.1855e-23;

        /// <summary>
        /// Proton mass in Planck mass units: m_p/m_P ? 7.68 ? 10???
        /// </summary>
        public const double ProtonMassRatio = 7.685e-20;

        /// <summary>
        /// W boson mass in Planck mass units: M_W/m_P ? 6.58 ? 10???
        /// </summary>
        public const double WBosonMassRatio = 6.58e-18;

        /// <summary>
        /// Z boson mass in Planck mass units: M_Z/m_P ? 7.47 ? 10???
        /// </summary>
        public const double ZBosonMassRatio = 7.47e-18;

        /// <summary>
        /// Higgs boson mass in Planck mass units: M_H/m_P ? 1.02 ? 10???
        /// </summary>
        public const double HiggsBosonMassRatio = 1.02e-17;

        // ============================================================
        // DERIVED CONSTANTS & SIMULATION PARAMETERS
        // ============================================================

        // === Graph Topology ===
        // RQ-COMPLIANT: Thresholds are NOT hardcoded but computed adaptively from graph statistics.
        // The "default" values below are ONLY used when adaptive computation fails.

        /// <summary>
        /// Fallback heavy cluster threshold. In RQ-compliant mode, use adaptive threshold:
        /// threshold = mean(weights) + AdaptiveThresholdSigma * stddev(weights)
        /// This value is NEVER used directly in physics - only as emergency fallback.
        /// </summary>
        public const double DefaultHeavyClusterThreshold = 0.5; // Neutral: mean of [0,1]

        /// <summary>
        /// Standard deviations above mean for adaptive heavy threshold.
        /// Value 1.5? means ~7% of edges are "heavy" (statistical definition).
        /// This is a STATISTICAL parameter, not a physical one.
        /// </summary>
        public const double AdaptiveThresholdSigma = 1.5;

        // ============================================================
        // COSMOLOGICAL CONSTANTS (dimensionless ratios)
        // ============================================================

        /// <summary>
        /// Cosmological constant in Planck units: ?/l_P? ? 2.88 ? 10????
        /// This is the famous "cosmological constant problem" - 
        /// the observed value is ~120 orders of magnitude smaller than naive QFT prediction.
        /// For simulation, we use a much larger value for numerical tractability.
        /// </summary>
        public const double CosmologicalConstantPhysical = 2.88e-122;

        /// <summary>
        /// Simulation cosmological constant - scaled up for numerical stability.
        /// Represents effective dark energy contribution in the simulation.
        /// </summary>
        //TODO разница на 140 порядков - слишком много?

        public const double CosmologicalConstantSimulation = 1e-4;

        // ============================================================
        // QUANTUM INFORMATION CONSTANTS
        // ============================================================

        /// <summary>
        /// Landauer limit: minimum energy to erase one bit of information.
        /// E_min = k_B T ln(2). In Planck units with k_B = 1 and T ~ 1: E_min = ln(2) ? 0.693
        /// </summary>
        public const double LandauerLimit = 0.693147180559945; // ln(2)

        /// <summary>
        /// Bekenstein bound coefficient: maximum entropy of a region.
        /// S_max = 2? k_B E R / (? c). In natural units: S_max = 2? E R
        /// </summary>
        public const double BekensteinCoefficient = 2 * Math.PI;

        /// <summary>
        /// Black hole entropy coefficient: S_BH = A / (4 l_P?)
        /// The famous Bekenstein-Hawking entropy formula.
        /// </summary>
        public const double BlackHoleEntropyCoefficient = 0.25;
    }
}

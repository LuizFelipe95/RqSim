using RQSimulation;

namespace RqSimGPUCPUTests;

/// <summary>
/// Unit tests for Wheeler-DeWitt Constraint implementation (RQ-Hypothesis Stage 2).
/// Tests the constraint enforcement: H_total = H_gravity + ? * H_matter ? 0.
/// </summary>
public partial class RqSimGPUCPUTest
{
    // ============================================================
    // WHEELER-DEWITT CONSTANTS TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void WheelerDeWittConstants_DefaultValues_AreValid()
    {
        // Verify Wheeler-DeWitt constants have valid values
        Assert.IsTrue(PhysicsConstants.WheelerDeWittConstants.GravitationalCoupling > 0.0,
            "GravitationalCoupling (?) should be positive");
        Assert.IsTrue(PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier > 0.0,
            "ConstraintLagrangeMultiplier (?) should be positive");
        Assert.IsTrue(PhysicsConstants.WheelerDeWittConstants.ConstraintTolerance > 0.0,
            "ConstraintTolerance should be positive");
        Assert.IsTrue(PhysicsConstants.WheelerDeWittConstants.MaxAllowedViolation > 0.0,
            "MaxAllowedViolation should be positive");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void WheelerDeWittConstants_StrictModeDisabledByDefault()
    {
        // Strict mode should be off by default for backward compatibility
        Assert.IsFalse(PhysicsConstants.WheelerDeWittConstants.EnableStrictMode,
            "EnableStrictMode should be false by default for backward compatibility");
    }
    
    // ============================================================
    // LOCAL CONSTRAINT CALCULATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void CalculateWheelerDeWittConstraint_EmptyGraph_ReturnsNonNegative()
    {
        // Empty graph should have non-negative constraint violation (squared)
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        double constraint = graph.CalculateWheelerDeWittConstraint(0);
        
        // Squared constraint violation should be non-negative
        Assert.IsTrue(constraint >= 0.0, 
            "Squared constraint violation should be non-negative");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void CalculateWheelerDeWittConstraint_InvalidNodeId_ThrowsException()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        bool exceptionThrown = false;
        try
        {
            graph.CalculateWheelerDeWittConstraint(-1);
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }
        Assert.IsTrue(exceptionThrown, "Should throw ArgumentOutOfRangeException for negative nodeId");
        
        exceptionThrown = false;
        try
        {
            graph.CalculateWheelerDeWittConstraint(100);
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }
        Assert.IsTrue(exceptionThrown, "Should throw ArgumentOutOfRangeException for nodeId >= N");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void CalculateWheelerDeWittConstraint_ReturnsSquaredValue()
    {
        // Constraint violation should always be squared (non-negative)
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        // Create some structure
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        for (int i = 0; i < graph.N; i++)
        {
            double constraint = graph.CalculateWheelerDeWittConstraint(i);
            Assert.IsTrue(constraint >= 0.0, 
                $"Constraint violation at node {i} should be non-negative (squared)");
        }
    }
    
    // ============================================================
    // TOTAL CONSTRAINT VIOLATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void CalculateTotalConstraintViolation_EmptyGraph_ReturnsSmallValue()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        double violation = graph.CalculateTotalConstraintViolation();
        
        Assert.IsTrue(violation >= 0.0, "Total violation should be non-negative");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void CalculateTotalConstraintViolation_IsNormalized()
    {
        // Verify normalization: total is divided by N
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        double total = graph.CalculateTotalConstraintViolation();
        
        // Calculate sum manually
        double sum = 0.0;
        for (int i = 0; i < graph.N; i++)
        {
            sum += graph.CalculateWheelerDeWittConstraint(i);
        }
        double expectedNormalized = sum / graph.N;
        
        Assert.AreEqual(expectedNormalized, total, 1e-10, 
            "Total violation should be normalized by N");
    }
    
    // ============================================================
    // SATISFIES CONSTRAINT TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void SatisfiesWheelerDeWittConstraint_EmptyGraph_ReturnsTrue()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        ClearAllEdges(graph);
        graph.BuildSoAViews();
        
        bool satisfies = graph.SatisfiesWheelerDeWittConstraint();
        
        // Empty graph should trivially satisfy constraint
        Assert.IsTrue(satisfies, "Empty graph should satisfy Wheeler-DeWitt constraint");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void SatisfiesWheelerDeWittConstraint_UsesTolerance()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double violation = graph.CalculateTotalConstraintViolation();
        double tolerance = PhysicsConstants.WheelerDeWittConstants.ConstraintTolerance;
        
        bool satisfies = graph.SatisfiesWheelerDeWittConstraint();
        bool expectedSatisfies = violation < tolerance;
        
        Assert.AreEqual(expectedSatisfies, satisfies, 
            $"SatisfiesWheelerDeWittConstraint should use tolerance check");
    }
    
    // ============================================================
    // CONSTRAINT-WEIGHTED HAMILTONIAN TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void ComputeConstraintWeightedHamiltonian_IncludesConstraintPenalty()
    {
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        double H_standard = graph.ComputeNetworkHamiltonian();
        double violation = graph.CalculateTotalConstraintViolation();
        double lambda = PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier;
        
        double H_weighted = graph.ComputeConstraintWeightedHamiltonian();
        double expected = H_standard + lambda * violation;
        
        Assert.AreEqual(expected, H_weighted, 1e-10, 
            "Weighted Hamiltonian should include constraint penalty");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void ComputeConstraintWeightedHamiltonian_GreaterOrEqualToStandard()
    {
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        double H_standard = graph.ComputeNetworkHamiltonian();
        double H_weighted = graph.ComputeConstraintWeightedHamiltonian();
        
        // Since ? > 0 and violation ? 0, weighted should be ? standard
        Assert.IsTrue(H_weighted >= H_standard, 
            "Weighted Hamiltonian should be >= standard (constraint penalty is non-negative)");
    }
    
    // ============================================================
    // CONSTRAINT COMPONENTS TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void GetConstraintComponents_ReturnsValidTuple()
    {
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        var (H_geom, H_matter, constraint) = graph.GetConstraintComponents(0);
        
        // Verify the constraint is calculated correctly
        double kappa = PhysicsConstants.WheelerDeWittConstants.GravitationalCoupling;
        double expectedConstraint = H_geom - kappa * H_matter;
        
        Assert.AreEqual(expectedConstraint, constraint, 1e-10, 
            "Constraint should be H_geom - ? * H_matter");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void GetConstraintComponents_InvalidNodeId_ThrowsException()
    {
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        bool exceptionThrown = false;
        try
        {
            graph.GetConstraintComponents(-1);
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }
        Assert.IsTrue(exceptionThrown, "Should throw ArgumentOutOfRangeException for negative nodeId");
        
        exceptionThrown = false;
        try
        {
            graph.GetConstraintComponents(100);
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrown = true;
        }
        Assert.IsTrue(exceptionThrown, "Should throw ArgumentOutOfRangeException for nodeId >= N");
    }
    
    // ============================================================
    // TOPOLOGY MOVE CONSTRAINT CHECK TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void IsTopologyMoveConstraintAllowed_NonStrictMode_AlwaysTrue()
    {
        // When strict mode is disabled, all moves should be allowed
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        // Strict mode is disabled by default
        Assert.IsFalse(PhysicsConstants.WheelerDeWittConstants.EnableStrictMode);
        
        bool allowed = graph.IsTopologyMoveConstraintAllowed(0, 1);
        
        Assert.IsTrue(allowed, "Non-strict mode should allow all moves");
    }
    
    // ============================================================
    // ENERGY LEDGER STRICT MODE TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictConservationMode_DefaultsFalse()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        
        Assert.IsFalse(ledger.StrictConservationMode, 
            "Strict conservation mode should be false by default");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictMode_BlocksVacuumSpending()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        
        bool result = ledger.TrySpendVacuumEnergy(10.0);
        
        Assert.IsFalse(result, "Strict mode should block vacuum energy spending");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictMode_LogsViolation()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        
        // Clear history first
        ledger.ClearConstraintViolationHistory();
        
        // Attempt blocked operation
        ledger.TrySpendVacuumEnergy(10.0);
        
        Assert.AreEqual(1, ledger.ConstraintViolationHistory.Count, 
            "Violation should be logged");
        Assert.AreEqual(10.0, ledger.ConstraintViolationHistory[0].Violation, 
            "Violation amount should match");
        Assert.IsTrue(ledger.ConstraintViolationHistory[0].Context.Contains("VacuumSpendAttempt"), 
            "Context should indicate vacuum spend attempt");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictMode_BlocksExternalInjection()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        
        double initialPool = ledger.VacuumPool;
        bool result = ledger.RecordExternalInjection(100.0, "TestInjection");
        
        Assert.IsFalse(result, "Strict mode should block external injection");
        Assert.AreEqual(initialPool, ledger.VacuumPool, 
            "Vacuum pool should not change when injection is blocked");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictMode_AllowsSurplusReturn()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        
        // Returning surplus (positive delta) should still work
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double initialPool = ledger.VacuumPool;
        bool result = ledger.TryAbsorbDeficit(50.0, graph); // Surplus
        
        Assert.IsTrue(result, "Strict mode should allow surplus return");
        Assert.AreEqual(initialPool + 50.0, ledger.VacuumPool, 1e-10, 
            "Vacuum pool should increase by surplus amount");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_StrictMode_BlocksDeficitAbsorption()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        
        var config = new SimulationConfig { NodeCount = 10, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        double initialPool = ledger.VacuumPool;
        bool result = ledger.TryAbsorbDeficit(-50.0, graph); // Deficit
        
        Assert.IsFalse(result, "Strict mode should block deficit absorption");
        Assert.AreEqual(initialPool, ledger.VacuumPool, 
            "Vacuum pool should not change when deficit is blocked");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_NonStrictMode_AllowsNormalOperations()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        // StrictConservationMode is false by default
        
        bool spendResult = ledger.TrySpendVacuumEnergy(10.0);
        Assert.IsTrue(spendResult, "Non-strict mode should allow spending");
        
        bool injectResult = ledger.RecordExternalInjection(10.0, "Test");
        Assert.IsTrue(injectResult, "Non-strict mode should allow injection");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void EnergyLedger_ViolationStatistics_Calculated()
    {
        var ledger = new EnergyLedger();
        ledger.Initialize(1000.0);
        ledger.StrictConservationMode = true;
        ledger.ClearConstraintViolationHistory();
        
        // Generate some violations
        ledger.TrySpendVacuumEnergy(10.0);
        ledger.TrySpendVacuumEnergy(20.0);
        ledger.TrySpendVacuumEnergy(30.0);
        
        var (count, total, max, avg) = ledger.GetViolationStatistics();
        
        Assert.AreEqual(3, count, "Count should be 3");
        Assert.AreEqual(60.0, total, 1e-10, "Total should be 60");
        Assert.AreEqual(30.0, max, 1e-10, "Max should be 30");
        Assert.AreEqual(20.0, avg, 1e-10, "Average should be 20");
    }
    
    // ============================================================
    // INTEGRATION TESTS
    // ============================================================
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void WheelerDeWitt_ConstraintViolation_IsNonNegative()
    {
        // Both regular and random graphs should have non-negative violations
        
        var config1 = new SimulationConfig { NodeCount = 16, Seed = TestSeed };
        var engine1 = new SimulationEngine(config1);
        var graph1 = engine1.Graph;
        
        // Create regular grid (symmetric)
        CreateWheelerDeWittTriangleGrid(graph1, 4);
        double violation1 = graph1.CalculateTotalConstraintViolation();
        Assert.IsTrue(violation1 >= 0.0, "Grid violation should be non-negative");
        
        var config2 = new SimulationConfig { NodeCount = 16, Seed = TestSeed };
        var engine2 = new SimulationEngine(config2);
        var graph2 = engine2.Graph;
        
        // Create random structure (asymmetric)
        ClearAllEdges(graph2);
        var rng = new Random(TestSeed);
        for (int i = 0; i < 30; i++)
        {
            int a = rng.Next(16);
            int b = rng.Next(16);
            if (a != b)
            {
                graph2.Edges[a, b] = true;
                graph2.Edges[b, a] = true;
                graph2.Weights[a, b] = 1.0;
                graph2.Weights[b, a] = 1.0;
            }
        }
        graph2.BuildSoAViews();
        
        double violation2 = graph2.CalculateTotalConstraintViolation();
        Assert.IsTrue(violation2 >= 0.0, "Random graph violation should be non-negative");
    }
    
    [TestMethod]
    [TestCategory("WheelerDeWitt")]
    public void WheelerDeWitt_LocalConstraintContribution_Normalized()
    {
        var config = new SimulationConfig { NodeCount = 20, Seed = TestSeed };
        var engine = new SimulationEngine(config);
        var graph = engine.Graph;
        
        CreateWheelerDeWittTriangleGrid(graph, 4);
        
        // Sum of local contributions should equal total weighted violation
        double totalContribution = 0.0;
        for (int i = 0; i < graph.N; i++)
        {
            totalContribution += graph.ComputeLocalConstraintContribution(i);
        }
        
        double expectedTotal = PhysicsConstants.WheelerDeWittConstants.ConstraintLagrangeMultiplier 
            * graph.CalculateTotalConstraintViolation();
        
        Assert.AreEqual(expectedTotal, totalContribution, 1e-10, 
            "Sum of local contributions should equal total weighted violation");
    }
    
    // ============================================================
    // HELPER METHODS FOR WHEELER-DEWITT TESTS
    // ============================================================
    
    private void CreateWheelerDeWittTriangleGrid(RQGraph graph, int size)
    {
        ClearAllEdges(graph);
        
        // Create a simple grid with triangular connections
        for (int i = 0; i < Math.Min(size * size, graph.N); i++)
        {
            int row = i / size;
            int col = i % size;
            
            // Connect to right neighbor
            if (col < size - 1 && i + 1 < graph.N)
            {
                graph.Edges[i, i + 1] = true;
                graph.Edges[i + 1, i] = true;
                graph.Weights[i, i + 1] = 1.0;
                graph.Weights[i + 1, i] = 1.0;
            }
            
            // Connect to bottom neighbor
            if (row < size - 1 && i + size < graph.N)
            {
                graph.Edges[i, i + size] = true;
                graph.Edges[i + size, i] = true;
                graph.Weights[i, i + size] = 1.0;
                graph.Weights[i + size, i] = 1.0;
            }
            
            // Diagonal for triangulation (alternating)
            if (col < size - 1 && row < size - 1 && i + size + 1 < graph.N)
            {
                if ((row + col) % 2 == 0)
                {
                    graph.Edges[i, i + size + 1] = true;
                    graph.Edges[i + size + 1, i] = true;
                    graph.Weights[i, i + size + 1] = 1.0;
                    graph.Weights[i + size + 1, i] = 1.0;
                }
            }
        }
        
        graph.BuildSoAViews();
    }
}

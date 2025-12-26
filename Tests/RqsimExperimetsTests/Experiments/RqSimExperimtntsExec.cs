using Microsoft.VisualStudio.TestTools.UnitTesting;
using RQSimulation;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using RqSimGraphEngine.Experiments;

namespace RqsimExperimetsTests.Experiments
{
    [TestClass]
    public class RqSimExperimtntsExec
    {
        private const string OutputDirectory = "ExperimentResults";

        [TestMethod]
        public void RunAllExperimentsAndSaveResults()
        {
            // Ensure output directory exists
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OutputDirectory);
            Directory.CreateDirectory(outputPath);

            var results = new List<ExperimentRunResult>();
            var experiments = ExperimentFactory.AvailableExperiments.ToList();

            Console.WriteLine($"Found {experiments.Count} experiments to run.");

            foreach (var experiment in experiments)
            {
                Console.WriteLine($"Running experiment: {experiment.Name}");
                try
                {
                    var result = RunSingleExperiment(experiment);
                    results.Add(result);
                    Console.WriteLine($"Experiment {experiment.Name} completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Experiment {experiment.Name} failed: {ex.Message}");
                    results.Add(new ExperimentRunResult
                    {
                        ExperimentName = experiment.Name,
                        Status = "Failed",
                        ErrorMessage = ex.Message,
                        Timestamp = DateTime.Now
                    });
                }
            }

            // Save results to JSON
            string fileName = $"ExperimentResults_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string fullPath = Path.Combine(outputPath, fileName);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(results, options);

            File.WriteAllText(fullPath, jsonString);
            Console.WriteLine($"Results saved to {fullPath}");
        }

        private ExperimentRunResult RunSingleExperiment(IExperiment experiment)
        {
            // 1. Get Configuration
            var startupConfig = experiment.GetConfig();
            var simConfig = startupConfig.ToSimulationConfig();

            // Override for testing speed
            simConfig.TotalSteps = 10; // Run only 10 steps for verification
            simConfig.VisualizationInterval = 100; // Disable visualization updates
            simConfig.LogEvery = 100;

            experiment.ApplyPhysicsOverrides(); // Apply static overrides if any (modifies PhysicsConstants)

            var engine = new SimulationEngine(simConfig);
            var graph = engine.Graph;

            for (int step = 0; step < simConfig.TotalSteps; step++)
            {
                // 1. Update Physics
                if (simConfig.UseUnifiedPhysicsStep)
                {
                    graph.UnifiedPhysicsStep(true);
                }
                else
                {
                    graph.UnifiedPhysicsStep(true);
                }

                // 2. Update Topology (Quantum Graphity)
                if (simConfig.UseQuantumGraphity)
                {
                    graph.QuantumGraphityStep();
                }
            }

            // Calculate metrics manually since helper methods might be missing
            int edgeCount = 0;
            int n = graph.N;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (graph.Edges[i, j])
                    {
                        edgeCount++;
                    }
                }
            }

            double avgDegree = n > 0 ? (2.0 * edgeCount) / n : 0.0;

            // 4. Collect Metrics
            return new ExperimentRunResult
            {
                ExperimentName = experiment.Name,
                Status = "Success",
                Timestamp = DateTime.Now,
                NodeCount = graph.N,
                EdgeCount = edgeCount,
                AverageDegree = avgDegree,
                SpectralDimension = graph.SmoothedSpectralDimension,
                Config = startupConfig
            };
        }
    }

    public class ExperimentRunResult
    {
        public string ExperimentName { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public int NodeCount { get; set; }
        public int EdgeCount { get; set; }
        public double AverageDegree { get; set; }
        public double SpectralDimension { get; set; }
        public StartupConfig Config { get; set; }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RQSimulation;
using System;

namespace RqSimGPUCPUTests.Tests.MCMC
{
    [TestClass]
    public class MCMCSampler_Tests
    {
        [TestMethod]
        public void CalculateEuclideanAction_ReturnsFiniteValue()
        {
            var graph = new RQGraph(50, 0.2, 0.1, 3, 1.0, 1.0, 0.1, 0.1, 42);
            var sampler = new MCMCSampler(graph);
            
            double action = sampler.CalculateEuclideanAction();
            
            Assert.IsFalse(double.IsNaN(action));
            Assert.IsFalse(double.IsInfinity(action));
        }

        [TestMethod]
        public void ProposeMove_ReturnsValidDeltaAndActions()
        {
            var graph = new RQGraph(50, 0.2, 0.1, 3, 1.0, 1.0, 0.1, 0.1, 42);
            var sampler = new MCMCSampler(graph);
            
            var (delta, apply, revert) = sampler.ProposeMove();
            
            Assert.IsFalse(double.IsNaN(delta));
            Assert.IsNotNull(apply);
            Assert.IsNotNull(revert);
            
            // Test applying
            double actionBefore = sampler.CalculateEuclideanAction();
            apply();
            double actionAfter = sampler.CalculateEuclideanAction();
            
            // Delta should match approximately (if calculation is exact)
            // Note: In my implementation, I calculated delta by full recompute, so it should match exactly.
            Assert.AreEqual(delta, actionAfter - actionBefore, 1e-9);
            
            // Test reverting
            revert();
            double actionReverted = sampler.CalculateEuclideanAction();
            Assert.AreEqual(actionBefore, actionReverted, 1e-9);
        }

        [TestMethod]
        public void SampleConfigurationSpace_UpdatesStatistics()
        {
            var graph = new RQGraph(20, 0.5, 0.1, 3, 1.0, 1.0, 0.1, 0.1, 42);
            var sampler = new MCMCSampler(graph);
            
            sampler.SampleConfigurationSpace(100);
            
            Assert.IsTrue(sampler.AcceptedMoves + sampler.RejectedMoves == 100);
            Assert.IsTrue(sampler.AcceptanceRate >= 0.0 && sampler.AcceptanceRate <= 1.0);
        }
    }
}

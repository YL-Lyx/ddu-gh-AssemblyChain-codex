using System.Collections.Generic;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Toolkit.Processing;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit.Processing
{
    public class PartCreationProcessorTests
    {
        [Fact]
        public void FromMeshes_CreatesPartGeometries()
        {
            var mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(0, 1), new Interval(0, 1), new Interval(0, 1)), 1, 1, 1);
            var options = new PartCreationProcessor.PartCreationOptions { BaseName = "Part" };

            var result = PartCreationProcessor.FromMeshes(new[] { mesh }, options);

            Assert.Single(result.CreatedItems);
            Assert.Equal(1, result.SuccessCount);
            Assert.Empty(result.Messages.FindAll(m => m.Level == ProcessingMessageLevel.Error));
        }

        [Fact]
        public void FromMeshes_InvalidMesh_RegistersWarning()
        {
            Mesh invalidMesh = null;
            var options = new PartCreationProcessor.PartCreationOptions();

            var result = PartCreationProcessor.FromMeshes(new[] { invalidMesh }, options);

            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Warning);
        }

        [Fact]
        public void FromBreps_ReturnsPhysicsEnabledParts()
        {
            var brep = Brep.CreateFromBox(new Box(Plane.WorldXY, new Interval(0, 1), new Interval(0, 1), new Interval(0, 1)));
            var physics = new PhysicsProperties(1, 0.6, 0.2, 0.1, 0.05);
            var options = new PartCreationProcessor.PartCreationOptions
            {
                BaseName = "Part",
                IncludePhysics = true,
                Physics = physics
            };

            var result = PartCreationProcessor.FromBreps(new[] { brep }, options);

            Assert.Single(result.CreatedItems);
            Assert.Equal(1, result.SuccessCount);
            Assert.Contains(result.Messages, m => m.Text.Contains("with physics"));
        }
    }
}

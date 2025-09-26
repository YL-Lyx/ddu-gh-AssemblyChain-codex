using System.Collections.Generic;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Toolkit.Processing;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Core.Tests.Toolkit.Processing
{
    public class AssemblyBuilderTests
    {
        [Fact]
        public void Build_WithValidParts_CreatesAssembly()
        {
            var parts = new List<Part?> { CreatePart(0, "P0"), CreatePart(1, "P1") };

            var result = AssemblyBuilder.Build("Demo", parts);

            Assert.True(result.HasAssembly);
            Assert.Equal(2, result.SuccessCount);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Remark);
        }

        [Fact]
        public void Build_WithInvalidParts_ReturnsWarningAndError()
        {
            var parts = new List<Part?> { null };

            var result = AssemblyBuilder.Build("Demo", parts);

            Assert.False(result.HasAssembly);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Warning);
            Assert.Contains(result.Messages, m => m.Level == ProcessingMessageLevel.Error);
        }

        private static Part CreatePart(int id, string name)
        {
            var mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(0, 1), new Interval(0, 1), new Interval(0, 1)), 1, 1, 1);
            var geometry = new PartGeometry(id, mesh, name, mesh, "Mesh");
            return new Part(id, name, geometry);
        }
    }
}

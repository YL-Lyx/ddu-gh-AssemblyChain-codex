using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Xunit;

namespace AssemblyChain.Gh.Kernel.Tests
{
    public class AcGhCreatePartTests
    {
        [Fact]
        public void Build_WithValidMeshInput_ReturnsGeometry()
        {
            var component = new AcGhCreatePart();
            var mesh = CreateTriangleMesh();
            var ghMesh = new GH_Mesh(mesh);
            var dataAccess = new FakePartDataAccess("TestPart", new[] { ghMesh }, Array.Empty<GH_Brep>(), null);

            var options = component.ParseInput(dataAccess);
            Assert.True(component.Validate(options));

            var result = component.Build(options);
            component.Output(dataAccess, result);

            Assert.Equal(1, result.SuccessCount);
            Assert.Single(result.Results);
            Assert.IsType<PartGeometry>(result.Results[0]);
            Assert.Single(dataAccess.Output);
            Assert.IsType<PartGeometry>(dataAccess.Output.Single().Value);
        }

        [Fact]
        public void Validate_WithMissingMeshInput_ReturnsFalse()
        {
            var component = new AcGhCreatePart();
            var dataAccess = new FakePartDataAccess("Empty", Array.Empty<GH_Mesh>(), Array.Empty<GH_Brep>(), null);

            var options = component.ParseInput(dataAccess);
            var isValid = component.Validate(options);

            Assert.False(isValid);
        }

        [Fact]
        public void Build_WithBrepAndPhysics_ReturnsPart()
        {
            var component = new AcGhCreatePart();
            SetPrivateField(component, "_includePhysics", true);
            SetPrivateField(component, "_inputMode", GetGeometryInputModeValue("Brep"));

            var brep = Brep.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(1, 1, 1)));
            var ghBrep = new GH_Brep(brep);
            var physics = new PhysicsProperties(2.5, 0.3, 0.1, 0.05, 0.05);
            var dataAccess = new FakePartDataAccess("PhysicsPart", Array.Empty<GH_Mesh>(), new[] { ghBrep }, physics);

            var options = component.ParseInput(dataAccess);
            Assert.True(component.Validate(options));

            var result = component.Build(options);
            component.Output(dataAccess, result);

            Assert.Equal(1, result.SuccessCount);
            Assert.True(result.HasPhysics);
            Assert.Single(result.Results);
            Assert.IsType<Part>(result.Results[0]);
            Assert.Single(dataAccess.Output);
            Assert.True(dataAccess.Output.Single().HasPhysics);
        }

        private static Mesh CreateTriangleMesh()
        {
            var mesh = new Mesh();
            mesh.Vertices.Add(0, 0, 0);
            mesh.Vertices.Add(1, 0, 0);
            mesh.Vertices.Add(0, 1, 0);
            mesh.Faces.AddFace(0, 1, 2);
            mesh.Normals.ComputeNormals();
            return mesh;
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static object GetGeometryInputModeValue(string name)
        {
            var enumType = typeof(AcGhCreatePart).GetNestedType("GeometryInputMode", BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("GeometryInputMode enum not found.");
            return Enum.Parse(enumType, name);
        }

        private sealed class FakePartDataAccess : AcGhCreatePart.IPartDataAccess
        {
            private readonly string _name;
            private readonly IReadOnlyList<GH_Mesh> _meshes;
            private readonly IReadOnlyList<GH_Brep> _breps;
            private readonly PhysicsProperties? _physics;

            public FakePartDataAccess(string name, IEnumerable<GH_Mesh> meshes, IEnumerable<GH_Brep> breps, PhysicsProperties? physics)
            {
                _name = name;
                _meshes = (meshes ?? Array.Empty<GH_Mesh>()).ToList();
                _breps = (breps ?? Array.Empty<GH_Brep>()).ToList();
                _physics = physics;
            }

            public List<AcGhPartWrapGoo> Output { get; } = new();

            public string GetName() => _name;

            public IReadOnlyList<GH_Mesh> GetMeshes() => _meshes;

            public IReadOnlyList<GH_Brep> GetBreps() => _breps;

            public PhysicsProperties? GetPhysics() => _physics;

            public void SetOutput(IEnumerable<AcGhPartWrapGoo> values)
            {
                Output.Clear();
                Output.AddRange(values);
            }
        }
    }
}

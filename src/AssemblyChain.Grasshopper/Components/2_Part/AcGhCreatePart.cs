using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Creates Part objects with flexible geometry input and optional physics properties
    /// </summary>
    public class AcGhCreatePart : GH_Component, IGH_VariableParameterComponent
    {
        private enum GeometryInputMode { Mesh, Brep }

        private GeometryInputMode _inputMode = GeometryInputMode.Mesh;
        private bool _includePhysics;

        public AcGhCreatePart()
            : base("Create Part", "CP", "Create Part objects with flexible geometry input and optional physics properties.", "AssemblyChain", "2|Part")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Optional name for the part.", GH_ParamAccess.item, string.Empty);
            // Default input is Mesh list (will switch to Brep via menu)
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh geometry that defines the part.", GH_ParamAccess.list);

            // Add physics properties input if enabled
            if (_includePhysics)
            {
                pManager.AddParameter(new AcGhPhysicalPropertyParam(), "Physical Property", "Physical", "Optional physics properties for the part.", GH_ParamAccess.item);
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhPartWrapParam(), "Part", "Part", "AssemblyChain part (geometry with optional physics)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                if (!ValidateInputConnections())
                {
                    return;
                }

                var adapter = new GhDataAccessAdapter(DA, _includePhysics);
                var options = ParseInput(adapter);

                if (!Validate(options))
                {
                    return;
                }

                var result = Build(options);
                Output(adapter, result);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
            }
        }

        internal PartOptions ParseInput(IPartDataAccess dataAccess)
        {
            ArgumentNullException.ThrowIfNull(dataAccess);

            var name = (dataAccess.GetName() ?? string.Empty).Trim();

            var physics = _includePhysics ? dataAccess.GetPhysics() : null;

            IReadOnlyList<GH_Mesh> meshes = Array.Empty<GH_Mesh>();
            IReadOnlyList<GH_Brep> breps = Array.Empty<GH_Brep>();

            if (_inputMode == GeometryInputMode.Mesh)
            {
                meshes = (dataAccess.GetMeshes() ?? Array.Empty<GH_Mesh>())
                    .Where(mesh => mesh != null)
                    .ToList();
            }
            else
            {
                breps = (dataAccess.GetBreps() ?? Array.Empty<GH_Brep>())
                    .Where(brep => brep != null)
                    .ToList();
            }

            return new PartOptions(name, _inputMode, _includePhysics, physics, meshes, breps);
        }

        internal bool Validate(PartOptions options)
        {
            if (options == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid part options.");
                return false;
            }

            if (_inputMode == GeometryInputMode.Mesh && options.Meshes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No mesh geometry provided.");
                return false;
            }

            if (_inputMode == GeometryInputMode.Brep && options.Breps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No brep geometry provided.");
                return false;
            }

            return true;
        }

        internal PartBuildResult Build(PartOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.InputMode == GeometryInputMode.Mesh
                ? BuildFromMeshes(options)
                : BuildFromBreps(options);
        }

        internal void Output(IPartDataAccess dataAccess, PartBuildResult result)
        {
            ArgumentNullException.ThrowIfNull(dataAccess);
            ArgumentNullException.ThrowIfNull(result);

            var goos = result.Results.Select(part => new AcGhPartWrapGoo(part)).ToList();
            dataAccess.SetOutput(goos);

            var report = $"Created {result.SuccessCount} Part objects";
            if (result.FailureCount > 0)
            {
                report += $", {result.FailureCount} failed";
            }

            if (result.HasPhysics)
            {
                report += " (with physics)";
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);
        }

        private PartBuildResult BuildFromMeshes(PartOptions options)
        {
            var results = new List<object>();
            int successCount = 0, failureCount = 0;

            for (int i = 0; i < options.Meshes.Count; i++)
            {
                try
                {
                    var mesh = options.Meshes[i]?.Value?.DuplicateMesh();
                    if (mesh == null || !mesh.IsValid)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid mesh at index {i}, skipping.");
                        continue;
                    }

                    mesh.Normals.ComputeNormals();

                    var partName = CreatePartName(options.Name, i);
                    var geometry = new PartGeometry(i, mesh, partName, options.Meshes[i]?.Value, "Mesh");
                    var partData = CreatePartData(options, geometry);
                    ReportCreation(options, partName, mesh, partData);

                    results.Add(partData);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error at index {i}: {ex.Message}");
                }
            }

            return new PartBuildResult(results, successCount, failureCount, options.HasPhysics);
        }

        private PartBuildResult BuildFromBreps(PartOptions options)
        {
            var results = new List<object>();
            int successCount = 0, failureCount = 0;

            for (int i = 0; i < options.Breps.Count; i++)
            {
                try
                {
                    var brep = options.Breps[i]?.Value;
                    if (brep == null || !brep.IsValid)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid brep at index {i}, skipping.");
                        continue;
                    }

                    var brepMeshes = Mesh.CreateFromBrep(brep, MeshingParameters.Default);
                    if (brepMeshes == null || brepMeshes.Length == 0)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Meshing failed at index {i}, skipping.");
                        continue;
                    }

                    var mesh = new Mesh();
                    foreach (var m in brepMeshes)
                    {
                        if (m != null)
                        {
                            mesh.Append(m);
                        }
                    }

                    if (!mesh.IsValid)
                    {
                        failureCount++;
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Generated mesh invalid at index {i}, skipping.");
                        continue;
                    }

                    mesh.Normals.ComputeNormals();

                    var partName = CreatePartName(options.Name, i);
                    var geometry = new PartGeometry(i, mesh, partName, brep, "Brep");
                    var partData = CreatePartData(options, geometry);
                    ReportCreation(options, partName, mesh, partData);

                    results.Add(partData);
                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error at index {i}: {ex.Message}");
                }
            }

            return new PartBuildResult(results, successCount, failureCount, options.HasPhysics);
        }

        private static string CreatePartName(string baseName, int index)
        {
            return !string.IsNullOrEmpty(baseName) ? $"{baseName}_{index}" : $"Part_{index}";
        }

        private object CreatePartData(PartOptions options, PartGeometry geometry)
        {
            if (options.HasPhysics && options.Physics != null)
            {
                return new Part(geometry.IndexId, geometry.Name, geometry, options.Physics);
            }

            return geometry;
        }

        private void ReportCreation(PartOptions options, string partName, Mesh mesh, object partData)
        {
            if (options.HasPhysics && options.Physics != null && partData is Part)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Created Part '{partName}' with physics: mass={options.Physics.Mass:F3}kg, friction={options.Physics.Friction:F2}");
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    $"Created Part '{partName}' with {mesh.Vertices.Count} vertices, {mesh.Faces.Count} faces");
            }
        }

        public override Guid ComponentGuid => new Guid("c9d0e1f2-a3b4-5678-9abc-def012345678");

        // IGH_VariableParameterComponent implementation
        public bool CanInsertParameter(GH_ParameterSide side, int index) => false;
        public bool CanRemoveParameter(GH_ParameterSide side, int index) => false;
        public IGH_Param CreateParameter(GH_ParameterSide side, int index) => null;
        public bool DestroyParameter(GH_ParameterSide side, int index) => false;

        public void VariableParameterMaintenance()
        {
            if (Params.Input.Count < 2) return;

            // Handle geometry parameter (index 1)
            var geometryParam = Params.Input[1];
            if (_inputMode == GeometryInputMode.Mesh)
            {
                geometryParam.Name = "Mesh";
                geometryParam.NickName = "Mesh";
                geometryParam.Description = "Mesh geometry that defines the part.";
            }
            else
            {
                geometryParam.Name = "Brep";
                geometryParam.NickName = "Brep";
                geometryParam.Description = "Brep geometry that defines the part.";
            }

            // Handle physics parameter (index 2, if present)
            if (Params.Input.Count >= 3)
            {
                var physicsParam = Params.Input[2];
                if (_includePhysics)
                {
                    physicsParam.Name = "Physical Property";
                    physicsParam.NickName = "Physical Property";
                    physicsParam.Description = "Physics properties for the part.";
                }
                else
                {
                    physicsParam.Name = "Physical Property (disabled)";
                    physicsParam.NickName = "Physical Property";
                    physicsParam.Description = "Physics properties for the part (currently disabled).";
                }
            }
        }

        private void UpdateInputParameters()
        {
            // Handle geometry parameter type change (index 1)
            if (Params.Input.Count >= 2)
            {
                var currentGeometryParam = Params.Input[1];
                bool needsGeometryChange = false;

                if (_inputMode == GeometryInputMode.Mesh && !(currentGeometryParam is Grasshopper.Kernel.Parameters.Param_Mesh))
                {
                    needsGeometryChange = true;
                }
                else if (_inputMode == GeometryInputMode.Brep && !(currentGeometryParam is Grasshopper.Kernel.Parameters.Param_Brep))
                {
                    needsGeometryChange = true;
                }

                if (needsGeometryChange)
                {
                    // Remove current geometry parameter
                    Params.UnregisterInputParameter(Params.Input[1]);

                    // Add new geometry parameter
                    if (_inputMode == GeometryInputMode.Mesh)
                    {
                        Params.RegisterInputParam(new Grasshopper.Kernel.Parameters.Param_Mesh
                        {
                            Name = "Mesh",
                            NickName = "Mesh",
                            Description = "Mesh geometry that defines the part.",
                            Access = GH_ParamAccess.list
                        }, 1);
                    }
                    else
                    {
                        Params.RegisterInputParam(new Grasshopper.Kernel.Parameters.Param_Brep
                        {
                            Name = "Brep",
                            NickName = "Brep",
                            Description = "Brep geometry that defines the part.",
                            Access = GH_ParamAccess.list
                        }, 1);
                    }
                }
            }

            // Handle physics parameter (index 2) - add if needed, remove when disabled
            if (_includePhysics)
            {
                if (Params.Input.Count < 3)
                {
                    // Add physics parameter if it doesn't exist
                    Params.RegisterInputParam(new AcGhPhysicalPropertyParam
                    {
                        Name = "Physical Property",
                        NickName = "Physical Property",
                        Description = "Physics properties for the part.",
                        Access = GH_ParamAccess.item
                    }, 2);
                }
            }
            else
            {
                // Remove physics parameter when disabled to disconnect and hide it
                if (Params.Input.Count >= 3)
                {
                    Params.UnregisterInputParameter(Params.Input[2]);
                }
            }

            VariableParameterMaintenance();
            Params.OnParametersChanged();
        }

        private void UpdatePhysicsMode(bool includePhysics)
        {
            _includePhysics = includePhysics;
            UpdateInputParameters();
            // Output parameters remain constant, no need to update
            ExpireSolution(true);
        }

        private void UpdateOutputParameters()
        {
            // Output parameter should remain constant - it's always AcGhPartWrapParam
            // No need to modify output parameters as they don't change based on physics settings
            // The output type remains the same regardless of whether physics are included
        }

        /// <summary>
        /// 验证输入连接类型，防止Grasshopper的自动类型转换
        /// </summary>
        private bool ValidateInputConnections()
        {
            // 获取几何输入参数（索引1）
            if (Params.Input.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing geometry input parameter.");
                return false;
            }

            var geometryParam = Params.Input[1];

            // 检查所有连接的源
            foreach (var source in geometryParam.Sources)
            {
                if (!IsCompatibleSource(source))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        $"Invalid connection: Component is in {_inputMode} mode. " +
                        $"Only direct {_inputMode} outputs are allowed. " +
                        $"Connected source: {source.Name} ({source.GetType().Name})");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查源参数是否与当前输入模式兼容
        /// </summary>
        private bool IsCompatibleSource(IGH_Param source)
        {
            return _inputMode switch
            {
                GeometryInputMode.Mesh => source is Grasshopper.Kernel.Parameters.Param_Mesh ||
                                          source.Type == typeof(GH_Mesh),
                GeometryInputMode.Brep => source is Grasshopper.Kernel.Parameters.Param_Brep ||
                                          source.Type == typeof(GH_Brep),
                _ => false
            };
        }

        // Right-click menu
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            menu.Items.Add(new ToolStripSeparator());

            // Geometry input options
            var geometryMenu = new ToolStripMenuItem("Geometry Input");
            var meshItem = new ToolStripMenuItem("Mesh", null, (s, e) =>
            {
                _inputMode = GeometryInputMode.Mesh;
                UpdateInputParameters();
                ExpireSolution(true);
            })
            { Checked = _inputMode == GeometryInputMode.Mesh };
            geometryMenu.DropDownItems.Add(meshItem);

            var brepItem = new ToolStripMenuItem("Brep", null, (s, e) =>
            {
                _inputMode = GeometryInputMode.Brep;
                UpdateInputParameters();
                ExpireSolution(true);
            })
            { Checked = _inputMode == GeometryInputMode.Brep };
            geometryMenu.DropDownItems.Add(brepItem);
            menu.Items.Add(geometryMenu);

            // Physics properties option
            var physicsItem = new ToolStripMenuItem("Include Physical Properties", null, (s, e) =>
            {
                UpdatePhysicsMode(!_includePhysics);
            })
            { Checked = _includePhysics };
            menu.Items.Add(physicsItem);
        }

        // Serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("InputMode", (int)_inputMode);
            writer.SetBoolean("IncludePhysics", _includePhysics);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            int modeInt = (int)GeometryInputMode.Mesh;
            if (reader.TryGetInt32("InputMode", ref modeInt))
            {
                _inputMode = (GeometryInputMode)modeInt;
            }

            bool includePhysics = false;
            if (reader.TryGetBoolean("IncludePhysics", ref includePhysics))
            {
                _includePhysics = includePhysics;
            }

            // Update parameters after deserialization
            UpdateInputParameters();
            UpdateOutputParameters();

            return base.Read(reader);
        }

        internal interface IPartDataAccess
        {
            string GetName();
            IReadOnlyList<GH_Mesh> GetMeshes();
            IReadOnlyList<GH_Brep> GetBreps();
            PhysicsProperties? GetPhysics();
            void SetOutput(IEnumerable<AcGhPartWrapGoo> values);
        }

        internal sealed record PartOptions(
            string Name,
            GeometryInputMode InputMode,
            bool IncludePhysics,
            PhysicsProperties? Physics,
            IReadOnlyList<GH_Mesh> Meshes,
            IReadOnlyList<GH_Brep> Breps)
        {
            public bool HasPhysics => IncludePhysics && Physics != null;
        }

        internal sealed record PartBuildResult(
            IReadOnlyList<object> Results,
            int SuccessCount,
            int FailureCount,
            bool HasPhysics);

        private sealed class GhDataAccessAdapter : IPartDataAccess
        {
            private readonly IGH_DataAccess _dataAccess;
            private readonly bool _includePhysics;

            public GhDataAccessAdapter(IGH_DataAccess dataAccess, bool includePhysics)
            {
                _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
                _includePhysics = includePhysics;
            }

            public string GetName()
            {
                string name = string.Empty;
                _dataAccess.GetData(0, ref name);
                return name ?? string.Empty;
            }

            public IReadOnlyList<GH_Mesh> GetMeshes()
            {
                var meshes = new List<GH_Mesh>();
                _dataAccess.GetDataList(1, meshes);
                return meshes;
            }

            public IReadOnlyList<GH_Brep> GetBreps()
            {
                var breps = new List<GH_Brep>();
                _dataAccess.GetDataList(1, breps);
                return breps;
            }

            public PhysicsProperties? GetPhysics()
            {
                if (!_includePhysics)
                {
                    return null;
                }

                var physicsGoo = new AcGhPhysicalPropertyGoo();
                if (_dataAccess.GetData(2, ref physicsGoo) && physicsGoo?.Value != null)
                {
                    return physicsGoo.Value;
                }

                return null;
            }

            public void SetOutput(IEnumerable<AcGhPartWrapGoo> values)
            {
                _dataAccess.SetDataList(0, values);
            }
        }
    }
}

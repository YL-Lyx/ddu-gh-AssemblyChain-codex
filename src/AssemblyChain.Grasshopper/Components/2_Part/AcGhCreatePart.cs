using System;
using System.Collections.Generic;
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
        private bool _includePhysics = false;

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
                // Validate input connections
                if (!ValidateInputConnections())
                {
                    return;
                }

                string name = string.Empty;
                DA.GetData(0, ref name);
                name = name?.Trim() ?? string.Empty;

                var results = new List<object>();
                int successCount = 0, failureCount = 0;

                // Get physics properties if enabled
                PhysicsProperties physicsProperties = null;
                if (_includePhysics)
                {
                    var physicsGoo = new AcGhPhysicalPropertyGoo();
                    if (DA.GetData(2, ref physicsGoo) && physicsGoo.Value != null)
                    {
                        physicsProperties = physicsGoo.Value;
                    }
                }

                if (_inputMode == GeometryInputMode.Mesh)
                {
                    var meshes = new List<GH_Mesh>();
                    if (!DA.GetDataList(1, meshes) || meshes.Count == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No mesh geometry provided.");
                        return;
                    }

                    for (int i = 0; i < meshes.Count; i++)
                    {
                        try
                        {
                            var m = meshes[i]?.Value?.DuplicateMesh();
                            if (m == null || !m.IsValid)
                            {
                                failureCount++;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid mesh at index {i}, skipping.");
                                continue;
                            }
                            m.Normals.ComputeNormals();

                            var partName = !string.IsNullOrEmpty(name) ? $"{name}_{i}" : $"Part_{i}";
                            var pg = new PartGeometry(i, m, partName, meshes[i]?.Value, "Mesh");

                            object partData;
                            if (_includePhysics && physicsProperties != null)
                            {
                                var part = new Part(i, partName, pg, physicsProperties);
                                partData = part;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                    $"Created Part '{partName}' with physics: mass={physicsProperties.Mass:F3}kg, friction={physicsProperties.Friction:F2}");
                            }
                            else
                            {
                                partData = pg; // Store PartGeometry directly
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                    $"Created Part '{partName}' with {m.Vertices.Count} vertices, {m.Faces.Count} faces");
                            }

                            results.Add(partData);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failureCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error at index {i}: {ex.Message}");
                        }
                    }
                }
                else // Brep
                {
                    var breps = new List<GH_Brep>();
                    int inputIndex = _includePhysics ? 1 : 1;
                    if (!DA.GetDataList(inputIndex, breps) || breps.Count == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No brep geometry provided.");
                        return;
                    }

                    for (int i = 0; i < breps.Count; i++)
                    {
                        try
                        {
                            var b = breps[i]?.Value;
                            if (b == null || !b.IsValid)
                            {
                                failureCount++;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Invalid brep at index {i}, skipping.");
                                continue;
                            }

                            var brepMeshes = Mesh.CreateFromBrep(b, MeshingParameters.Default);
                            if (brepMeshes == null || brepMeshes.Length == 0)
                            {
                                failureCount++;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Meshing failed at index {i}, skipping.");
                                continue;
                            }

                            var mesh = new Mesh();
                            foreach (var m in brepMeshes)
                            {
                                if (m != null) mesh.Append(m);
                            }
                            if (!mesh.IsValid)
                            {
                                failureCount++;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Generated mesh invalid at index {i}, skipping.");
                                continue;
                            }
                            mesh.Normals.ComputeNormals();

                            var partName = !string.IsNullOrEmpty(name) ? $"{name}_{i}" : $"Part_{i}";
                            var pg = new PartGeometry(i, mesh, partName, breps[i]?.Value, "Brep");

                            object partData;
                            if (_includePhysics && physicsProperties != null)
                            {
                                var part = new Part(i, partName, pg, physicsProperties);
                                partData = part;
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                    $"Created Part '{partName}' with physics: mass={physicsProperties.Mass:F3}kg, friction={physicsProperties.Friction:F2}");
                            }
                            else
                            {
                                partData = pg; // Store PartGeometry directly
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                    $"Created Part '{partName}' with {mesh.Vertices.Count} vertices, {mesh.Faces.Count} faces");
                            }

                            results.Add(partData);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failureCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error at index {i}: {ex.Message}");
                        }
                    }
                }

                var report = $"Created {successCount} Part objects";
                if (failureCount > 0) report += $", {failureCount} failed";
                if (_includePhysics && physicsProperties != null) report += " (with physics)";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);

                var goos = results.ConvertAll(p => new AcGhPartWrapGoo(p));
                DA.SetDataList(0, goos);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
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
    }
}





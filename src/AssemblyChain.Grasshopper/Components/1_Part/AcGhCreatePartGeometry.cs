using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Creates PartGeometry objects with flexible geometry input
    /// </summary>
    public class AcGhCreatePartGeometry : GH_Component, IGH_VariableParameterComponent
    {
        private enum GeometryInputMode { Mesh, Brep }

        private GeometryInputMode _inputMode = GeometryInputMode.Mesh;

        public AcGhCreatePartGeometry()
            : base("Create PartGeometry", "CPG", "Create PartGeometry objects with flexible geometry input.", "AssemblyChain", "1|Part")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Optional name for the part geometry.", GH_ParamAccess.item, string.Empty);
            // Default input is Mesh list (will switch to Brep via menu)
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh geometry that defines the part.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AcGhPartGeometryParam(), "Part", "PG", "AssemblyChain part geometry", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                // 首先验证输入连接类型，防止Grasshopper的自动类型转换
                if (!ValidateInputConnections())
                {
                    return;
                }

                string name = string.Empty;
                DA.GetData(0, ref name);
                name = name?.Trim() ?? string.Empty;

                var results = new List<PartGeometry>();
                int successCount = 0, failureCount = 0;

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

                            var partName = !string.IsNullOrEmpty(name) ? $"{name}_{i}" : $"PartGeometry_{i}";
                            var pg = new PartGeometry(i, m, partName, meshes[i]?.Value, "Mesh");
                            results.Add(pg);
                            successCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                $"Created PartGeometry '{partName}' with {m.Vertices.Count} vertices, {m.Faces.Count} faces");
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
                    if (!DA.GetDataList(1, breps) || breps.Count == 0)
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

                            var partName = !string.IsNullOrEmpty(name) ? $"{name}_{i}" : $"PartGeometry_{i}";
                            var pg = new PartGeometry(i, mesh, partName, breps[i]?.Value, "Brep");
                            results.Add(pg);
                            successCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                                $"Created PartGeometry '{partName}' with {mesh.Vertices.Count} vertices, {mesh.Faces.Count} faces");
                        }
                        catch (Exception ex)
                        {
                            failureCount++;
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error at index {i}: {ex.Message}");
                        }
                    }
                }

                var report = $"Created {successCount} PartGeometry objects";
                if (failureCount > 0) report += $", {failureCount} failed";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, report);

                var goos = results.ConvertAll(pg => new AcGhPartGeometryGoo(pg));
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
            var param = Params.Input[1];
            if (_inputMode == GeometryInputMode.Mesh)
            {
                param.Name = "Mesh";
                param.NickName = "Mesh";
                param.Description = "Mesh geometry that defines the part.";
            }
            else
            {
                param.Name = "Brep";
                param.NickName = "Brep";
                param.Description = "Brep geometry that defines the part.";
            }
        }

        private void UpdateInputParameters()
        {
            if (Params.Input.Count >= 2)
            {
                Params.UnregisterInputParameter(Params.Input[1]);
            }

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

            VariableParameterMaintenance();
            Params.OnParametersChanged();
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

            var meshItem = new ToolStripMenuItem("Input: Mesh", null, (s, e) =>
            {
                _inputMode = GeometryInputMode.Mesh;
                UpdateInputParameters();
                ExpireSolution(true);
            })
            { Checked = _inputMode == GeometryInputMode.Mesh };
            menu.Items.Add(meshItem);

            var brepItem = new ToolStripMenuItem("Input: Brep", null, (s, e) =>
            {
                _inputMode = GeometryInputMode.Brep;
                UpdateInputParameters();
                ExpireSolution(true);
            })
            { Checked = _inputMode == GeometryInputMode.Brep };
            menu.Items.Add(brepItem);
        }

        // Serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetInt32("InputMode", (int)_inputMode);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            int modeInt = (int)GeometryInputMode.Mesh;
            if (reader.TryGetInt32("InputMode", ref modeInt))
            {
                _inputMode = (GeometryInputMode)modeInt;
            }
            return base.Read(reader);
        }
    }
}





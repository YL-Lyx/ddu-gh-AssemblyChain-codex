using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using AssemblyChain.Core.Toolkit.Processing;
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
                if (!ValidateInputConnections())
                {
                    return;
                }

                var options = CreateOptions(DA);
                PartCreationProcessor.PartCreationResult result = _inputMode == GeometryInputMode.Mesh
                    ? ProcessMeshInput(DA, options)
                    : ProcessBrepInput(DA, options);

                EmitMessages(result.Messages);

                var goos = result.CreatedItems.ConvertAll(p => new AcGhPartWrapGoo(p));
                DA.SetDataList(0, goos);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
            }
        }

        private PartCreationProcessor.PartCreationOptions CreateOptions(IGH_DataAccess dataAccess)
        {
            string name = string.Empty;
            dataAccess.GetData(0, ref name);

            PhysicsProperties? physics = null;
            if (_includePhysics)
            {
                var physicsGoo = new AcGhPhysicalPropertyGoo();
                if (dataAccess.GetData(2, ref physicsGoo) && physicsGoo.Value != null)
                {
                    physics = physicsGoo.Value;
                }
            }

            return new PartCreationProcessor.PartCreationOptions
            {
                BaseName = name?.Trim() ?? string.Empty,
                IncludePhysics = _includePhysics,
                Physics = physics
            };
        }

        private PartCreationProcessor.PartCreationResult ProcessMeshInput(IGH_DataAccess dataAccess,
            PartCreationProcessor.PartCreationOptions options)
        {
            var meshes = new List<GH_Mesh>();
            dataAccess.GetDataList(1, meshes);
            var rhinoMeshes = meshes.ConvertAll(m => m?.Value);
            return PartCreationProcessor.FromMeshes(rhinoMeshes, options);
        }

        private PartCreationProcessor.PartCreationResult ProcessBrepInput(IGH_DataAccess dataAccess,
            PartCreationProcessor.PartCreationOptions options)
        {
            var breps = new List<GH_Brep>();
            dataAccess.GetDataList(1, breps);
            var rhinoBreps = breps.ConvertAll(b => b?.Value);
            return PartCreationProcessor.FromBreps(rhinoBreps, options);
        }

        private void EmitMessages(IEnumerable<ProcessingMessage> messages)
        {
            foreach (var message in messages)
            {
                AddRuntimeMessage(ToRuntimeLevel(message.Level), message.Text);
            }
        }

        private static GH_RuntimeMessageLevel ToRuntimeLevel(ProcessingMessageLevel level)
        {
            return level switch
            {
                ProcessingMessageLevel.Remark => GH_RuntimeMessageLevel.Remark,
                ProcessingMessageLevel.Warning => GH_RuntimeMessageLevel.Warning,
                ProcessingMessageLevel.Error => GH_RuntimeMessageLevel.Error,
                _ => GH_RuntimeMessageLevel.Remark
            };
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





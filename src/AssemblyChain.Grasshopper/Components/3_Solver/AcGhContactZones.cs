#nullable enable
using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using AssemblyChain.Core.Contact;
using AssemblyChain.Core.Toolkit.Processing;

namespace AssemblyChain.Gh.Kernel
{
    public class AcGhContactZones : GH_Component
    {
        public AcGhContactZones()
            : base("Contact Zones", "CZ", "Extract geometric zones and planes where parts contact from contact model", "AssemblyChain", "3|Solver")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AcGhContactModelParam(), "ContactModel", "CM", "Contact model containing contact data", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Zones", "Zones", "Contact geometries grouped per part (DataTree: {i} -> all contacts of part i)", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Planes", "Planes", "Contact planes grouped per part (DataTree: {i} -> all contacts of part i)", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                AcGhContactModelGoo contactModelGoo = null!;
                if (!DA.GetData(0, ref contactModelGoo) || contactModelGoo?.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid contact model input.");
                    return;
                }

                var extraction = ContactZoneExtractor.ExtractFaceContacts(contactModelGoo.Value.Contacts);
                EmitMessages(extraction.Messages);

                var (zonesTree, planesTree) = BuildOutputTrees(extraction);
                DA.SetDataTree(0, zonesTree);
                DA.SetDataTree(1, planesTree);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error extracting contact zones: {ex.Message}");
            }
        }

        private (GH_Structure<IGH_GeometricGoo> zones, GH_Structure<GH_Plane> planes) BuildOutputTrees(
            ContactZoneExtractor.ContactZoneExtractionResult extraction)
        {
            var zonesTree = new GH_Structure<IGH_GeometricGoo>();
            var planesTree = new GH_Structure<GH_Plane>();

            foreach (var partIndex in extraction.PartIndices)
            {
                var path = new GH_Path(partIndex);
                zonesTree.EnsurePath(path);
                planesTree.EnsurePath(path);

                if (extraction.PartGeometries.TryGetValue(partIndex, out var geometries))
                {
                    foreach (var geometry in geometries)
                    {
                        AppendGeometryToTree(geometry, path, zonesTree);
                        planesTree.Append(new GH_Plane(geometry.Plane), path);
                    }
                }
            }

            return (zonesTree, planesTree);
        }

        private static void AppendGeometryToTree(ContactZoneExtractor.ContactFaceGeometry geometry, GH_Path path,
            GH_Structure<IGH_GeometricGoo> zonesTree)
        {
            if (geometry.Brep != null)
            {
                zonesTree.Append(new GH_Brep(geometry.Brep), path);
            }
            else if (geometry.Mesh != null)
            {
                zonesTree.Append(new GH_Mesh(geometry.Mesh), path);
            }
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


        public override Guid ComponentGuid => new Guid("c7d8e9f0-a1b2-c3d4-e5f6-a7b8c9d0e1f2");
    }
}

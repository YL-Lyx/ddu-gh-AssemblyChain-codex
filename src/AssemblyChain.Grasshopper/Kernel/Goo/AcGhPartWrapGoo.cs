using System;
using AssemblyChain.Core.Domain.Entities;
using AssemblyChain.Core.Domain.ValueObjects;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Unified Grasshopper Goo wrapper for Parts - handles both geometry-only and physics-enabled parts
    /// </summary>
    public class AcGhPartWrapGoo : AcGhGooBase<object>
    {
        public AcGhPartWrapGoo()
        {
        }

        public AcGhPartWrapGoo(object value)
            : base(value)
        {
        }

        protected override AcGhGooBase<object> CreateInstance(object value)
        {
            return new AcGhPartWrapGoo(value);
        }

        protected override AcGhGooBase<object> CreateEmpty()
        {
            return new AcGhPartWrapGoo();
        }

        /// <summary>
        /// The geometry of the part (always available)
        /// </summary>
        public PartGeometry Geometry
        {
            get
            {
                if (Value is Part part)
                    return part.Geometry;
                else if (Value is PartGeometry geometry)
                    return geometry;
                return null;
            }
        }

        /// <summary>
        /// The physics properties (null if not available)
        /// </summary>
        public PhysicsProperties Physics
        {
            get
            {
                if (Value is Part part)
                    return part.Physics;
                return null;
            }
        }

        /// <summary>
        /// Whether this part has physics properties
        /// </summary>
        public bool HasPhysics => Physics != null;

        /// <summary>
        /// The complete part object (constructed on demand if physics available)
        /// </summary>
        public Part CompletePart
        {
            get
            {
                if (Value is Part part)
                    return part;
                else if (Value is PartGeometry geometry)
                    return new Part(geometry.IndexId, geometry.Name, geometry);
                return null;
            }
        }

        public override bool IsValid => Value != null && Geometry?.HasValidGeometry == true;

        public override string TypeName => "Part";

        public override string TypeDescription => HasPhysics
            ? "AssemblyChain part with geometry and physics"
            : "AssemblyChain part geometry";

        public override string ToString()
        {
            if (Value == null) return "Null Part";
            var name = Geometry?.Name ?? "Unknown";
            return HasPhysics ? $"{name} (with physics)" : $"{name} (geometry only)";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case Part part:
                    Value = part;
                    return true;
                case PartGeometry geometry:
                    Value = geometry;
                    return true;
                case AcGhPartWrapGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (Value != null)
            {
                // Can cast to Part if we have a complete part
                if (typeof(T).IsAssignableFrom(typeof(Part)) && CompletePart != null)
                {
                    target = (T)(object)CompletePart;
                    return true;
                }

                // Can cast to PartGeometry
                if (typeof(T).IsAssignableFrom(typeof(PartGeometry)) && Geometry != null)
                {
                    target = (T)(object)Geometry;
                    return true;
                }

                // Can cast to PhysicsProperties if available
                if (typeof(T).IsAssignableFrom(typeof(PhysicsProperties)) && Physics != null)
                {
                    target = (T)(object)Physics;
                    return true;
                }
            }

            return base.CastTo(ref target);
        }
    }
}

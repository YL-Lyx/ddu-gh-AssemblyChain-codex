using AssemblyChain.Core.Contact;
using Grasshopper.Kernel.Types;

namespace AssemblyChain.Gh.Kernel
{
    /// <summary>
    /// Grasshopper Goo wrapper for <see cref="DetectionOptions"/>.
    /// </summary>
    public class AcGhDetectionOptionsGoo : GH_Goo<DetectionOptions>
    {
        public AcGhDetectionOptionsGoo()
        {
        }

        public AcGhDetectionOptionsGoo(DetectionOptions value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return new AcGhDetectionOptionsGoo(Value);
        }

        public override bool IsValid => true;

        public override string TypeName => "DetectionOptions";

        public override string TypeDescription => "AssemblyChain contact detection options";

        public override string ToString()
        {
            var options = Value;
            return $"Tolerance: {options.Tolerance:G3}, MinArea: {options.MinPatchArea:G3}, BroadPhase: {options.BroadPhase}";
        }

        public override bool CastFrom(object source)
        {
            switch (source)
            {
                case DetectionOptions options:
                    Value = options;
                    return true;
                case AcGhDetectionOptionsGoo goo:
                    Value = goo.Value;
                    return true;
                default:
                    return base.CastFrom(source);
            }
        }

        public override bool CastTo<T>(ref T target)
        {
            if (typeof(T).IsAssignableFrom(typeof(DetectionOptions)))
            {
                target = (T)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }
    }
}

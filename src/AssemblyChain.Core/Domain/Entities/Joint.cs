using System;
using AssemblyChain.Core.Domain.Common;
using AssemblyChain.Core.Domain.Entities;
using Rhino.Geometry;

namespace AssemblyChain.Core.Domain.Entities
{
    /// <summary>
    /// Domain entity representing a joint/connection between parts
    /// </summary>
    public class Joint : Entity
    {
        /// <summary>
        /// First part in the joint
        /// </summary>
        public Part PartA { get; }

        /// <summary>
        /// Second part in the joint
        /// </summary>
        public Part PartB { get; }

        /// <summary>
        /// Type of joint
        /// </summary>
        public JointType Type { get; }

        /// <summary>
        /// Joint position/frame in world coordinates
        /// </summary>
        public Plane Frame { get; }

        /// <summary>
        /// Joint limits (for revolute/prismatic joints)
        /// </summary>
        public JointLimits Limits { get; }

        /// <summary>
        /// Whether the joint is currently active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Creates a new Joint
        /// </summary>
        public Joint(int id, Part partA, Part partB, JointType type,
            Plane frame, JointLimits limits = null)
            : base(id)
        {
            PartA = partA ?? throw new ArgumentNullException(nameof(partA));
            PartB = partB ?? throw new ArgumentNullException(nameof(partB));

            if (partA.Id == partB.Id)
                throw new ArgumentException("Cannot create joint between same part");

            Type = type;
            Frame = frame;
            Limits = limits ?? JointLimits.Unlimited;
            IsActive = true;
        }

        /// <summary>
        /// Activates the joint
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the joint
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Checks if this joint involves a specific part
        /// </summary>
        public bool InvolvesPart(int partId)
        {
            return PartA.Id == partId || PartB.Id == partId;
        }

        /// <summary>
        /// Gets the other part in the joint
        /// </summary>
        public Part GetOtherPart(int partId)
        {
            if (PartA.Id == partId) return PartB;
            if (PartB.Id == partId) return PartA;
            throw new ArgumentException("Part is not part of this joint", nameof(partId));
        }
    }

    /// <summary>
    /// Types of mechanical joints
    /// </summary>
    public enum JointType
    {
        Fixed,      // No relative motion
        Revolute,   // Rotational motion around axis
        Prismatic,  // Linear motion along axis
        Spherical,  // 3DOF rotational motion
        Planar      // 3DOF motion in plane
    }

    /// <summary>
    /// Joint motion limits
    /// </summary>
    public class JointLimits
    {
        /// <summary>
        /// Minimum limit
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Maximum limit
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Whether the joint has limits
        /// </summary>
        public bool HasLimits => Min < Max;

        public JointLimits(double min, double max)
        {
            if (min >= max)
                throw new ArgumentException("Min must be less than Max");

            Min = min;
            Max = max;
        }

        public static JointLimits Unlimited =>
            new JointLimits(double.NegativeInfinity, double.PositiveInfinity);
    }
}





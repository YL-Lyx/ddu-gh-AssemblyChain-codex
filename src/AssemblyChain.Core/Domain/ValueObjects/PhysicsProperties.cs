using System;
using System.Collections.Generic;
using AssemblyChain.Core.Domain.Common;

namespace AssemblyChain.Core.Domain.ValueObjects
{
    /// <summary>
    /// Physics properties value object for parts
    /// </summary>
    public class PhysicsProperties : ValueObject
    {
        /// <summary>
        /// Mass of the part in kg
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// Friction coefficient (0-1)
        /// </summary>
        public double Friction { get; }

        /// <summary>
        /// Restitution coefficient (0-1)
        /// </summary>
        public double Restitution { get; }

        /// <summary>
        /// Rolling friction coefficient (0-1)
        /// </summary>
        public double RollingFriction { get; }

        /// <summary>
        /// Spinning friction coefficient (0-1)
        /// </summary>
        public double SpinningFriction { get; }

        public PhysicsProperties(double mass, double friction, double restitution,
            double rollingFriction, double spinningFriction)
        {
            Mass = Math.Max(0.001, mass); // Ensure positive mass
            Friction = Math.Clamp(friction, 0.0, 1.0);
            Restitution = Math.Clamp(restitution, 0.0, 1.0);
            RollingFriction = Math.Clamp(rollingFriction, 0.0, 1.0);
            SpinningFriction = Math.Clamp(spinningFriction, 0.0, 1.0);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Mass;
            yield return Friction;
            yield return Restitution;
            yield return RollingFriction;
            yield return SpinningFriction;
        }

        public static PhysicsProperties Default =>
            new PhysicsProperties(1.0, 0.5, 0.1, 0.01, 0.01);
    }
}



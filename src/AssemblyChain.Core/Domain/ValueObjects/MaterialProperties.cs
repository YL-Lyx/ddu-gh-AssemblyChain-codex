using System;
using System.Collections.Generic;
using AssemblyChain.Core.Domain.Common;

namespace AssemblyChain.Core.Domain.ValueObjects
{
    /// <summary>
    /// Material properties value object
    /// </summary>
    public class MaterialProperties : ValueObject
    {
        /// <summary>
        /// Material name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Density in kg/m³
        /// </summary>
        public double Density { get; }

        /// <summary>
        /// Young's modulus in Pa
        /// </summary>
        public double YoungsModulus { get; }

        /// <summary>
        /// Poisson's ratio
        /// </summary>
        public double PoissonsRatio { get; }

        /// <summary>
        /// Yield strength in Pa
        /// </summary>
        public double YieldStrength { get; }

        /// <summary>
        /// Ultimate tensile strength in Pa
        /// </summary>
        public double UltimateStrength { get; }

        /// <summary>
        /// Thermal expansion coefficient in 1/K
        /// </summary>
        public double ThermalExpansion { get; }

        public MaterialProperties(string name, double density, double youngsModulus = 0,
            double poissonsRatio = 0, double yieldStrength = 0, double ultimateStrength = 0,
            double thermalExpansion = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Density = Math.Max(0, density);
            YoungsModulus = Math.Max(0, youngsModulus);
            PoissonsRatio = Math.Clamp(poissonsRatio, 0.0, 0.5);
            YieldStrength = Math.Max(0, yieldStrength);
            UltimateStrength = Math.Max(0, ultimateStrength);
            ThermalExpansion = thermalExpansion;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Density;
            yield return YoungsModulus;
            yield return PoissonsRatio;
            yield return YieldStrength;
            yield return UltimateStrength;
            yield return ThermalExpansion;
        }

        // Common materials
        public static MaterialProperties Steel => new MaterialProperties("Steel", 7850, 200e9, 0.3, 250e6, 400e6, 12e-6);
        public static MaterialProperties Aluminum => new MaterialProperties("Aluminum", 2700, 70e9, 0.33, 50e6, 100e6, 23e-6);
        public static MaterialProperties Plastic => new MaterialProperties("Plastic", 1200, 2e9, 0.35, 30e6, 50e6, 100e-6);
        public static MaterialProperties Wood => new MaterialProperties("Wood", 600, 10e9, 0.3, 30e6, 60e6, 5e-6);
    }
}



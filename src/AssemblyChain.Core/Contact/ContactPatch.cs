using Rhino.Geometry;

namespace AssemblyChain.Core.Contact
{
    public readonly struct ContactPatch
    {
        public int I { get; }
        public int J { get; }
        public Vector3d NormalIJ { get; }
        public double Area { get; }
        public double FrictionMu { get; }

        public ContactPatch(int I, int J, Vector3d NormalIJ, double Area, double FrictionMu)
        {
            this.I = I;
            this.J = J;
            this.NormalIJ = NormalIJ;
            this.Area = Area;
            this.FrictionMu = FrictionMu;
        }
    }
}

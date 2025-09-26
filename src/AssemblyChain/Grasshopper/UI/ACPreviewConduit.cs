using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;

namespace AssemblyChain.Gh.Visualization
{
    public class ACPreviewConduit : DisplayConduit
    {
        // --------- 构造函数 ----------
        public ACPreviewConduit()
        {
            ShowParts = true;
            ShowIds = true;
            ShowZones = true;
            Opacity = 0.6;
            Wireframe = false;
            Parts = Array.Empty<PartVis>();
            Zones = Array.Empty<ZoneVis>();
            Relationships = Array.Empty<RelationshipVis>();
        }

        // --------- 公开状态 ----------
        public bool ShowParts { get; set; } = true;
        public bool ShowIds { get; set; } = true;
        public bool ShowZones { get; set; } = true;
        public bool ShowRelationships { get; set; } = false;
        public double Opacity { get; set; } = 0.6;      // 0..1
        public bool Wireframe { get; set; } = false;

        // Part 显示数据
        public IReadOnlyList<PartVis> Parts { get; set; } = Array.Empty<PartVis>();
        // Contact Zone 显示数据
        public IReadOnlyList<ZoneVis> Zones { get; set; } = Array.Empty<ZoneVis>();
        // Relationship 显示数据
        public IReadOnlyList<RelationshipVis> Relationships { get; set; } = Array.Empty<RelationshipVis>();

        // --------- 数据结构 ----------
        public struct PartVis
        {
            public int PartId;
            public Mesh Mesh;           // 用于显示；Brep 可先转 Mesh
            public Point3d LabelPoint;  // 标签位置（质心或自定义）
            public Color Color;         // 稳定色（根据 PartId 生成）
        }

        public struct ZoneVis
        {
            public int PartIdA, PartIdB;
            public Mesh FaceMeshA;      // 可空
            public Mesh FaceMeshB;      // 可空
            public Polyline[] Boundaries;
            public Point3d Centroid;
        }

        public struct RelationshipVis
        {
            public int FromId, ToId;
            public Point3d FromPos, ToPos;
            public bool IsActive;
            public string Reason;
            public double Weight;
            public Color Color;
        }

        protected override void DrawForeground(DrawEventArgs e)
        {
            double tr = 1.0 - Math.Max(0.0, Math.Min(1.0, Opacity));

            // ---- Parts ----
            if (ShowParts)
            {
                foreach (var p in Parts)
                {
                    if (p.Mesh == null) continue;

                    if (Wireframe)
                    {
                        e.Display.DrawMeshWires(p.Mesh, p.Color);
                    }
                    else
                    {
                        var mat = new DisplayMaterial(p.Color)
                        {
                            Transparency = tr,
                            IsTwoSided = true
                        };
                        e.Display.DrawMeshShaded(p.Mesh, mat);
                        e.Display.DrawMeshWires(p.Mesh, System.Drawing.Color.FromArgb(180, 20, 20, 20));
                    }

                    if (ShowIds)
                    {
                        var labelColor = Wireframe ? Color.White : Color.Yellow;
                        var fontSize = Wireframe ? 20 : 18;
                        e.Display.Draw2dText(p.PartId.ToString(), labelColor, p.LabelPoint, true, fontSize);
                    }
                }
            }

            // ---- Contact Zones ----
            if (ShowZones)
            {
                foreach (var z in Zones)
                {
                    if (z.FaceMeshA != null)
                    {
                        var mA = new DisplayMaterial(System.Drawing.Color.FromArgb(255, 255, 80, 80))
                        {
                            Transparency = tr,
                            IsTwoSided = true
                        };
                        e.Display.DrawMeshShaded(z.FaceMeshA, mA);
                    }

                    if (z.FaceMeshB != null)
                    {
                        var mB = new DisplayMaterial(System.Drawing.Color.FromArgb(255, 80, 80, 255))
                        {
                            Transparency = tr,
                            IsTwoSided = true
                        };
                        e.Display.DrawMeshShaded(z.FaceMeshB, mB);
                    }

                    if (z.Boundaries != null)
                    {
                        foreach (var pl in z.Boundaries)
                            e.Display.DrawPolyline(pl, System.Drawing.Color.White, 2);
                    }
                }
            }

            // ---- Relationships ----
            if (ShowRelationships)
            {
                foreach (var rel in Relationships)
                {
                    if (!rel.IsActive) continue;
                    e.Display.DrawLine(rel.FromPos, rel.ToPos, rel.Color, 2);
                    var direction = rel.ToPos - rel.FromPos;
                    direction.Unitize();
                    var arrowSize = 0.1;
                    var arrowPos = rel.ToPos - direction * arrowSize;
                    DrawArrow(e, arrowPos, direction, arrowSize, rel.Color);
                }
            }
        }

        private void DrawArrow(DrawEventArgs e, Point3d position, Vector3d direction, double size, Color color)
        {
            var right = Vector3d.CrossProduct(direction, Vector3d.ZAxis);
            if (right.IsTiny()) right = Vector3d.CrossProduct(direction, Vector3d.XAxis);
            right.Unitize();
            var up = Vector3d.CrossProduct(right, direction);
            up.Unitize();
            var arrowHead1 = position + right * size * 0.3 - up * size * 0.3;
            var arrowHead2 = position - right * size * 0.3 - up * size * 0.3;
            e.Display.DrawLine(position, arrowHead1, color, 1);
            e.Display.DrawLine(position, arrowHead2, color, 1);
        }
    }
}




using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;

namespace AssemblyChain.Gh.Visualization
{
    public class ACDBGConduit : DisplayConduit
    {
        public ACDBGConduit()
        {
            ShowNodes = true;
            ShowEdges = true;
            ShowSCC = true;
            ShowKeyPieces = true;
            ShowParts = false;
            ShowContacts = false;
            Opacity = 0.6;
            Wireframe = false;
            NodeSize = 0.1;
            EdgeWidth = 2.0;
            TextSize = 12;
        }

        // Display properties
        public bool ShowNodes { get; set; } = true;
        public bool ShowEdges { get; set; } = true;
        public bool ShowSCC { get; set; } = true;
        public bool ShowKeyPieces { get; set; } = true;
        public bool ShowParts { get; set; } = false;
        public bool ShowContacts { get; set; } = false;
        public double Opacity { get; set; } = 0.6;
        public bool Wireframe { get; set; } = false;
        public double NodeSize { get; set; } = 0.1;
        public double EdgeWidth { get; set; } = 2.0;
        public int TextSize { get; set; } = 12;

        // Display data
        public List<DBGNodeVis> Nodes { get; set; } = new();
        public List<DBGEdgeVis> Edges { get; set; } = new();
        public List<DBGSCCVis> SccGroups { get; set; } = new();
        public List<DBGKeyPieceVis> KeyPieces { get; set; } = new();
        public List<DBGPartVis> Parts { get; set; } = new();
        public List<DBGContactVis> Contacts { get; set; } = new();

        // Data structures for visualization
        public struct DBGNodeVis
        {
            public int Id;
            public string Name;
            public Point3d Position;
            public bool IsKeyPiece;
            public bool IsActive;
            public Color Color;
        }

        public struct DBGEdgeVis
        {
            public int FromId;
            public int ToId;
            public Point3d FromPos;
            public Point3d ToPos;
            public bool IsActive;
            public string Reason;
            public double Weight;
            public Color Color;
        }

        public struct DBGSCCVis
        {
            public List<int> NodeIds;
            public Point3d Center;
            public double Radius;
            public Color Color;
        }

        public struct DBGKeyPieceVis
        {
            public int NodeId;
            public Point3d Position;
            public string Label;
            public Color Color;
        }

        public struct DBGPartVis
        {
            public int PartId;
            public Mesh Mesh;
            public Point3d LabelPoint;
            public Color Color;
        }

        public struct DBGContactVis
        {
            public int PartIdA, PartIdB;
            public Mesh FaceMeshA;
            public Mesh FaceMeshB;
            public Polyline[] Boundaries;
            public Point3d Centroid;
            public Color Color;
        }

        public void ApplySnapshot(GraphSnapshot snapshot, string showOptions = "ids,dbg", double opacity = 0.6, bool wireframe = false)
        {
            if (snapshot == null) return;

            var options = showOptions.ToLower().Split(',');
            ShowNodes = Array.IndexOf(options, "ids") >= 0 || Array.IndexOf(options, "dbg") >= 0;
            ShowEdges = Array.IndexOf(options, "dbg") >= 0;
            ShowSCC = Array.IndexOf(options, "dbg") >= 0;
            ShowKeyPieces = Array.IndexOf(options, "dbg") >= 0;
            ShowParts = Array.IndexOf(options, "parts") >= 0;
            ShowContacts = Array.IndexOf(options, "contacts") >= 0;

            Opacity = opacity;
            Wireframe = wireframe;

            Nodes.Clear();
            Edges.Clear();
            SccGroups.Clear();
            KeyPieces.Clear();
            Parts.Clear();
            Contacts.Clear();

            foreach (var node in snapshot.Nodes)
            {
                Nodes.Add(new DBGNodeVis
                {
                    Id = node.Id,
                    Name = node.Name,
                    Position = node.Position,
                    IsKeyPiece = node.IsKeyPiece,
                    IsActive = node.IsActive,
                    Color = GetNodeColor(node.Id, node.IsKeyPiece, node.IsActive)
                });
            }

            foreach (var edge in snapshot.Edges)
            {
                var fromNode = snapshot.Nodes.Find(n => n.Id == edge.FromId);
                var toNode = snapshot.Nodes.Find(n => n.Id == edge.ToId);
                if (fromNode == null || toNode == null) continue;
                Edges.Add(new DBGEdgeVis
                {
                    FromId = edge.FromId,
                    ToId = edge.ToId,
                    FromPos = fromNode.Position,
                    ToPos = toNode.Position,
                    IsActive = edge.IsActive,
                    Reason = edge.Reason,
                    Weight = edge.Weight,
                    Color = GetEdgeColor(edge.IsActive, edge.Weight)
                });
            }

            for (int i = 0; i < snapshot.SccGroups.Count; i++)
            {
                var scc = snapshot.SccGroups[i];
                var center = CalculateSCCCenter(scc, snapshot.Nodes);
                var radius = CalculateSCCRadius(scc, snapshot.Nodes, center);
                SccGroups.Add(new DBGSCCVis
                {
                    NodeIds = scc,
                    Center = center,
                    Radius = radius,
                    Color = GetSCCColor(i)
                });
            }

            foreach (var keyId in snapshot.KeyPieceNodeIds)
            {
                var node = snapshot.Nodes.Find(n => n.Id == keyId);
                if (node == null) continue;
                KeyPieces.Add(new DBGKeyPieceVis
                {
                    NodeId = keyId,
                    Position = node.Position,
                    Label = $"Key: {node.Name}",
                    Color = Color.Red
                });
            }
        }

        public void Clear()
        {
            var buffers = new VisualizationBuffers(Nodes, Edges, SccGroups, KeyPieces, Parts, Contacts);
            buffers.ClearAll();
        }

        private readonly struct VisualizationBuffers
        {
            private readonly ICollection<DBGNodeVis> _nodes;
            private readonly ICollection<DBGEdgeVis> _edges;
            private readonly ICollection<DBGSCCVis> _sccGroups;
            private readonly ICollection<DBGKeyPieceVis> _keyPieces;
            private readonly ICollection<DBGPartVis> _parts;
            private readonly ICollection<DBGContactVis> _contacts;

            public VisualizationBuffers(
                ICollection<DBGNodeVis> nodes,
                ICollection<DBGEdgeVis> edges,
                ICollection<DBGSCCVis> sccGroups,
                ICollection<DBGKeyPieceVis> keyPieces,
                ICollection<DBGPartVis> parts,
                ICollection<DBGContactVis> contacts)
            {
                _nodes = nodes ?? Array.Empty<DBGNodeVis>();
                _edges = edges ?? Array.Empty<DBGEdgeVis>();
                _sccGroups = sccGroups ?? Array.Empty<DBGSCCVis>();
                _keyPieces = keyPieces ?? Array.Empty<DBGKeyPieceVis>();
                _parts = parts ?? Array.Empty<DBGPartVis>();
                _contacts = contacts ?? Array.Empty<DBGContactVis>();
            }

            public void ClearAll()
            {
                ClearCollection(_nodes);
                ClearCollection(_edges);
                ClearCollection(_sccGroups);
                ClearCollection(_keyPieces);
                ClearCollection(_parts);
                ClearCollection(_contacts);
            }

            private static void ClearCollection<T>(ICollection<T> collection)
            {
                if (collection == null || collection.Count == 0)
                {
                    return;
                }

                collection.Clear();
            }
        }

        protected override void DrawForeground(DrawEventArgs e)
        {
            // SCC groups
            if (ShowSCC)
            {
                foreach (var scc in SccGroups)
                {
                    var alpha = (int)(Opacity * 50);
                    var color = Color.FromArgb(alpha, scc.Color);
                    e.Display.DrawCircle(new Circle(scc.Center, scc.Radius), color, 2);
                }
            }

            // Edges
            if (ShowEdges)
            {
                foreach (var edge in Edges)
                {
                    if (!edge.IsActive) continue;
                    var alpha = (int)(Opacity * 255);
                    var color = Color.FromArgb(alpha, edge.Color);
                    e.Display.DrawLine(edge.FromPos, edge.ToPos, color, (int)EdgeWidth);

                    var direction = edge.ToPos - edge.FromPos;
                    direction.Unitize();
                    var arrowSize = NodeSize * 0.5;
                    var arrowPos = edge.ToPos - direction * arrowSize;
                    DrawArrow(e, arrowPos, direction, arrowSize, color);
                }
            }

            // Nodes
            if (ShowNodes)
            {
                foreach (var node in Nodes)
                {
                    if (!node.IsActive) continue;
                    var alpha = (int)(Opacity * 255);
                    var color = Color.FromArgb(alpha, node.Color);
                    var sphere = new Sphere(node.Position, NodeSize);
                    e.Display.DrawSphere(sphere, color);
                    var labelPos = node.Position + new Vector3d(0, 0, NodeSize * 1.5);
                    e.Display.Draw2dText(node.Name, color, labelPos, true, TextSize);
                }
            }

            // Parts
            if (ShowParts)
            {
                foreach (var part in Parts)
                {
                    if (part.Mesh == null) continue;
                    var alpha = (int)(Opacity * 128);
                    var color = Color.FromArgb(alpha, part.Color);
                    if (Wireframe)
                        e.Display.DrawMeshWires(part.Mesh, color);
                    else
                        e.Display.DrawMeshShaded(part.Mesh, new DisplayMaterial(color));
                }
            }

            // Contacts
            if (ShowContacts)
            {
                foreach (var contact in Contacts)
                {
                    var alpha = (int)(Opacity * 120);
                    var color = Color.FromArgb(alpha, contact.Color);
                    if (contact.FaceMeshA != null)
                        e.Display.DrawMeshShaded(contact.FaceMeshA, new DisplayMaterial(color));
                    if (contact.FaceMeshB != null)
                        e.Display.DrawMeshShaded(contact.FaceMeshB, new DisplayMaterial(color));
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

        private Color GetNodeColor(int id, bool isKeyPiece, bool isActive)
        {
            if (!isActive) return Color.Gray;
            if (isKeyPiece) return Color.Red;
            var rnd = new Random(id * 73856093);
            return Color.FromArgb(255, rnd.Next(60, 220), rnd.Next(60, 220), rnd.Next(60, 220));
        }

        private Color GetEdgeColor(bool isActive, double weight)
        {
            if (!isActive) return Color.Gray;
            var intensity = Math.Min(255, (int)(weight * 1000));
            return Color.FromArgb(255, intensity, 100, 100);
        }

        private Color GetSCCColor(int index)
        {
            var colors = new[] { Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Cyan };
            return colors[index % colors.Length];
        }

        private Point3d CalculateSCCCenter(List<int> nodeIds, List<GraphNode> nodes)
        {
            var center = Point3d.Origin;
            foreach (var id in nodeIds)
            {
                var node = nodes.Find(n => n.Id == id);
                if (node != null) center += node.Position;
            }
            return nodeIds.Count > 0 ? center / nodeIds.Count : center;
        }

        private double CalculateSCCRadius(List<int> nodeIds, List<GraphNode> nodes, Point3d center)
        {
            double maxDist = 0;
            foreach (var id in nodeIds)
            {
                var node = nodes.Find(n => n.Id == id);
                if (node != null)
                {
                    var dist = center.DistanceTo(node.Position);
                    maxDist = Math.Max(maxDist, dist);
                }
            }
            return maxDist + 0.1;
        }
    }

    // Snapshot support types (minimal placeholders to compile; replace with real ones if available)
    public class GraphSnapshot
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public List<List<int>> SccGroups { get; set; } = new();
        public List<int> KeyPieceNodeIds { get; set; } = new();
    }

    public class GraphNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Point3d Position { get; set; }
        public bool IsKeyPiece { get; set; }
        public bool IsActive { get; set; }
    }

    public class GraphEdge
    {
        public int FromId { get; set; }
        public int ToId { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }
        public double Weight { get; set; }
    }
}




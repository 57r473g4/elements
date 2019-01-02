using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// Extension methods for converting IFC types to Element types.
    /// </summary>
    public static class IFCExtensions
    {
        /// <summary>
        /// Convert an IfcSlab to a Floor.
        /// </summary>
        /// <param name="slab">An IfcSlab.</param>
        public static Floor ToFloor(this IfcSlab slab, IEnumerable<IfcRelContainedInSpatialStructure> relContains)
        {
            // TODO: When inverse are set on instances, this lookup will be unnecessary.
            var storeyRel = relContains.FirstOrDefault(rc => rc.RelatingStructure.GetType() == typeof(IfcBuildingStorey) && rc.RelatedElements.Contains(slab));
            var transform = new Transform();

            // For IFCs that we've tested, relative placements are used.
            // For this reason, we don't need to transform using the building story.
            // if (storeyRel != null)
            // {
            //     var storey = (IfcBuildingStorey)storeyRel.RelatingStructure;
            //     transform.Move(new Vector3(0, 0, storey.Elevation));
            // }

            transform.Concatenate(slab.ObjectPlacement.ToTransform());

            // Check if the slab is contained in a building storey
            foreach (var cis in slab.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            }

            var repItems = slab.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            // Console.WriteLine($"Found representation type: {rep.GetType().ToString()}");
            var foundSolid = repItems.FirstOrDefault(i => i.GetType() == typeof(IFC.IfcExtrudedAreaSolid));
            if (foundSolid == null)
            {
                throw new Exception("No IfcExtrudedAreaSolid could be found in the provided IfcSlab.");
            }

            var solid = (IFC.IfcExtrudedAreaSolid)foundSolid;
            var floorType = new FloorType($"{Guid.NewGuid().ToString()}_floor_type", (IfcLengthMeasure)solid.Depth);
            var profileDef = (IFC.IfcArbitraryClosedProfileDef)solid.SweptArea;
            transform.Concatenate(solid.Position.ToTransform());
            var pline = (IFC.IfcPolyline)profileDef.OuterCurve;
            var outline = pline.ToPolygon(true);
            var floor = new Floor(new Profile(outline), floorType, transform.Origin.Z, BuiltInMaterials.Concrete, transform);
            return floor;
        }

        /// <summary>
        /// Convert an IfcCartesianPoint to a Vector3.
        /// </summary>
        /// <param name="cartesianPoint">An IfcCartesianPoint.</param>
        public static Vector3 ToVector3(this IfcCartesianPoint cartesianPoint)
        {
            return cartesianPoint.Coordinates.ToVector3();
        }

        /// <summary>
        /// Convert a collection of IfcLengthMeasure to a Vector3.
        /// </summary>
        /// <param name="measures">A collection of IfcLengthMeasure.</param>
        public static Vector3 ToVector3(this List<IfcLengthMeasure> measures)
        {
            if (measures.Count == 2)
            {
                return new Vector3(measures[0], measures[1]);
            }
            else if (measures.Count == 3)
            {
                return new Vector3(measures[0], measures[1], measures[2]);
            }
            else
            {
                throw new Exception($"{measures.Count} measures could not be converted to a Vector3.");
            }
        }

        /// <summary>
        /// Convert an IfcPolyline to a Polygon.
        /// </summary>
        /// <param name="polyline">An IfcPolyline.</param>
        public static Polygon ToPolygon(this IfcPolyline polyline, bool dropLastPoint = false)
        {
            var count = dropLastPoint ? polyline.Points.Count - 1 : polyline.Points.Count;
            var verts = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                var v = polyline.Points[i].ToVector3();
                verts[i] = v;
            }
            return new Polygon(verts);
        }

        /// <summary>
        /// Check if an IfcPolyline is closed.
        /// </summary>
        /// <param name="pline"></param>
        /// <returns></returns>
        public static bool IsClosed(this IfcPolyline pline)
        {
            var start = pline.Points[0];
            var end = pline.Points[pline.Points.Count-1];
            return start.Equals(end);
        }

        /// <summary>
        /// Check if two IfcCartesianPoints have the same coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Equals(this IfcCartesianPoint point, IfcCartesianPoint other)
        {
            for(var i=0; i<point.Coordinates.Count; i++)
            {
                if(point.Coordinates[i] != other.Coordinates[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Convert an IfcAxis2Placement3D to a Transform.
        /// </summary>
        /// <param name="cs">An IfcAxis2Placement3D.</param>
        /// <returns></returns>
        public static Transform ToTransform(this IfcAxis2Placement3D cs)
        {
            var x = cs.RefDirection.ToVector3();
            var z = cs.Axis.ToVector3();
            var o = cs.Location.ToVector3();
            var t = new Transform(o, x, z);
            return t;
        }

        /// <summary>
        /// Convert an IfcAxis2Placement2D to a Transform.
        /// </summary>
        /// <param name="cs">An IfcAxis2Placement2D.</param>
        public static Transform ToTransform(this IfcAxis2Placement2D cs)
        {
            var x = cs.RefDirection.ToVector3();
            var z = Vector3.ZAxis;
            var o = cs.Location.ToVector3();
            return new Transform(o, x, z);
        }

        /// <summary>
        /// Convert an IfcDirection to a Vector3.
        /// </summary>
        /// <param name="direction">An IfcDirection.</param>
        public static Vector3 ToVector3(this IfcDirection direction)
        {
            var ratios = direction.DirectionRatios;
            return new Vector3(ratios[0], ratios[1], ratios[2]);
        }

        /// <summary>
        /// Convert an IfcAxis2Placement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcAxis2Placement.</param>
        /// <returns></returns>
        public static Transform ToTransform(this IfcAxis2Placement placement)
        {
            // SELECT IfcAxis2Placement3d, IfcAxis2Placement2d
            if (placement.Choice.GetType() == typeof(IfcAxis2Placement2D))
            {
                var cs = (IfcAxis2Placement2D)placement.Choice;
                return cs.ToTransform();
            }
            else if (placement.Choice.GetType() == typeof(IfcAxis2Placement3D))
            {
                var cs = (IfcAxis2Placement3D)placement.Choice;
                return cs.ToTransform();
            }
            else
            {
                throw new Exception($"The specified placement of type, {placement.GetType().ToString()}, cannot be converted to a Transform.");
            }
        }

        /// <summary>
        /// Convert an IfcLocalPlacement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcLocalPlacement.</param>
        /// <returns></returns>
        public static Transform ToTransform(this IfcLocalPlacement placement)
        {
            var t = placement.RelativePlacement.ToTransform();
            if(placement.PlacementRelTo != null)
            {
                var tr = placement.PlacementRelTo.ToTransform();
                tr.Concatenate(t);
                return tr;
            }
            return t;
        }

        /// <summary>
        /// Convert an IfcObjectPlacement to a Transform.
        /// </summary>
        /// <param name="placement">An IfcObjectPlacement.</param>
        public static Transform ToTransform(this IfcObjectPlacement placement)
        {
            if (placement.GetType() == typeof(IfcLocalPlacement))
            {
                var lp = (IfcLocalPlacement)placement;
                var t = lp.ToTransform();
                return t;
            }
            else if (placement.GetType() == typeof(IfcGridPlacement))
            {
                throw new Exception("IfcGridPlacement conversion to Transform not supported.");
            }
            return null;
        }

        /// <summary>
        /// Convert and IfcSpace to a Space.
        /// </summary>
        /// <param name="space">An IfcSpace.</param>
        /// <returns></returns>
        public static Space ToSpace(this IfcSpace space, IEnumerable<IfcRelContainedInSpatialStructure> relContains)
        {
            var transform = new Transform();
            // var storeyRel = relContains.FirstOrDefault(rc => rc.RelatingStructure.GetType() == typeof(IfcBuildingStorey) && rc.RelatedElements.Contains(space));
            // if (storeyRel != null)
            // {
            //     var storey = (IfcBuildingStorey)storeyRel.RelatingStructure;
            //     transform.Move(new Vector3(0, 0, storey.Elevation));
            // }

            var repItems = space.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            var localPlacement = space.ObjectPlacement.ToTransform();
            transform.Concatenate(localPlacement);

            var foundSolid = repItems.First();
            var material = new Material("space", new Color(1.0f, 0.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            if (foundSolid.GetType() == typeof(IFC.IfcExtrudedAreaSolid))
            {
                var solid = (IFC.IfcExtrudedAreaSolid)foundSolid;
                var profileDef = (IFC.IfcArbitraryClosedProfileDef)solid.SweptArea;
                transform.Concatenate(solid.Position.ToTransform());
                var pline = (IFC.IfcPolyline)profileDef.OuterCurve;
                var outline = pline.ToPolygon(true);
                var result = new Space(new Profile(outline), 0.0, (IfcLengthMeasure)solid.Depth, material, transform);
                return result;
            }
            else if (foundSolid.GetType() == typeof(IFC.IfcFacetedBrep))
            {
                var solid = (IFC.IfcFacetedBrep)foundSolid;
                var shell = solid.Outer;
                var faces = new PlanarFace[shell.CfsFaces.Count];
                for(var i=0; i< shell.CfsFaces.Count; i++)
                {
                    var f = shell.CfsFaces[i];
                    foreach (var b in f.Bounds)
                    {
                        var loop = (IFC.IfcPolyLoop)b.Bound;
                        var poly = loop.Polygon.ToPolygon();
                        faces[i] = new PlanarFace(poly);
                    }
                }
                var newSpace = new Space(material, transform);
                newSpace.SetFaces(faces);
                return newSpace;
            }

            return null;
        }

        /// <summary>
        /// Convert a collection of IfcCartesianPoint to a Polygon.
        /// </summary>
        /// <param name="loop">A collection of IfcCartesianPoint</param>
        public static Polygon ToPolygon(this List<IfcCartesianPoint> loop)
        {
            var verts = new Vector3[loop.Count];
            for (var i = 0; i < loop.Count; i++)
            {
                verts[i] = loop[i].ToVector3();
            }
            return new Polygon(verts);
        }

        /// <summary>
        /// Convert an IfcPolyloop to a Polygon.
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static Polygon ToPolygon(this IfcPolyLoop loop)
        {
            return loop.Polygon.ToPolygon();
        }
    }
}
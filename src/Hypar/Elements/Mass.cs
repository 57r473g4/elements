using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A mass represents an extruded building mass.
    /// </summary>
    public class Mass : Element, ITessellate<Mesh>
    {
        private List<Polyline> _sides = new List<Polyline>();
        private Polyline _bottom;
        private Polyline _top;
        private double _bottomElevation;
        private double _topElevation;

        /// <summary>
        /// The bottom perimeter of the Mass.
        /// </summary>
        /// <returns></returns>
        public Polyline Bottom => _bottom;

        /// <summary>
        /// The elevation of the bottom perimeter.
        /// </summary>
        public double BottomElevation => _bottomElevation;

        /// <summary>
        /// The top perimeter of the mass.
        /// </summary>
        /// <returns></returns>
        public Polyline Top => _top;

        /// <summary>
        /// The elevation of the top perimeter.
        /// </summary>
        public double TopElevation => _topElevation;

        /// <summary>
        /// Construct a default mass.
        /// </summary>
        public Mass():base(BuiltInMaterials.Mass)
        {
            var defaultProfile = Profiles.Rectangular();
            this._top = defaultProfile;
            this._bottom = defaultProfile;
            this._bottomElevation = 0.0;
            this._topElevation = 1.0;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// Construct a mass from perimeters and elevations.
        /// </summary>
        /// <param name="bottom">The bottom perimeter of the mass.</param>
        /// <param name="bottomElevation">The elevation of the bottom perimeter.</param>
        /// <param name="top">The top perimeter of the mass.</param>
        /// <param name="topElevation">The elevation of the top perimeter.</param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Mass(Polyline bottom, double bottomElevation, Polyline top, double topElevation, Transform transform = null) : base(BuiltInMaterials.Mass, transform)
        {
            if (bottom.Vertices.Count() != top.Vertices.Count())
            {
                throw new ArgumentException(Messages.PROFILES_UNEQUAL_VERTEX_EXCEPTION);
            }

            if (topElevation <= bottomElevation)
            {
                throw new ArgumentOutOfRangeException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "topElevation");
            }

            this._top = top;
            this._bottom = bottom;
            this._bottomElevation = bottomElevation;
            this._topElevation = topElevation;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> VerticalEdges()
        {
            foreach(var f in Faces())
            {
                yield return f.Segments().ElementAt(1);
            }
            
        }

        /// <summary>
        /// A collection of curves representing the horizontal edges of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> HorizontalEdges()
        {
            foreach(var f in Faces())
            {
                yield return f.Segments().ElementAt(0);
                yield return f.Segments().ElementAt(2);
            }
        }

        /// <summary>
        /// A collection of polylines representing the perimeter of each face of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polyline> Faces()
        {
            var b = this._bottom.Vertices.ToArray();
            var t = this._top.Vertices.ToArray();

            for (var i = 0; i < b.Length; i++)
            {
                var next = i + 1;
                if (i == b.Length - 1)
                {
                    next = 0;
                }
                var v1 = b[i];
                var v2 = b[next];
                var v3 = t[next];
                var v4 = t[i];
                var v1n = new Vector3(v1.X, v1.Y, this._bottomElevation);
                var v2n = new Vector3(v2.X, v2.Y, this._bottomElevation);
                var v3n = new Vector3(v3.X, v3.Y, this._topElevation);
                var v4n = new Vector3(v4.X, v4.Y, this._topElevation);
                yield return new Polyline(new[] { v1n, v2n, v3n, v4n });
            }
        }

        /// <summary>
        /// For each face of the mass apply a creator function.
        /// </summary>
        /// <param name="creator">The function to apply.</param>
        /// <typeparam name="T">The creator function's return type.</typeparam>
        /// <returns></returns>
        public IEnumerable<T> ForEachFaceCreateElementsOfType<T>(Func<Polyline, IEnumerable<T>> creator)
        {
            var results = new List<T>();
            foreach(var p in Faces())
            {
                results.AddRange(creator(p));
            }
            return results;
        }
        
        /// <summary>
        /// Tessellate the mass.
        /// </summary>
        /// <returns>A mesh representing the tessellated mass.</returns>
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            foreach (var s in Faces())
            {
                mesh.AddQuad(s.ToArray());
            }

            mesh.AddTesselatedFace(new[] { this.Bottom }, this.BottomElevation);
            mesh.AddTesselatedFace(new[] { this.Top }, this.TopElevation, true);
            return mesh;
        }

        /// <summary>
        /// Set the bottom profile of the mass.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static Mass WithBottomProfile(Polyline profile)
        {
            if(profile.Count() == 0)
            {
                throw new ArgumentException(Messages.EMPTY_POLYLINE_EXCEPTION, "profile");
            }

            var mass = new Mass();
            mass._bottom = profile;
            return mass;
        }

        /// <summary>
        /// Set the top elevation of the mass.
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public Mass WithTopAtElevation(double elevation)
        {
            if(elevation <= this._bottomElevation)
            {
                throw new ArgumentException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "elevation");
            }
            this._topElevation = elevation;
            return this;
        }

        /// <summary>
        /// Set the bottom elevation of the mass.
        /// </summary>
        /// <param name="elevation"></param>
        /// <returns></returns>
        public Mass WithBottomAtElevation(double elevation)
        {
            if(elevation >= this._topElevation)
            {
                throw new ArgumentException(Messages.BOTTOM_ABOVE_TOP_EXCEPTION, "elevation");
            }
            this._bottomElevation = elevation;
            return this;
        }

        /// <summary>
        /// Set the top profile of the mass.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Mass WithTopProfile(Polyline profile)
        {
            if(profile.Count() == 0)
            {
                throw new ArgumentException(Messages.EMPTY_POLYLINE_EXCEPTION, "profile");
            }
            this._top = profile;
            return this;
        }
    }
}
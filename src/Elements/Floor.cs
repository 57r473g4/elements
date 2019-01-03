using Elements.Geometry;
using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using Elements.Serialization;

namespace Elements
{
    /// <summary>
    /// A Floor is a horizontal element defined by a perimeter and one or several voids.
    /// </summary>
    public class Floor : Element, IElementTypeProvider<FloorType>, IGeometry3D, IProfileProvider
    {
        /// <summary>
        /// The elevation from which the Floor is extruded.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The FloorType of the Floor.
        /// </summary>
        [JsonProperty("element_type")]
        public FloorType ElementType { get; }

        /// <summary>
        /// The Profile of the Floor.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// The transformed Profile of the Floor.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The thickness of the Floor's extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.ElementType.Thickness; }
        }

        /// <summary>
        /// The Floor's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry { get; }

        /// <summary>
        /// Construct a Floor.
        /// </summary>
        /// <param name="profile">The IProfile of the Floor.</param>
        /// <param name="elementType">The ElementType of the Floor.</param>
        /// <param name="elevation">The elevation of the Floor.</param>
        /// <param name="material">The Floor's Material.</param>
        /// <param name="transform">The Floor's transform.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the slab's thickness is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Floor(IProfile profile, FloorType elementType, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            this.Profile = profile;
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Geometry = new[]{new Extrude(this.Profile, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// Construct a Floor.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="elementType"></param>
        /// <param name="elevation"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Floor(Polygon profile, FloorType elementType, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            this.Profile = new Profile(profile);
            this.Elevation = elevation;
            this.ElementType = elementType;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Geometry = new[]{new Extrude(this.Profile, this.Thickness, material == null ? BuiltInMaterials.Concrete : material)};
        }

        /// <summary>
        /// The area of the Floor.
        /// Overlapping openings and openings which are outside of the Floor's perimeter,
        /// will result in incorrect area results.
        /// </summary>
        public double Area()
        {
            return this.Profile.Area();
        }
    }
}
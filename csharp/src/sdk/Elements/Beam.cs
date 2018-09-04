using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public class Beam : Element, ILocateable<Line>, ITessellate<Mesh>, ITransformable, IMaterialize
    {
        /// <summary>
        /// The cross-section profile of the beam.
        /// </summary>
        [JsonProperty("profile")]
        public Polyline Profile{get;}
        
        /// <summary>
        /// The up axis of the beam.
        /// </summary>
        [JsonProperty("up_axis")]
        public Vector3 UpAxis{get;}

        /// <summary>
        /// The center line of the beam.
        /// </summary>
        /// <value></value>
        [JsonProperty("location")]
        public Line Location{get;}

        /// <summary>
        /// The transform of the beam.
        /// </summary>
        /// <value></value>
        [JsonProperty("transform")]
        public Transform Transform{get;}

        /// <summary>
        /// The beam's material.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material {get;set;}

        /// <summary>
        /// Construct a default beam.
        /// </summary>
        /// <returns></returns>
        public Beam(Line centerLine, Polyline profile)
        {
            this.Location = centerLine;
            this.Profile = profile;
            this.Material = BuiltInMaterials.Default;
        }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The center line of the beam.</param>
        /// <param name="profile">The structural profile of the beam.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="up">The up axis of the beam.</param>
        public Beam(Line centerLine, Polyline profile, Material material, Vector3 up = null)
        {
            this.Profile = profile;
            this.Location = centerLine;

            if(up != null)
            {
                this.UpAxis = up;
            }

            this.Material = material;
            this.Transform = centerLine.GetTransform(up);
        }

        /// <summary>
        /// Tessellate the beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated beam.</returns>
        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this.Location, new[] { this.Profile });
        }
    }
}
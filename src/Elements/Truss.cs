using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A Truss is an aggregation of StructuralFraming Elements.
    /// </summary>
    public class Truss : Element, IAggregateElement
    {
        private List<Beam> _web = new List<Beam>();
        private List<Beam> _topChord = new List<Beam>();
        private List<Beam> _bottomChord = new List<Beam>();
        
        /// <summary>
        /// The Elements aggregated by this Element.
        /// </summary>
        [JsonProperty("elements")]
        public List<Element> Elements{get;}

        /// <summary>
        /// The start of the Truss.
        /// </summary>
        [JsonProperty("start")]
        public Vector3 Start{get;}
        
        /// <summary>
        /// The end of the Truss.
        /// </summary>
        [JsonProperty("end")]
        public Vector3 End{get;}

        /// <summary>
        /// The depth of the Truss.
        /// </summary>
        [JsonProperty("depth")]
        public double Depth{get;}

        /// <summary>
        /// The number of divisions in the Truss.
        /// </summary>
        [JsonProperty("divisions")]
        public int Divisions{get;}

        /// <summary>
        /// The Profile used for members in the top chord of the Truss.
        /// </summary>
        [JsonProperty("top_chord_profile")]
        public Profile TopChordProfile{get;}

        /// <summary>
        /// The Profile used for members in the bottom chord of the Truss.
        /// </summary>
        [JsonProperty("bottom_chord_profile")]
        public Profile BottomChordProfile{get;}

        /// <summary>
        /// The Profile used for members in the web of the Truss.
        /// </summary>
        [JsonProperty("web_profile")]
        public Profile WebProfile{get;}

        /// <summary>
        /// Construct a Truss.
        /// </summary>
        /// <param name="start">The start of the Truss.</param>
        /// <param name="end">The end of the Truss.</param>
        /// <param name="depth">The depth of the Truss.</param>
        /// <param name="divisions">The number of panels in the Truss.</param>
        /// <param name="topChordProfile">The Profile to be used for the top chord.</param>
        /// <param name="bottomChordProfile">The Profile to be used for the bottom chord.</param>
        /// <param name="webProfile">The Profile to be used for the web.</param>
        /// <param name="material">The truss' material.</param>
        /// <param name="startSetback">A setback to apply to the start of all members of the Truss.</param>
        /// <param name="endSetback">A setback to apply to the end of all members of the Truss.</param>
        [JsonConstructor]
        public Truss(Vector3 start, Vector3 end, double depth, int divisions, Profile topChordProfile, Profile bottomChordProfile, Profile webProfile, Material material, double startSetback = 0.0, double endSetback = 0.0)
        {
            if(depth <= 0)
            {
                throw new ArgumentOutOfRangeException($"The provided depth ({depth}) must be greater than 0.0.");
            }

            this.Start = start;
            this.End = end;
            this.Depth = depth;
            this.Divisions = divisions;
            this.TopChordProfile = topChordProfile;
            this.BottomChordProfile = bottomChordProfile;
            this.WebProfile = webProfile;

            var l = new Line(start, end);
            var pts = Vector3.AtNEqualSpacesAlongLine(l, divisions, true);
            for(var i=0; i<pts.Count; i++)
            {
                if(i != pts.Count-1)
                {
                    var bt = new Line(pts[i], pts[i+1]);
                    var bb = new Line(pts[i] - new Vector3(0,0,depth), pts[i+1] - new Vector3(0,0,depth));
                    this._topChord.Add(new Beam(bt, topChordProfile, material, null, startSetback, endSetback));
                    this._bottomChord.Add(new Beam(bb, bottomChordProfile, material, null, startSetback, endSetback));
                    var diag = i>Math.Ceiling((double)divisions/2) ? new Line(pts[i], pts[i+1] - new Vector3(0,0,depth)) : new Line(pts[i+1], pts[i] - new Vector3(0,0,depth));
                    this._web.Add(new Beam(diag, webProfile, material, null, startSetback, endSetback));
                }
                var wb = new Line(pts[i], pts[i] - new Vector3(0,0,depth));
                this._web.Add(new Beam(wb, webProfile, material, null, startSetback, endSetback));
            }

            this.Elements = new List<Element>();
            this.Elements.AddRange(this._topChord);
            this.Elements.AddRange(this._bottomChord);
            this.Elements.AddRange(this._web);
        }
    }
}
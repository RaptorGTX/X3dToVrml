/*
Copyright(c) 2017 RaptorGTX

This software is provided 'as-is', without any express or implied
warranty.In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software.If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace X3dToVrml2
{
    /// <summary>
    /// This class is used to store data of an IndexedFaceSet with its parent Transform's data
    /// </summary>
    public class TransformableIndexedFaceSet
    {
        public String Name { get; set; }

        public Vector3 Translation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector4 Rotation { get; set; }

        public float[] Coordinates { get; set; }
        public float[] TextureCoordinates { get; set; }
        public int[] CoordinateIndex { get; set; }
        public int[] TextureCoordinateIndex { get; set; }

        public TransformableIndexedFaceSet()
        {
        }

        /// <summary>
        /// Converts data stored in this object to VRML2 code
        /// </summary>
        /// <returns>Returns this class's VRML2 code equivalent</returns>
        public string toVrmlNode()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("DEF " + Name + " Transform {\n");
            sb.Append(string.Format("\ttranslation {0:f6} {1:f6} {2:f6}\n", Translation.X, Translation.Y, Translation.Z));
            sb.Append(string.Format("\tscale {0:f6} {1:f6} {2:f6}\n", Scale.X, Scale.Y, Scale.Z));
            sb.Append(string.Format("\trotation {0:f6} {1:f6} {2:f6} {3:f6}\n\n", Rotation.X, Rotation.Y, Rotation.Z, Rotation.W));
            sb.Append("\tchildren [\n\t\tShape {\n\t\t\tappearance Appearance {\n\t\t\t\tmaterial Material {\n\t\t\t\t\tdiffuseColor 0.8 0.8 0.8\n\t\t\t\t}\n\n\t\t\t}\n\n\t\t\tgeometry IndexedFaceSet {\n\t\t\t\tcoord Coordinate {\n\t\t\t\t\tpoint [");

            foreach (float coord in this.Coordinates)
            {
                sb.Append(string.Format("{0:f6} ", coord));
            }

            sb.Append("]\n\t\t\t\t}\n\n\t\t\t\ttexCoord TextureCoordinate {\n\t\t\t\t\tpoint [");
            if (this.TextureCoordinates != null)
            {
                foreach (float coord in this.TextureCoordinates)
                {
                    sb.Append(string.Format("{0:f6} ", coord));
                }
            }

            sb.Append("]\n\t\t\t\t}\n\n\t\t\t\ttexCoordIndex [");
            if (this.TextureCoordinateIndex != null)
            {
                foreach (int index in this.TextureCoordinateIndex)
                {
                    sb.Append(index + " ");
                }
            }

            sb.Append("]\n\t\t\t\tcoordIndex [");
            foreach (int index in this.CoordinateIndex)
            {
                sb.Append(index + " ");
            }

            sb.Append("]\n\n\t\t\t\tcolorPerVertex FALSE\n\t\t\t\tsolid FALSE\n\t\t\t}\n\t\t}\n\t]\n}\n\n");

            return sb.ToString();
        }
    }
}

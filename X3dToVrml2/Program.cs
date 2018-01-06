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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace X3dToVrml2
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = "";
            // string outputFilePath = "";

            // Extract parameters and put them in the right variables
            // Expected format is -param value -param2 value2
            Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length > 0 && args[i][0] == '-')
                {
                    // Check if next argument is value, if it is add it with this command
                    if ((i + 1) < args.Length && args[i + 1].Length > 0 && !(args[i + 1][0] == '-'))
                    {
                        keyValuePair.Add(args[i], args[i + 1]);
                        i++; // No need to go over the next one
                    }
                    else
                    {
                        keyValuePair.Add(args[i], string.Empty);
                    }
                }
            }

            // String arguments
            inputFilePath = keyValuePair.ContainsKey("-input") ? keyValuePair["-input"] : "";

            // Check if input file exists and load it
            HtmlDocument htmlDocument = new HtmlDocument();

            if (File.Exists(inputFilePath))
            {
                htmlDocument.Load(inputFilePath, Encoding.UTF8);
            }
            else
            {
                Console.WriteLine("Failed to convert " + inputFilePath + ". File not found.");
            }

            // Extract all the transforms that contain another transform containing an IndexedFaceSets
            var transforms = htmlDocument.DocumentNode.Descendants().Where(x => x.Name == "transform" &&
                x.Descendants().FirstOrDefault(y => y.Name == "transform") != null &&
                x.Descendants().FirstOrDefault(y => y.Name == "transform").Descendants().FirstOrDefault(z => z.Name == "indexedfaceset") != null);

            // Parse all the transforms and store the info in a list
            List<TransformableIndexedFaceSet> objects = new List<TransformableIndexedFaceSet>();
            foreach (var transform in transforms)
            {
                TransformableIndexedFaceSet tifs = new TransformableIndexedFaceSet();

                if (transform.Attributes != null && transform.Attributes["def"] != null)
                {
                    tifs.Name = transform.Attributes["def"].Value;
                }

                if (transform.Attributes != null && transform.Attributes["translation"] != null)
                {
                    var translation = transform.Attributes["translation"].Value.Split(' ');
                    tifs.Translation = new Vector3(float.Parse(translation[0]), float.Parse(translation[1]), float.Parse(translation[2]));
                }

                if (transform.Attributes != null && transform.Attributes["scale"] != null)
                {
                    var scale = transform.Attributes["scale"].Value.Split(' ');
                    tifs.Scale = new Vector3(float.Parse(scale[0]), float.Parse(scale[1]), float.Parse(scale[2]));
                }

                if (transform.Attributes != null && transform.Attributes["rotation"] != null)
                {
                    var rotation = transform.Attributes["rotation"].Value.Split(' ');
                    tifs.Rotation = new Vector4(float.Parse(rotation[0]), float.Parse(rotation[1]), float.Parse(rotation[2]), float.Parse(rotation[3]));
                }

                // Get this ifs
                var indexedFaceSet = transform.Descendants().FirstOrDefault(y => y.Name == "transform").Descendants().FirstOrDefault(z => z.Name == "indexedfaceset");

                // Tex coords indices
                if (indexedFaceSet.Attributes != null && indexedFaceSet.Attributes["texCoordIndex"] != null)
                {
                    string texCoordIdxAttribute = indexedFaceSet.Attributes["texCoordIndex"].Value;

                    List<int> texCoordIdx = new List<int>();
                    foreach (var texCoord in texCoordIdxAttribute.Split(' '))
                    {
                        if (texCoord == string.Empty)
                            continue;

                        texCoordIdx.Add(int.Parse(texCoord));
                    }

                    tifs.TextureCoordinateIndex = texCoordIdx.ToArray();
                }

                // Coord indices
                if (indexedFaceSet.Attributes != null && indexedFaceSet.Attributes["coordIndex"] != null)
                {
                    string coordIdxAttribute = indexedFaceSet.Attributes["coordIndex"].Value;

                    List<int> coordIdx = new List<int>();
                    foreach (var coord in coordIdxAttribute.Split(' '))
                    {
                        if (coord == string.Empty)
                            continue;

                        coordIdx.Add(int.Parse(coord));
                    }

                    tifs.CoordinateIndex = coordIdx.ToArray();
                }

                // Coords
                var coordinate = indexedFaceSet.Descendants().FirstOrDefault(x => x.Name == "coordinate");

                if (coordinate.Attributes != null && coordinate.Attributes["point"] != null)
                {
                    string pointAttribute = coordinate.Attributes["point"].Value;

                    List<float> points = new List<float>();
                    foreach (var point in pointAttribute.Split(' '))
                    {
                        if (point == string.Empty)
                            continue;

                        points.Add(float.Parse(point));
                    }

                    tifs.Coordinates = points.ToArray();
                }

                // Texture coordinates
                var textureCoordinate = indexedFaceSet.Descendants().FirstOrDefault(x => x.Name == "texturecoordinate");

                if (textureCoordinate != null && textureCoordinate.Attributes != null && textureCoordinate.Attributes["point"] != null)
                {
                    string pointAttribute = textureCoordinate.Attributes["point"].Value;

                    List<float> points = new List<float>();
                    foreach (var point in pointAttribute.Split(' '))
                    {
                        if (point == string.Empty)
                            continue;

                        points.Add(float.Parse(point));
                    }

                    tifs.TextureCoordinates = points.ToArray();
                }

                objects.Add(tifs);
            }

            // Create the VRML document and write it to output file
            StringBuilder sb = new StringBuilder();

            sb.Append("#VRML V2.0 utf8\n\n");
            
            foreach (var obj in objects)
            {
                sb.Append(obj.toVrmlNode());
            }

            File.WriteAllText(inputFilePath.Replace(".x3d", ".wrl"), sb.ToString(), Encoding.UTF8);
        }
    }
}

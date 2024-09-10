using System;
using System.Xml;

namespace GULP.Graphics.Tiled;

public class Layer
{
    public readonly int[] LayerData;
    public int Id { get; private set; }
    public string LayerName { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Layer(int id, string layerName, int width, int height, int[] layerData)
    {
        Id = id;
        LayerName = layerName;
        Width = width;
        Height = height;
        LayerData = layerData;
    }

    public static Layer Load(XmlReader reader, int id, string layerName, int width, int height)
    {
        while (reader.Read())
        {
            var subtreeReaderName = reader.Name;

            //in doing this I'm making a key assumption that our layer data is stored in CSV only
            //this may be reconsidered if our map/layer size greatly increases in the future and I need to base64 encode
            if (subtreeReaderName == "data" && reader.NodeType == XmlNodeType.Element)
            {
                //read, cleanup starting newline character, split into an array of tile numbers
                string rawCsv = reader.ReadElementContentAsString();
                rawCsv = rawCsv.Replace("\n", "");
                string[] rawCsvSplit = rawCsv.Split(',');

                //convert to integer to make using them later easier
                int[] intLayerData = Array.ConvertAll(rawCsvSplit, int.Parse);
                return new Layer(id, layerName, width, height, intLayerData);
            }
        }

        throw new ArgumentNullException("Not able to find a layer data for layer: " + layerName);
    }
}
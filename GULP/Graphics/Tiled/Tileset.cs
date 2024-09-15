using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Tiled;

public class Tileset
{
    public readonly List<Tile> Tiles = new();

    public int Firstgid { get; private set; }
    public string Name { get; private set; }
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public int TileCount { get; private set; }
    public int Columns { get; private set; }

    public string ImageSource { get; private set; }
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }

    public static Tileset Load(string filename, ContentManager content, int firstgid)
    {
        var result = new Tileset
        {
            Firstgid = firstgid
        };

        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Parse
        };

        Dictionary<int, Rectangle> objects = new();

        //scope of the using definiton is automatically defined as end of the current code block
        using var stream = File.OpenText(Path.Combine(content.RootDirectory, "Tiled", filename));
        using var reader = XmlReader.Create(stream, settings);
        while (reader.Read())
        {
            var name = reader.Name;
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    if (name != "tileset")
                        throw new Exception("invalid tileset format");
                    break;
                case XmlNodeType.Element:
                    switch (name)
                    {
                        case "tileset":
                            result.Name = reader.GetAttribute("name");
                            result.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ??
                                                         throw new InvalidOperationException());
                            result.TileHeight = int.Parse(reader.GetAttribute("tileheight") ??
                                                          throw new InvalidOperationException());
                            result.TileCount = int.Parse(reader.GetAttribute("tilecount") ??
                                                         throw new InvalidOperationException());
                            result.Columns = int.Parse(reader.GetAttribute("columns") ?? throw new
                                InvalidOperationException());
                            break;
                        case "image":
                            result.ImageSource = reader.GetAttribute("source");
                            result.ImageWidth =
                                int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
                            result.ImageHeight =
                                int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
                            break;
                        case "tile":
                            var tileId = int.Parse(reader.GetAttribute("id") ?? throw new InvalidOperationException());

                            //TODO this should be a separate method...
                            var st = reader.ReadSubtree();
                            st.Read();
                            while (st.Read())
                            {
                                var stName = st.Name;
                                switch (st.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        switch (stName)
                                        {
                                            case "object":
                                                var objectType = st.GetAttribute("type");
                                                //eventually we'll need to handle other object types
                                                if (objectType == "Collision")
                                                {
                                                    var x = int.Parse(reader.GetAttribute("x") ??
                                                                      throw new InvalidOperationException());
                                                    var y = int.Parse(reader.GetAttribute("y") ??
                                                                      throw new InvalidOperationException());
                                                    var width = int.Parse(reader.GetAttribute("width") ??
                                                                          throw new InvalidOperationException());
                                                    var height = int.Parse(reader.GetAttribute("height") ??
                                                                           throw new InvalidOperationException());

                                                    objects.Add(tileId, new Rectangle(x, y, width, height));
                                                }

                                                break;
                                        }

                                        break;
                                    case XmlNodeType.EndElement:
                                        break;
                                    case XmlNodeType.Whitespace:
                                        break;
                                }
                            }

                            break;
                    }

                    break;
                case XmlNodeType.EndElement:
                    break;
                case XmlNodeType.Whitespace:
                    break;
            }
        }

        //load our tileset texture, then create tile objects that we can reference later
        var imageSourceName =
            result.ImageSource?.Remove(result.ImageSource.Length -
                                       4); //removing our suffixes in order to use ContentManager
        var texture2D = content.Load<Texture2D>("Tiled/" + imageSourceName);

        for (int i = 0; i < result.ImageHeight; i += result.TileHeight)
        {
            for (int j = 0; j < result.ImageWidth; j += result.TileWidth)
            {
                //calculate the id of this tile based on the first gid, and how many tiles we've processed before
                //local index in just this tileset
                var localId = j / result.TileWidth + i / result.TileHeight * result.Columns;
                //global id amongst all the tilesets is how tiled handles it...
                var globalId = firstgid + localId;

                objects.TryGetValue(localId, out var rect);
                //j and i represent top left corner of the tile
                result.Tiles.Add(new Tile(texture2D, j, i, result.TileWidth, result.TileHeight, globalId, rect));
            }
        }

        return result;
    }
}
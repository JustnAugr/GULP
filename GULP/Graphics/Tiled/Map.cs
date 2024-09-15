using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Tiled;

public class Map
{
    public readonly List<Tileset> Tilesets = new();
    public readonly List<Layer> Layers = new();

    public int Width { get; private set; } //in tiles
    public int Height { get; private set; } //in tiles
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }

    public int PixelWidth => Width * TileWidth;
    public int PixelHeight => Height * TileHeight;

    public static Map Load(string filename, ContentManager contentManager)
    {
        var result = new Map();
        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Parse
        };

        //scope of the using definition is automatically defined as end of the current code block
        using var stream = File.OpenText(filename);
        using var reader = XmlReader.Create(stream, settings);
        while (reader.Read())
        {
            var name = reader.Name;
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    if (name != "map")
                        throw new Exception("invalid map format");
                    break;
                case XmlNodeType.Element:
                    switch (name)
                    {
                        case "map":
                        {
                            result.Width =
                                int.Parse(reader.GetAttribute("width") ?? throw new InvalidOperationException());
                            result.Height =
                                int.Parse(reader.GetAttribute("height") ?? throw new InvalidOperationException());
                            result.TileWidth = int.Parse(reader.GetAttribute("tilewidth") ??
                                                         throw new InvalidOperationException());
                            result.TileHeight = int.Parse(reader.GetAttribute("tileheight") ??
                                                          throw new InvalidOperationException());
                        }
                            break;
                        case "tileset":
                        {
                            int firstgid = int.Parse(reader.GetAttribute("firstgid") ??
                                                     throw new InvalidOperationException());
                            string source = reader.GetAttribute("source") ?? throw new InvalidOperationException();
                            var tileset = Tileset.Load(source, contentManager, firstgid);
                            result.Tilesets.Add(tileset);
                            result.Tilesets.Sort((tileset1, tileset2) =>
                                tileset1.Firstgid.CompareTo(tileset2.Firstgid));
                        }
                            break;
                        case "layer":
                        {
                            int id = int.Parse(reader.GetAttribute("id") ?? throw new InvalidOperationException());
                            string layerName = reader.GetAttribute("name") ?? throw new InvalidOperationException();
                            int width = int.Parse(reader.GetAttribute("width") ??
                                                  throw new InvalidOperationException());
                            int height = int.Parse(reader.GetAttribute("height") ??
                                                   throw new InvalidOperationException());
                            //delve into "data"
                            using var subtreeReader = reader.ReadSubtree();
                            subtreeReader.Read();

                            var layer = Layer.Load(subtreeReader, id, layerName, width, height);
                            result.Layers
                                .Add(layer);
                            result.Layers.Sort((layer1, layer2) => layer1.Id.CompareTo(layer2.Id));
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

        //now it's time to take out tilesets, use them to create Texture[] arrays in our layers
        //then in the draw method we can just simply draw the Texture[] as needed

        return result;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //the size of our map in pixels
        var height = Height * TileHeight;
        var width = Width * TileWidth;

        var tileIndex = 0;
        //draw our map one tile at a time, layer by layer, then row by row
        //so we draw layer 1->4 at tile 1, then layer 1->4 at tile 2 etc
        for (var i = 0; i < height; i += TileHeight)
        {
            for (var j = 0; j < width; j += TileWidth)
            {
                //this should already be sorted
                foreach (var layer in Layers)
                {
                    var tileNumber = layer.LayerData[tileIndex];

                    //transparent, doesn't need to actually be drawn
                    if (tileNumber == 0)
                        continue;

                    var tile = GetTile(tileNumber);
                    tile.Draw(spriteBatch, new Vector2(j, i));
                }

                //after drawing every layer for this tile, we can draw the next one
                tileIndex++;
            }
        }
    }

    private Tile GetTile(int tileNumber)
    {
        //return the last tileset whose firstgid is <= our tilenumber
        //ie if our tileNumber is 40 and the first gid of this tileset is 35 but the next tileset's first gid is 50
        //then we want the one with firsgid=35
        //todo realistically we won't ever, and shouldn't ever, have many tilesets because we want to minimize different textures, but I would like to optimize this - i hate doing the where and maxby separately
        var tileset = Tilesets.Where(ts => ts.Firstgid <= tileNumber).MaxBy(ts => ts.Firstgid);
        return tileset.Tiles[tileNumber - tileset.Firstgid]; //offset it by the firstgid as tileNumber is absolute
    }

    public Tile GetTile(float x, float y, int layer)
    {
        //quick math to convert an x and y coordinate on our map into the index of our tile array
        var xIndex = (int)Math.Floor(x) / TileWidth;
        var yIndex = (int)Math.Floor(y) / TileHeight;
        var cols = Width;

        //the index of this tile in our tileData array
        var tileIndex = xIndex + yIndex * cols;
        //the number of that tile in our raw csv
        var tileNumber = Layers[layer].LayerData[tileIndex];

        //0 meaning it was an empty air tile
        return tileNumber == 0 ? null : GetTile(tileNumber);
    }
}
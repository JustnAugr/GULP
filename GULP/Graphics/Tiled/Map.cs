using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Tiled;

public class Map
{
    public readonly List<Tileset> Tilesets = new();
    public readonly List<Layer> Layers = new();

    private readonly Dictionary<int, Tile> _tileCache = new();

    public int Width { get; private set; } //in tiles
    public int Height { get; private set; } //in tiles
    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public Camera Camera { get; set; }

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

                            //don't sort for now, instead rely on file order which is more consistent I find
                            //result.Layers.Sort((layer1, layer2) => layer1.Id.CompareTo(layer2.Id));
                        }
                            break;
                        case "objectgroup":
                        {
                            //TODO write objectGroup layer loading: will be used for spawn locations
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

    public void Update(GameTime gameTime)
    {
        //TODO I think we can do something smarter here rather than update every single tile regardless of if its being drawn...
        foreach (var tileset in Tilesets)
        {
            tileset.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //the size of our map in pixels
        var height = Height * TileHeight;
        var width = Width * TileWidth;
        var iStart = 0;
        var jStart = 0;

        //alter our start and end loop values if the camera is available, because we can easily cull a lot of the tiles
        //if our camera is showing a smaller area than the ENTIRE map
        if (Camera != null)
        {
            iStart = Math.Max((int)Math.Floor(Camera.Top) / TileHeight * TileHeight - TileHeight, iStart);
            jStart = Math.Max((int)Math.Floor(Camera.Left) / TileWidth * TileWidth - TileWidth, jStart);

            height = Math.Min((int)Math.Floor(Camera.Bottom) / TileHeight * TileHeight + TileHeight, height);
            width = Math.Min((int)Math.Floor(Camera.Right) / TileWidth * TileWidth + TileWidth, width);
        }

        //draw our map one tile at a time, layer by layer, then row by row
        //so we draw layer 1->4 at tile 1, then layer 1->4 at tile 2 etc
        for (var i = iStart; i < height; i += TileHeight)
        {
            for (var j = jStart; j < width; j += TileWidth)
            {
                //this should already be sorted
                for (var k = 0; k < Layers.Count; k++)
                {
                    //TODO move this math into a separate method given we do it in a couple of spots
                    var tileNumber = Layers[k].LayerData[j / TileHeight + i / TileWidth * Width];

                    //transparent, doesn't need to actually be drawn
                    if (tileNumber == 0)
                        continue;

                    //todo I don't know if this caching really helps, but I'll keep it for now, it can't hurt
                    Tile tile;
                    if (!_tileCache.TryGetValue(tileNumber, out var value))
                    {
                        tile = GetTile(tileNumber);
                        _tileCache[tileNumber] = tile;
                    }
                    else
                        tile = value;

                    if (tile is AnimatedTile animationTile)
                    {
                        animationTile.Play(); //maybe eventually we'd want to not autoplay in all scenarios but for now
                        animationTile.Draw(spriteBatch, new Vector2(j, i), k);
                    }
                    else
                        tile.Draw(spriteBatch, new Vector2(j, i), k);
                }

                //after drawing every layer for this tile, we can draw the next one
            }
        }
    }

    private Tile GetTile(int tileNumber)
    {
        //return the last tileset whose firstgid is <= our tilenumber
        //ie if our tileNumber is 40 and the first gid of this tileset is 35 but the next tileset's first gid is 50
        //then we want the one with firsgid=35, as this will contain our tile
        Tileset tileset = null;
        var gidMax = -1;
        foreach (var ts in Tilesets)
        {
            if (ts.Firstgid <= tileNumber && ts.Firstgid > gidMax)
            {
                tileset = ts;
                gidMax = ts.Firstgid;
            }
        }

        if (tileset == null)
            throw new Exception("Unable to find tile for tileNumber:" + tileNumber);

        return tileset.Tiles[tileNumber - tileset.Firstgid]; //offset it by the firstgid as tileNumber is absolute
    }

    /// <summary>Get Tiles that an entity is currently colliding with/standing on</summary>
    /// <param name="rectangle">Entity rectangle collisionbox</param>
    /// <returns>returns set of Vector2 coords representing position of tile on an X Y map grid. Multiply by
    /// TileWidth and TileHeight to get actual world position of tile's top left corner</returns>
    public HashSet<Vector2> GetTiles(Rectangle rectangle)
    {
        HashSet<Vector2> tiles = new();

        //say tiles are 16 pixels wide
        //tile 1 is 0->15, tile 2 is 16 -> 31, tile 3 is 32 -> 47
        //let's say our collision box has X=14 with a width of 20 (so right=34)
        //we'd want to capture x=14, x=34, and some point between x=16->31

        var leftX = rectangle.X / TileWidth;
        var rightX = rectangle.Right / TileWidth;

        var topY = rectangle.Y / TileHeight;
        var bottomY = rectangle.Bottom / TileHeight;

        for (var y = topY; y <= bottomY; y++)
        {
            for (var x = leftX; x <= rightX; x++)
            {
                tiles.Add(new Vector2(x, y));
            }
        }

        return tiles;
    }

    /// <summary>
    /// Converts tile positions from map grid space to world space, and returns collision boxes in world space
    /// </summary>
    /// <param name="tilePositions">Set of tile positions in map grid space</param>
    /// <param name="layer">Map layer</param>
    /// <returns>Returns tile collision rectangles in world space to check against entity collision boxes</returns>
    public HashSet<Rectangle> GetTileCollisions(HashSet<Vector2> tilePositions)
    {
        //the input here is a set of Tiles defined as their x,y coordinates on the grid
        //where 0,0 represents the tilewidth*tileheight sized tile in the top left corner
        //the x and y should always represent the top left corner of that tile, where we start the draw from

        HashSet<Rectangle> rectangles = new();
        foreach (var tilePos in tilePositions)
        {
            var tileIndex = (int)Math.Floor(tilePos.X) + (int)Math.Floor(tilePos.Y) * Width;

            //get collisions on ALL layers
            foreach (var layer in Layers)
            {
                //the number of that tile in our raw csv
                var tileNumber = layer.LayerData[tileIndex];

                //0 meaning it was an empty air tile
                var tile = tileNumber == 0 ? null : GetTile(tileNumber);

                //if the tile is null or it doesn't contain a valid collisionbox...
                if (tile == null || tile.CollisionBox.IsEmpty)
                    continue;

                //we take the top left of the tile (which should be our tilePos),
                //and apply the collisionbox info to get a world position rectangle of the collision
                //this is because the tile object itself doesn't have it's world position, that's handled by the Map.Draw()
                var adjTileCollisionRect = new Rectangle((int)Math.Floor(tilePos.X) * TileWidth + tile.CollisionBox.X,
                    (int)Math.Floor(tilePos.Y) * TileHeight + tile.CollisionBox.Y,
                    tile.CollisionBox.Width, tile.CollisionBox.Height);

                rectangles.Add(adjTileCollisionRect);
            }
        }

        return rectangles;
    }
}
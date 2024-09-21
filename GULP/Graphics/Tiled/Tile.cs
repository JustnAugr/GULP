using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Tiled;

public class Tile
{
    public Texture2D Texture { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int Id { get; }
    public Rectangle CollisionBox { get; }

    public Tile(Texture2D texture, int x, int y, int width, int height, int id, Rectangle collisionBox)
    {
        Id = id;
        Texture = texture;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        CollisionBox = collisionBox;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, float layerDepth)
    {
        //the layerDepth here takes into account the lowest Y axis of the tile (it's bottom), as well as the layerDepth
        //- this means that a tile at layer 1 (the same layer as the player) will be drawn in front of the player if the
        //  tile is "lower" on the screen -> this allows the the player to go "behind" a fence
        //- anything on layer 2 should be drawn above the player due to layerDepth scalar
        //- SCREEN_Y_RESOLUTION is used to compress the layerDepth value to a smaller scale float
        spriteBatch.Draw(Texture, position, new Rectangle(X, Y, Width, Height), Color.White, 0, new Vector2(0, 0), 1,
            SpriteEffects.None, MathHelper.Clamp((position.Y + Height)/ (GULPGame.SCREEN_Y_RESOLUTION * 16)*layerDepth, 0f, 1f)); //TODO put this 16 into a constant,it's the tileHeight
    }
}
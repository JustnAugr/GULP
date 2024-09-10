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

    public Tile(Texture2D texture, int x, int y, int width, int height)
    {
        Texture = texture;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        spriteBatch.Draw(Texture, position, new Rectangle(X, Y, Width, Height), Color.White);
    }
}
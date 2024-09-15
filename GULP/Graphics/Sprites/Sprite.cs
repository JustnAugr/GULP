using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Sprites;

public class Sprite
{
    private readonly SpriteEffects _spriteEffects;
    public Texture2D Texture { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }

    public Sprite(Texture2D texture, int x, int y, int width, int height,
        SpriteEffects spriteEffects = SpriteEffects.None)
    {
        Texture = texture;
        X = x;
        Y = y;
        Height = height;
        Width = width;
        _spriteEffects = spriteEffects;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        //take the texture, extract the rectangle starting at X,Y with this height and width
        //draw it in our world at position
        //the layerDepth that we're drawing at uses the position of the Sprite at its lowest point (feet)
        //the lower the feet, the higher the precedence of it being drawn on top
        //this lets us stand behind something drawn in lower on the Y axis - in the "foreground"
        //we assume the sprite is always drawn on tile layer 1
        spriteBatch.Draw(Texture, position, new Rectangle(X, Y, Width, Height), Color.White, 0, new Vector2(0, 0), 1,
            _spriteEffects, (position.Y + Height)/GULPGame.WINDOW_HEIGHT);
    }
}
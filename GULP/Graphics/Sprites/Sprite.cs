using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Sprites;

public class Sprite
{
    private const int LAYER_DEPTH = 2;
    private readonly SpriteEffects _spriteEffects;

    public Rectangle Rect { get; }
    public Texture2D Texture { get; }
    public int X { get; }
    public int Y { get; }
    public int Height { get; }
    public int Width { get; }

    public Sprite(Texture2D texture, int x, int y, int width, int height,
        SpriteEffects spriteEffects = SpriteEffects.None)
    {
        Texture = texture;
        X = x;
        Y = y;
        Height = height;
        Width = width;
        _spriteEffects = spriteEffects;
        
        Rect = new Rectangle(X, Y, Width, Height);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        //take the texture, extract the rectangle starting at X,Y with this height and width
        //draw it in our world at position
        //the layerDepth that we're drawing at uses the position of the Sprite at its lowest point (feet)
        //the lower the feet, the higher the precedence of it being drawn on top
        //this lets us stand behind something drawn in lower on the Y axis - in the "foreground"
        //we assume the sprite is always drawn on tile layer 1

        spriteBatch.Draw(Texture, position, Rect, Color.White, 0, new Vector2(0, 0), 1,
            _spriteEffects,
            MathHelper.Clamp((position.Y + Height) / (GULPGame.SCREEN_Y_RESOLUTION * 16) * LAYER_DEPTH, 0f, 1f)); //TODO put this 16 into a constant,it's the tileHeight
        //we have to clmap this as when an entity gets down towards the bottom it can become undrawn as this division gets slightly over 1
    }
}
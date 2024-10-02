using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Sprites;

public class NinePatch
{
    private readonly int _patchWidth;
    private readonly int _patchHeight;
    private readonly int _width;
    private readonly int _height;

    private Rectangle _topLeft;
    private Rectangle _topMid;
    private Rectangle _topRight;
    private Rectangle _midLeft;
    private Rectangle _midMid;
    private Rectangle _midRight;
    private Rectangle _botLeft;
    private Rectangle _botMid;
    private Rectangle _botRight;

    public Texture2D Texture { get; }

    public NinePatch(Texture2D texture, int x, int y, int buffer, int patchWidth, int patchHeight, int width,
        int height)
    {
        Texture = texture;

        _patchWidth = patchWidth;
        _patchHeight = patchHeight;
        _width = width;
        _height = height;

        _topLeft = new Rectangle(x, y, patchWidth, patchHeight);
        _topMid = new Rectangle(_topLeft.X + patchWidth + buffer, y, patchWidth, patchHeight);
        _topRight = new Rectangle(_topMid.X + patchWidth + buffer, y, patchWidth, patchHeight);

        _midLeft = new Rectangle(x, _topLeft.Y + patchHeight + buffer, patchWidth, patchHeight);
        _midMid = new Rectangle(_midLeft.X + patchWidth + buffer, _midLeft.Y, patchWidth, patchHeight);
        _midRight = new Rectangle(_midMid.X + patchWidth + buffer, _midLeft.Y, patchWidth, patchHeight);

        _botLeft = new Rectangle(x, _midLeft.Y + patchHeight + buffer, patchWidth, patchHeight);
        _botMid = new Rectangle(_botLeft.X + patchWidth + buffer, _botLeft.Y, patchWidth, patchHeight);
        _botRight = new Rectangle(_botMid.X + patchWidth + buffer, _botLeft.Y, patchWidth, patchHeight);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        var scaleX = _width / (float)_patchWidth;
        var scaleY = _height / (float)_patchHeight;
        
        //top layer
        var topX = position.X;
        var topY = position.Y;
        DrawSlice(spriteBatch, _topLeft, new Vector2(topX, topY));
        topX += _patchWidth;
        for (int i = 0; i < scaleX; i++)
        {
            DrawSlice(spriteBatch, _topMid, new Vector2(topX, topY));
            topX += _patchWidth;
        }

        DrawSlice(spriteBatch, _topRight, new Vector2(topX, topY));

        //middle layers
        var midX = position.X;
        var midY = position.Y + _patchHeight;
        for (int i = 0; i < scaleY; i++)
        {
            DrawSlice(spriteBatch, _midLeft, new Vector2(midX, midY));
            midX += _patchWidth;
            for (int j = 0; j < scaleX; j++)
            {
                DrawSlice(spriteBatch, _midMid, new Vector2(midX, midY));
                midX += _patchWidth;
            }
            
            DrawSlice(spriteBatch, _midRight, new Vector2(midX, midY));
            midY += _patchHeight;
            midX = position.X;
        }

        //bottom layer
        var botX = position.X;
        var botY = midY;
        DrawSlice(spriteBatch, _botLeft, new Vector2(botX, botY));
        botX += _patchWidth;
        for (int i = 0; i < scaleX; i++)
        {
            DrawSlice(spriteBatch, _botMid, new Vector2(botX, botY));
            botX += _patchWidth;
        }

        DrawSlice(spriteBatch, _botRight, new Vector2(botX, botY));
    }

    private void DrawSlice(SpriteBatch spriteBatch, Rectangle slice, Vector2 position)
    {
        spriteBatch.Draw(Texture, position, slice, Color.White, 0, new Vector2(0, 0), 1,
            SpriteEffects.None, 1f);
    }
}
using Microsoft.Xna.Framework;

namespace GULP.Graphics.Sprites;

public enum SpriteDirection
{
    Down,
    Up,
    Left,
    Right
}
public static class SpriteDirectionExtensions
{
    public static Vector2 ToVectorDirection(this SpriteDirection direction)
    {
        switch (direction)
        {
            case SpriteDirection.Left:
                return new Vector2(-1, 0);
            case SpriteDirection.Right:
                return new Vector2(1, 0);
            case SpriteDirection.Up:
                return new Vector2(0, -1);
            case SpriteDirection.Down:
            default:
                return new Vector2(1, 0);
        }
    }
    public static SpriteDirection ToSpriteAnimation(this Vector2 direction)
    {
        var x = direction.X;
        var y = direction.Y;

        //if we're moving left or right (non-diag or diag), we should face that way
        //else just face up or down
        if (x < 0)
            return SpriteDirection.Left;
        if (x > 0)
            return SpriteDirection.Right;
        if (y < 0)
            return SpriteDirection.Up;
        //if (y > 0)
        return SpriteDirection.Down;
    }
}
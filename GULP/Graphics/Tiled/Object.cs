using System;
using Microsoft.Xna.Framework;

namespace GULP.Graphics.Tiled;

public class Object
{
    public Rectangle Rect { get; }
    public ObjectType Type { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public Object(string type, int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        
        Rect = new Rectangle(x, y, width, height);
        Type = Enum.Parse<ObjectType>(type, true);
    }
}
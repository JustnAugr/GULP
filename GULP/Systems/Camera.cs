using GULP.Entities;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Systems;

public class Camera
{
    private readonly Player _player;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Map _map;

    public float PositionX { get; private set; }
    public float PositionY { get; private set; }

    public Camera(Player player, GraphicsDevice graphicsDevice, Map map)
    {
        _player = player; //TODO should we have the camera have a follow(entity) method? more generic
        _graphicsDevice = graphicsDevice;
        _map = map;
    }

    public void Update(GameTime gameTime)
    {
        PositionX = _player.Position.X;
        PositionY = _player.Position.Y;
    }

    public Matrix GetTransformationMatrix()
    {
        //todo this obviously needs cleaning in terms of zoom, constants etc
        
        //additional zoom factor to be toggleable in settings separate from resolution
        var zoom = 1.5f;

        //we scale our viewport (the actual monitor size of the person playing) by the resolution they've chosen so that it fills their window
        //this can be taken to be a "scaling" viewport adaptor, rather than say a pillarbox adaptor that would just create a box around the game
        //if the player chose 720p when using a 2k monitor
        var scaleX = (float)_graphicsDevice.Viewport.Width / GULPGame.WINDOW_WIDTH;
        var scaleY = (float)_graphicsDevice.Viewport.Height / GULPGame.WINDOW_HEIGHT;

        var dx = MathHelper.Clamp(PositionX, //we offset the camera by the position of our player
            0 + GULPGame.WINDOW_WIDTH/(2f * zoom), //clamp the leftmost position by 0 + half the size of the current worldScreen (as determined by their resolution)
            _map.PixelWidth - GULPGame.WINDOW_WIDTH/(2f * zoom)); //clamp rightmost position by the size of the map - size of the current worldScreen (as determined by their resolution)
        
        var dy = MathHelper.Clamp(PositionY,
            0 + GULPGame.WINDOW_HEIGHT / (2f * zoom),
            _map.PixelHeight - GULPGame.WINDOW_HEIGHT/(2f * zoom));

        return Matrix.CreateTranslation(-dx, -dy, 0)
               * Matrix.CreateScale(scaleX, scaleY, 1)
               * Matrix.CreateScale(zoom, zoom, 1)
               * Matrix.CreateTranslation(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f, 0); //make sure we translate to their viewport
    }
}
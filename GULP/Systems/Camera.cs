using GULP.Entities;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Systems;

public class Camera
{
    private readonly IEntity _entityToFollow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Map _map;
    private float _zoom = 2.5f;

    public float PositionX { get; private set; }
    public float PositionY { get; private set; }
    
    public float Left => -(GetTransformationMatrix().Translation.X / Zoom);
    public float Right => Left + GULPGame.WINDOW_WIDTH / Zoom;
    
    public float Top => -(GetTransformationMatrix().Translation.Y / Zoom);
    public float Bottom => Top + GULPGame.WINDOW_HEIGHT / Zoom;

    public float Zoom //additional zoom factor separate from resolution
    {
        get => _zoom;
        set => _zoom = MathHelper.Clamp(value, 1f, 5f);
    }

    public Camera(IEntity entity, GraphicsDevice graphicsDevice, Map map)
    {
        _entityToFollow = entity;
        _graphicsDevice = graphicsDevice;
        _map = map;
    }

    public void Update(GameTime gameTime)
    {
        PositionX = _entityToFollow.Position.X;
        PositionY = _entityToFollow.Position.Y;
    }

    public Matrix GetTransformationMatrix()
    {
        //we scale our viewport (the actual monitor size of the person playing) by the resolution they've chosen so that it fills their window
        //this can be taken to be a "scaling" viewport adaptor, rather than say a pillarbox adaptor that would just create a box around the game
        //if the player chose 720p when using a 2k monitor
        var scaleX = (float)_graphicsDevice.Viewport.Width / GULPGame.WINDOW_WIDTH;
        var scaleY = (float)_graphicsDevice.Viewport.Height / GULPGame.WINDOW_HEIGHT;

        var dx = MathHelper.Clamp(PositionX, //we offset the camera by the position of our player
            0 + GULPGame.WINDOW_WIDTH /
            (2f * Zoom), //clamp the leftmost position by 0 + half the size of the current worldScreen (as determined by their resolution)
            _map.PixelWidth -
            GULPGame.WINDOW_WIDTH /
            (2f * Zoom)); //clamp rightmost position by the size of the map - size of the current worldScreen (as determined by their resolution)

        var dy = MathHelper.Clamp(PositionY,
            0 + GULPGame.WINDOW_HEIGHT / (2f * Zoom),
            _map.PixelHeight - GULPGame.WINDOW_HEIGHT / (2f * Zoom));

        //why are dx and dy negative? the camera controls the draw of everything on the screen, relative to the player being the center
        //consider the camera as a non-moving hole and everything else can be moved under it
        //if the player walks 100 to the right (+100), we pass -100 (100 to the left) to our camera because the camera itself doesn't move,
        //the world moves -100 to the left so that the player is still centered under our camera
        return Matrix.CreateTranslation(-dx, -dy, 0)
               * Matrix.CreateScale(scaleX, scaleY, 1)
               * Matrix.CreateScale(Zoom, Zoom, 1)
               * Matrix.CreateTranslation(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f,
                   0); //make sure we translate to their viewport
    }
}
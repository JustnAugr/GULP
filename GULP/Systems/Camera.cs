using GULP.Entities;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Systems;

public class Camera
{
    private const int STARTING_ZOOM_VALUE = 3;
    
    private readonly IEntity _entityToFollow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Map _map;
    private int _zoom = STARTING_ZOOM_VALUE;

    public float PositionX { get; private set; }
    public float PositionY { get; private set; }

    //we scale our viewport (the actual monitor size of the person playing) by the resolution they've chosen so that it fills their window
    //this can be taken to be a "scaling" viewport adaptor, rather than say a pillarbox adaptor that would just create a box around the game
    //if the player chose 720p when using a 2k monitor
    public float ScaleX => (float)_graphicsDevice.Viewport.Width / GULPGame.SCREEN_X_RESOLUTION;
    public float ScaleY => (float)_graphicsDevice.Viewport.Height / GULPGame.SCREEN_Y_RESOLUTION;

    //provide the bounds of the actual drawable screen, so that we can easily cull drawing anything outside of this
    public float Left => -(GetTransformationMatrix().Translation.X / GetTransformationMatrix().Right.X);
    public float Right => Left + _graphicsDevice.Viewport.Width / GetTransformationMatrix().Right.X;

    public float Top => -(GetTransformationMatrix().Translation.Y / GetTransformationMatrix().Up.Y);
    public float Bottom => Top + _graphicsDevice.Viewport.Height / GetTransformationMatrix().Up.Y;

    public int Zoom //additional zoom factor separate from resolution
    {
        get => _zoom;
        set => _zoom = MathHelper.Clamp(value, 1, 5);
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
        //the zoom here needs consider the scale
        //the matrix math will create a scale by doing Zoom * Scale, and if this isn't an integer then things will flicker as the camera moves
        //to control this, we make sure that the Zoom factor is evenly divisible by the scale so that our matrix math always gives us integer values
        var zoomX = Zoom / ScaleX;
        var zoomY = Zoom / ScaleY;

        var dx = MathHelper.Clamp(PositionX, //we offset the camera by the position of our player
            0 + GULPGame.SCREEN_X_RESOLUTION /
            (2f * zoomX), //clamp the leftmost position by 0 + half the size of the current worldScreen (as determined by their resolution)
            _map.PixelWidth -
            GULPGame.SCREEN_X_RESOLUTION /
            (2f * zoomX)); //clamp rightmost position by the size of the map - size of the current worldScreen (as determined by their resolution)

        var dy = MathHelper.Clamp(PositionY,
            0 + GULPGame.SCREEN_Y_RESOLUTION / (2f * zoomY),
            _map.PixelHeight - GULPGame.SCREEN_Y_RESOLUTION / (2f * zoomY));

        //why are dx and dy negative? the camera controls the draw of everything on the screen, relative to the player being the center
        //consider the camera as a non-moving hole and everything else can be moved under it
        //if the player walks 100 to the right (+100), we pass -100 (100 to the left) to our camera because the camera itself doesn't move,
        //the world moves -100 to the left so that the player is still centered under our camera
        return Matrix.CreateTranslation(-dx, -dy, 0)
               * Matrix.CreateScale(ScaleX, ScaleY, 1)
               * Matrix.CreateScale(zoomX, zoomY, 1)
               * Matrix.CreateTranslation(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f,
                   0); //make sure we translate to their viewport
    }
}
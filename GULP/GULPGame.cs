using System;
using System.Diagnostics;
using System.IO;
using GULP.Entities;
using GULP.Graphics.Tiled;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GULP;

public class GULPGame : Game
{
    //TODO both of these should be changeable in the settings
    //this is the actual resolution that should be set via a picker in the settings
    //allowing the player to choose between 720p, 1080p, 1440p, etc
    public const int SCREEN_X_RESOLUTION = 2560;
    public const int SCREEN_Y_RESOLUTION = 1440;
    //this is the actual window size, regardless of the resolution chosen
    public const int SCREEN_WIDTH = 1920;
    public const int SCREEN_HEIGHT = 1080;

    private const string PLAYER_TEXTURE_ASSET_NAME = "Sprites/player";
    private const string SLIME_TEXTURE_ASSET_NAME = "Sprites/slime";
    private const string TILED_PREFIX_ASSET_NAME = "Tiled";
    private const string MAP_FILE_ASSET_NAME = "map_01.tmx";

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputController _inputController;

    //Textures
    private Texture2D _playerTexture;

    private Map _map;

    //Entities
    private EntityManager _entityManager;
    private Player _player;
    private Camera _camera;

    public GULPGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
        _graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;

        //_graphics.ToggleFullScreen();
        //_graphics.HardwareModeSwitch = false; //this makes it borderless fullscreen

        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //Map
        _map = Map.Load(Path.Combine(Content.RootDirectory, TILED_PREFIX_ASSET_NAME, MAP_FILE_ASSET_NAME), Content);

        //Entities
        _entityManager = new EntityManager(_map);

        _playerTexture = Content.Load<Texture2D>(PLAYER_TEXTURE_ASSET_NAME);
        _player = new Player(_playerTexture, new Vector2(15 * 16, 15 * 16), _map,
            _entityManager); //todo location from map object (non-collision object)

        //TODO should the map and camera be globals? especially considering we're using them for things like draw culling...
        _camera = new Camera(_player, GraphicsDevice, _map);
        _map.Camera = _camera;

        var slimeTexture = Content.Load<Texture2D>(SLIME_TEXTURE_ASSET_NAME);
        var slime = new Slime(slimeTexture, new Vector2(40, 40), _map, _camera, _entityManager);

        _entityManager.AddEntity(slime);
        _entityManager.AddEntity(_player);

        _inputController = new InputController(_player, _camera);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _inputController.ProcessInputs(gameTime);
        _camera.Update(gameTime);
        _entityManager.Update(gameTime);
        _map.Update(gameTime);
        
        //probably should be in a separate DebugHelper class like the entity collisionbox logic
        var frameRate = (int)Math.Ceiling(1 / (float)gameTime.ElapsedGameTime.TotalSeconds);
        Debug.WriteLine("FPS = " + frameRate);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetTransformationMatrix(),
            sortMode: SpriteSortMode.FrontToBack);

        _map.Draw(_spriteBatch);
        _entityManager.Draw(_spriteBatch, gameTime);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
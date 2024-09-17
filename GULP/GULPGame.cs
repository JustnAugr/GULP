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
    //this is the actual resolution that should be set via a picker in the settings
    //allowing the player to choose between 720p, 1080p, 1440p, etc
    public const int WINDOW_WIDTH = 1920;
    public const int WINDOW_HEIGHT = 1080;

    private const string PLAYER_TEXTURE_ASSET_NAME = "Sprites/player";
    private const string TILED_PREFIX_ASSET_NAME = "Tiled";
    private const string MAP_FILE_ASSET_NAME = "map.tmx";

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

        //TODO this should be moved into some settings menu and allow the player to change
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

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
        _entityManager = new EntityManager();
        _playerTexture = Content.Load<Texture2D>(PLAYER_TEXTURE_ASSET_NAME);
        _player = new Player(_playerTexture, new Vector2(900, 540), _map); //TODO should the map be a global?
        _entityManager.AddEntity(_player);
        
        _camera = new Camera(_player, GraphicsDevice, _map);
        _inputController = new InputController(_player);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _inputController.ProcessInputs(gameTime);
        _camera.Update(gameTime);
        _entityManager.Update(gameTime);

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
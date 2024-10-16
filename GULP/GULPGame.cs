using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GULP.Entities;
using GULP.Graphics.Interface;
using GULP.Graphics.Tiled;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GULP;

public class GULPGame : Game
{
    private const string PLAYER_TEXTURE_ASSET_NAME = "Sprites/player";
    private const string SLIME_TEXTURE_ASSET_NAME = "Sprites/slime";
    private const string UI_TEXTURE_ASSET_NAME = "Interface/ui";
    private const string TILED_PREFIX_ASSET_NAME = "Tiled";
    private const string MAP_FILE_ASSET_NAME = "map_01.tmx";

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private EntityManager _entityManager;
    private Camera _camera;
    private Map _map;
    private InputController _inputController;
    private StartScreen _startScreen;

    public GULPGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        #region Core

        var gameSettings = new GameSettings(_graphics);
        GameContext.AddComponent(gameSettings);

        _inputController = new InputController();
        GameContext.AddComponent(_inputController);

        _camera = new Camera(GraphicsDevice);
        GameContext.AddComponent(_camera);

        #endregion

        #region Map

        _map = Map.Load(Path.Combine(Content.RootDirectory, TILED_PREFIX_ASSET_NAME, MAP_FILE_ASSET_NAME), Content);

        var playerSpawnObjects = _map.Objects.Where(obj => obj.Type == ObjectType.PlayerSpawn).ToArray();
        if (playerSpawnObjects.Length != 1)
            throw new ArgumentOutOfRangeException(nameof(playerSpawnObjects), "Must have 1 PlayerSpawn object per map");
        var playerSpawnLocation = playerSpawnObjects[0];
        //todo load slimeSpwan locations here too and pass to enemy manager?

        GameContext.AddComponent(_map);

        #endregion

        #region Entities

        _entityManager = new EntityManager();

        var playerTexture = Content.Load<Texture2D>(PLAYER_TEXTURE_ASSET_NAME);
        var player = new Player(playerTexture, new Vector2(playerSpawnLocation.X, playerSpawnLocation.Y));
        GameContext.Player = player; //set it on the context for global use
        _camera.Follow(player); //camera should follow the player

        var slimeTexture = Content.Load<Texture2D>(SLIME_TEXTURE_ASSET_NAME);
        var enemyManager = new EnemyManager(slimeTexture);

        _entityManager.AddEntity(player);
        _entityManager.AddEntity(enemyManager);
        GameContext.AddComponent(_entityManager);

        #endregion

        #region Interfaces

        var startScreenTexture = Content.Load<Texture2D>(UI_TEXTURE_ASSET_NAME);
        _startScreen = new StartScreen(startScreenTexture);
        _startScreen.Open();

        #endregion
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _inputController.ProcessInputs(gameTime);
        _camera.Update(gameTime);
        _startScreen.Update(gameTime);

        //todo this should be "IsGamePaused" and the menu func needs to move an interface manager later
        if (!GameContext.IsMenuOpen)
        {
            _entityManager.Update(gameTime);
            _map.Update(gameTime);
        }

        //probably should be in a separate DebugHelper class like the entity collisionbox logic
        var frameRate = (int)Math.Ceiling(1 / (float)gameTime.ElapsedGameTime.TotalSeconds);
        //Debug.WriteLine("FPS = " + frameRate);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetTransformationMatrix(),
            sortMode: SpriteSortMode.FrontToBack);

        _startScreen.Draw(_spriteBatch);
        _map.Draw(_spriteBatch);
        _entityManager.Draw(_spriteBatch, gameTime);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
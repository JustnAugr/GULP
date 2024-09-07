using System.Diagnostics;
using System.IO;
using System.Xml;
using GULP.Entities;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GULP;

public class GULPGame : Game
{
    private const int WINDOW_WIDTH = 1920;
    private const int WINDOW_HEIGHT = 1080;
    private const float WINDOW_SCALE_FACTOR = 2.5f;
    private const string PLAYER_TEXTURE_ASSET_NAME = "Sprites/player";

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputController _inputController;

    //Textures
    private Texture2D _playerTexture;

    private Player _player;

    public GULPGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //Texture Loading
        _playerTexture = Content.Load<Texture2D>(PLAYER_TEXTURE_ASSET_NAME);

        //_player = new Player(_playerTexture, new Vector2(900 / WINDOW_SCALE_FACTOR, 540 / WINDOW_SCALE_FACTOR));
        //_inputController = new InputController(_player);

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // _inputController.ProcessInputs(gameTime);
        // _player.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.ForestGreen);

        var transformMatrix = Matrix.Identity * Matrix.CreateScale(WINDOW_SCALE_FACTOR, WINDOW_SCALE_FACTOR, 1);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix);

        //_player.Draw(_spriteBatch);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
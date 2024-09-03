using GULP.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GULP;

public class GULPGame : Game
{
    private const int WINDOW_WIDTH = 1920;
    private const int WINDOW_HEIGHT = 1080;
    private const float WINDOW_SCALE_FACTOR = 3.5f;
    private string PLAYER_TEXTURE_ASSET_NAME;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //Textures
    private Texture2D _playerTexture;
    public SpriteAnimation _playerIdleDownAnimation;
    private SpriteAnimation _playerIdleRightAnimation;
    private SpriteAnimation _playerIdleLeftAnimation;
    private SpriteAnimation _playerIdleUpAnimation;

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
        PLAYER_TEXTURE_ASSET_NAME = "player";
        _playerTexture = Content.Load<Texture2D>(PLAYER_TEXTURE_ASSET_NAME);


        var idleFrameDuration = 1 / 4f;

        _playerIdleDownAnimation = new SpriteAnimation();
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 18, 22, 13, 21), idleFrameDuration);
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 66, 22, 13, 21), idleFrameDuration);
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 114, 22, 13, 21), idleFrameDuration);
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 162, 23, 13, 20), idleFrameDuration);
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 210, 23, 13, 20), idleFrameDuration);
        _playerIdleDownAnimation.AddFrame(new Sprite(_playerTexture, 258, 23, 13, 20), idleFrameDuration);
        _playerIdleDownAnimation.Play();

        _playerIdleRightAnimation = new SpriteAnimation();
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 17, 70, 15, 21), idleFrameDuration);
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 65, 70, 15, 21), idleFrameDuration);
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 113, 70, 15, 21), idleFrameDuration);
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 161, 71, 15, 20), idleFrameDuration);
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 209, 71, 15, 20), idleFrameDuration);
        _playerIdleRightAnimation.AddFrame(new Sprite(_playerTexture, 257, 71, 15, 20), idleFrameDuration);
        _playerIdleRightAnimation.Play();

        _playerIdleLeftAnimation = new SpriteAnimation();
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 17, 70, 15, 21, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 65, 70, 15, 21, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 113, 70, 15, 21, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 161, 71, 15, 20, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 209, 71, 15, 20, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.AddFrame(new Sprite(_playerTexture, 257, 71, 15, 20, SpriteEffects.FlipHorizontally),
            idleFrameDuration);
        _playerIdleLeftAnimation.Play();
        
        _playerIdleUpAnimation = new SpriteAnimation();
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 18, 118, 13, 21), idleFrameDuration);
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 66, 118, 13, 21), idleFrameDuration);
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 114, 118, 13, 21), idleFrameDuration);
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 162, 119, 13, 20), idleFrameDuration);
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 210, 119, 13, 20), idleFrameDuration);
        _playerIdleUpAnimation.AddFrame(new Sprite(_playerTexture, 258, 119, 13, 20), idleFrameDuration);
        _playerIdleUpAnimation.Play();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        _playerIdleDownAnimation.Update(gameTime);
        _playerIdleRightAnimation.Update(gameTime);
        _playerIdleLeftAnimation.Update(gameTime);
        _playerIdleUpAnimation.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Matrix transformMatrix = Matrix.Identity * Matrix.CreateScale(WINDOW_SCALE_FACTOR, WINDOW_SCALE_FACTOR, 1);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix);

        _playerIdleDownAnimation.Draw(_spriteBatch,
            new Vector2(700 / WINDOW_SCALE_FACTOR, 400 / WINDOW_SCALE_FACTOR));

        _playerIdleRightAnimation.Draw(_spriteBatch,
            new Vector2(800 / WINDOW_SCALE_FACTOR, 400 / WINDOW_SCALE_FACTOR));

        _playerIdleLeftAnimation.Draw(_spriteBatch,
            new Vector2(900 / WINDOW_SCALE_FACTOR, 400 / WINDOW_SCALE_FACTOR));

        _playerIdleUpAnimation.Draw(_spriteBatch,
            new Vector2(1000 / WINDOW_SCALE_FACTOR, 400 / WINDOW_SCALE_FACTOR));

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
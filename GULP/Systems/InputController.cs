using System.Diagnostics;
using GULP.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GULP.Systems;

public class InputController
{
    private const int CAMERA_ZOOM_STEP = 1;

    private readonly Player _player;
    private readonly Camera _camera;
    private KeyboardState _previousKeyboardState;

    public InputController(Player player, Camera camera)
    {
        _player = player;
        _camera = camera;
    }

    public void ProcessInputs(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        ProcessCameraInputs(keyboardState);
        ProcessPlayerInputs(keyboardState, gameTime);

        _previousKeyboardState = keyboardState;
    }

    private void ProcessCameraInputs(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.OemPlus) && !_previousKeyboardState.IsKeyDown(Keys.OemPlus))
        {
            _camera.Zoom += CAMERA_ZOOM_STEP;
        }
        else if (keyboardState.IsKeyDown(Keys.OemMinus) && !_previousKeyboardState.IsKeyDown(Keys.OemMinus))
        {
            _camera.Zoom -= CAMERA_ZOOM_STEP;
        }
    }

    private void ProcessPlayerInputs(KeyboardState keyboardState, GameTime gameTime)
    {
        var downPressed = keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S);
        var upPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W);
        var leftPressed = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A);
        var rightPressed = keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D);

        //create a directional vector based on which WASD keys are pressed
        float x = leftPressed ? -1 : rightPressed ? 1 : 0;
        float y = upPressed ? -1 : downPressed ? 1 : 0;

        Vector2 direction = new(x, y);

        if (keyboardState.IsKeyDown(Keys.Space) && !_player.IsAttacking)
        {
            if (!(downPressed || upPressed || leftPressed || rightPressed))
                _player.Attack(gameTime);
            else
                _player.Attack(direction, gameTime);
        }
        else if (downPressed || upPressed || leftPressed || rightPressed)
        {
            _player.Walk(direction, gameTime);
        }
        else
        {
            _player.Idle(gameTime);
        }
    }
}
﻿using System.Diagnostics;
using GULP.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GULP.Systems;

public class InputController
{
    private readonly Player _player;
    private KeyboardState _previousKeyboardState;

    public InputController(Player player)
    {
        _player = player;
    }

    public void ProcessInputs(GameTime gameTime)
    {
        var kbState = Keyboard.GetState();

        var downPressed = kbState.IsKeyDown(Keys.Down) || kbState.IsKeyDown(Keys.S);
        var upPressed = kbState.IsKeyDown(Keys.Up) || kbState.IsKeyDown(Keys.W);
        var leftPressed = kbState.IsKeyDown(Keys.Left) || kbState.IsKeyDown(Keys.A);
        var rightPressed = kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.D);

        float x = leftPressed ? -1 : rightPressed ? 1 : 0;
        float y = upPressed ? -1 : downPressed ? 1 : 0;

        if (kbState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
        {
            _player.Attack(x, y, gameTime);
        }
        else if (downPressed || upPressed || leftPressed || rightPressed)
        {
            _player.Walk(x, y, gameTime);
        }
        else
        {
            _player.Idle();
        }

        _previousKeyboardState = kbState;
    }
}
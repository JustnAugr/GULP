using System;
using System.Collections.Generic;
using GULP.Entities;
using GULP.Graphics.Sprites;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Interface;

public class StartScreen : IEntity
{
    private const int SLICE_WIDTH = 16;
    private const int SLICE_HEIGHT = 16;

    //todo dynamic based on viewport size and maybe resolution?
    private const int PAPER_NINEPATCH_WIDTH = 16 * 14;
    private const int PAPER_NINEPATCH_HEIGHT = 16 * 16;
    private const int BORDER_NINEPATCH_WIDTH = 16 * 16;
    private const int BORDER_NINEPATCH_HEIGHT = 16 * 18;

    private readonly Camera _camera;
    private readonly NPatch _paper;
    private readonly NPatch _border;
    private readonly NPatch _button;

    public Vector2 Position { get; set; }

    public StartScreen(Texture2D texture, Camera camera)
    {
        _camera = camera;

        _paper = new NPatch(texture, 216, 2, 2, 3, 3, SLICE_WIDTH, SLICE_HEIGHT, PAPER_NINEPATCH_WIDTH,
            PAPER_NINEPATCH_HEIGHT);
        _border = new NPatch(texture, 2, 134, 2, 4, 4, SLICE_WIDTH, SLICE_HEIGHT, BORDER_NINEPATCH_WIDTH,
            BORDER_NINEPATCH_HEIGHT);
    }

    public void Update(GameTime gameTime)
    {
        //draw it in the center of our screen/camera
        var posX = _camera.Left + (_camera.Right - _camera.Left) / 2f - BORDER_NINEPATCH_WIDTH / 2f;
        var posY = _camera.Top + (_camera.Bottom - _camera.Top) / 2f - BORDER_NINEPATCH_HEIGHT / 2f;

        Position = new Vector2(posX, posY);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _border.Draw(spriteBatch, new Vector2(Position.X, Position.Y));
        _paper.Draw(spriteBatch, new Vector2(Position.X + SLICE_WIDTH, Position.Y + SLICE_HEIGHT));
    }
}
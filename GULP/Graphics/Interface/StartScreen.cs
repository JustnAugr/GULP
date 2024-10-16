using System;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Interface;

public class StartScreen : IInterface
{
    private const int SLICE_WIDTH = 16;
    private const int SLICE_HEIGHT = 16;

    private readonly Texture2D _texture;

    private int _borderNinepatchWidth;
    private int _borderNinepatchHeight;

    private NPatch _paper;
    private NPatch _border;

    public Vector2 Position { get; set; }
    public bool IsOpen { get; private set; }

    public StartScreen(Texture2D texture)
    {
        _texture = texture;
        CreateNPatches();
    }

    private void CreateNPatches()
    {
        //get camera, get the Height and Width of the current view in slices
        GameContext.GetComponent(out Camera camera);
        var screenWidthSlices = camera.Width / SLICE_WIDTH;
        var screenHeightSlices = camera.Height / SLICE_HEIGHT;

        //create the border as 1/3 the screen width, 2/3 the screen height in slices, then convert to pixels
        _borderNinepatchWidth = (int)Math.Floor(screenWidthSlices * 1 / 3f) * SLICE_WIDTH;
        _borderNinepatchHeight = (int)Math.Floor(screenHeightSlices * 2 / 3f) * SLICE_HEIGHT;

        //needs to be "inside" the border, so adjusting for that
        var paperWidth = _borderNinepatchWidth - SLICE_WIDTH * 2;
        var paperHeight = _borderNinepatchHeight - SLICE_HEIGHT * 2;

        _paper = new NPatch(_texture, 216, 2, 2, 3, 3, SLICE_WIDTH, SLICE_HEIGHT, paperWidth,
            paperHeight);
        _border = new NPatch(_texture, 2, 134, 2, 4, 4, SLICE_WIDTH, SLICE_HEIGHT, _borderNinepatchWidth,
            _borderNinepatchHeight);
    }

    public bool Open()
    {
        IsOpen = true;
        GameContext.IsMenuOpen = true;
        GameContext.OpenMenu = this;

        //create new NPatches in case our screen resolution changed and the menu should now be a new size
        CreateNPatches();
        return true;
    }

    public bool Close()
    {
        IsOpen = false;
        GameContext.IsMenuOpen = false;
        GameContext.OpenMenu = null;
        return true;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsOpen)
            return;

        //draw it in the center of our screen/camera
        GameContext.GetComponent(out Camera camera);
        var posX = camera.Left + camera.Width / 2f - _borderNinepatchWidth / 2f;
        var posY = camera.Top + camera.Height / 2f - _borderNinepatchHeight / 2f;

        Position = new Vector2(posX, posY);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen)
            return;

        _border.Draw(spriteBatch, new Vector2(Position.X, Position.Y));
        _paper.Draw(spriteBatch, new Vector2(Position.X + SLICE_WIDTH, Position.Y + SLICE_HEIGHT));
    }
}
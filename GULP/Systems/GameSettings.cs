using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Systems;

public class GameSettings
{
    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    #region Resolution

    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }

    public bool IsBorderless { get; private set; } = true;
    public bool IsFullscreen { get; private set; } = false;

    #endregion

    public GameSettings(GraphicsDeviceManager graphicsDeviceManager)
    {
        _graphicsDeviceManager = graphicsDeviceManager;
        SetDefaultGraphicsOptions();
    }

    private void SetDefaultGraphicsOptions()
    {
        //default our resolution to the monitor's resolution
        ResolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        ResolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        //by default I want non-borderless fullscreen
        ToggleFullScreen(IsFullscreen, IsBorderless);
    }

    public List<Tuple<int, int>> GetResolutionOptions()
    {
        var result = new List<Tuple<int, int>>();
        foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
        {
            var width = mode.Width;
            var height = mode.Height;
            result.Add(new Tuple<int, int>(width, height));
        }

        return result;
    }

    public void ToggleFullScreen(bool fullScreen)
    {
        ToggleFullScreen(fullScreen, IsBorderless);
    }

    public void ToggleFullScreen(bool fullscreen, bool borderless)
    {
        if (!fullscreen)
        {
            _graphicsDeviceManager.IsFullScreen = false;
            //set our game window size to match our resolution
            _graphicsDeviceManager.PreferredBackBufferWidth = ResolutionWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = ResolutionHeight;
        }
        else
        {
            //set fullscreen and if it should hardware switch (no meaning borderless)
            _graphicsDeviceManager.IsFullScreen = true;
            _graphicsDeviceManager.HardwareModeSwitch = !borderless;

            //set our window size to be the size of the display that we're fullscreening into
            _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        //set our public bools for outside access
        IsFullscreen = fullscreen;
        IsBorderless = borderless;
        _graphicsDeviceManager.ApplyChanges();
    }
}
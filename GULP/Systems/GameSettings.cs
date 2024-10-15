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

    #endregion

    public GameSettings(GraphicsDeviceManager graphicsDeviceManager)
    {
        _graphicsDeviceManager = graphicsDeviceManager;

        ResolutionWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        ResolutionHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        SetDefaultGraphicsOptions();
    }

    private void SetDefaultGraphicsOptions()
    {
        //set our window size (when non-fullscreen) to match the Resolution Width and Height
        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        //set us as fullscreen
        _graphicsDeviceManager.IsFullScreen = true;
        _graphicsDeviceManager.ApplyChanges();
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

    public void ToggleFullScreen()
    {
        if (_graphicsDeviceManager.IsFullScreen)
        {
            //unfullscreen, set screen size to res size
            _graphicsDeviceManager.IsFullScreen = false;
            _graphicsDeviceManager.PreferredBackBufferWidth = ResolutionWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = ResolutionHeight;
        }
        else
        {
            _graphicsDeviceManager.IsFullScreen = true;
        }

        _graphicsDeviceManager.ApplyChanges();
    }

    public void ToggleBorderless()
    {
        _graphicsDeviceManager.HardwareModeSwitch = !_graphicsDeviceManager.HardwareModeSwitch;
        _graphicsDeviceManager.ApplyChanges();
    }
}
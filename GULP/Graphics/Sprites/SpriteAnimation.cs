using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Sprites;

public class SpriteAnimation
{
    private readonly float _defaultDuration;
    private readonly bool _shouldLoop;
    private readonly List<float> _spriteDurations = new();

    public readonly List<Sprite> Sprites = new();
    private bool NormalizeWidth { get; }
    private bool NormalizeHeight { get; }

    public float MinWidth { get; private set; } = float.MaxValue;

    public float MinHeight { get; private set; } = float.MaxValue;

    public int CurrentFrame
    {
        get
        {
            float durationSum = 0;
            for (int i = 0; i < _spriteDurations.Count; i++)
            {
                durationSum += _spriteDurations[i];

                if (durationSum >= PlaybackProgress)
                    return i;
            }

            return -1;
        }
    }

    public Sprite CurrentSprite => Sprites[CurrentFrame];
    public float PlaybackProgress { get; set; }
    public bool IsPlaying { get; private set; } = true;
    public float Duration { get; private set; }

    public SpriteAnimation(float defaultDuration = float.NaN, bool shouldLoop = true, bool normalizeHeight = true,
        bool normalizeWidth = false)
    {
        _defaultDuration = defaultDuration;
        _shouldLoop = shouldLoop;
        NormalizeHeight = normalizeHeight;
        NormalizeWidth = normalizeWidth;
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void Stop()
    {
        //we don't explicitly reset the progress here so that when Draw() gets called, we don't re-draw the first frame
        IsPlaying = false;
    }

    public void AddFrame(Sprite sprite)
    {
        if (float.IsNaN(_defaultDuration))
            throw new ArgumentException(
                "Must specify a default SpriteAnimation duration if not passing a duration for this frame!");

        MinHeight = Math.Min(MinHeight, sprite.Height);
        MinWidth = Math.Min(MinWidth, sprite.Width);
        Sprites.Add(sprite);
        _spriteDurations.Add(_defaultDuration);
        Duration += _defaultDuration;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying)
            return;

        PlaybackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (PlaybackProgress > Duration)
        {
            if (_shouldLoop)
                PlaybackProgress -= Duration;
            else
                Stop();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        Draw(spriteBatch, position, Color.White);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color tint)
    {
        //normalizing the heights because we draw from the top and if I don't we'd have weird effects when a sprites bobs
        //up and down
        var currentFrame = CurrentFrame;
        if (currentFrame >= 0 && currentFrame < Sprites.Count)
        {
            var currentSprite = Sprites[currentFrame];

            //we're normalizing to the smallest sprite to handle cases where were the walking animation involves a bit of an upward bob
            //we apply that upward, not downward so that the down position is the same as our idle position
            if (NormalizeHeight && currentSprite.Height > MinHeight)
                position = new Vector2(position.X, position.Y - (currentSprite.Height - MinHeight));
            //we also allow for optionally normalizing the width! this is especially important for cases where we've flipped the sprite horizontally
            if (NormalizeWidth && currentSprite.Width > MinWidth)
                position = new Vector2(position.X - (currentSprite.Width - MinWidth), position.Y);
        }

        //if we just finished an animation and are about to draw nothing, just redraw the last frame
        //then on next update we should get progressed to our next animation
        if (currentFrame == -1)
            currentFrame = Sprites.Count - 1;

        Sprites[currentFrame]?.Draw(spriteBatch, position, tint);
    }
}
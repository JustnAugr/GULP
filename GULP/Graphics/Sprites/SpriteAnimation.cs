using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Sprites;

public class SpriteAnimation
{
    private readonly float _defaultDuration;
    private readonly bool _shouldLoop;
    private readonly List<float> _spriteDurations = new();
    private float _minHeight = float.MaxValue;
    private float _minWidth = float.MaxValue;

    public readonly List<Sprite> Sprites = new();
    private bool NormalizeWidth { get; }
    private bool NormalizeHeight { get; }

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
    public float Duration => _spriteDurations.Sum();

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
        PlaybackProgress = 0;
        IsPlaying = false;
    }

    public void AddFrame(Sprite sprite)
    {
        if (float.IsNaN(_defaultDuration))
            throw new ArgumentException(
                "Must specify a default SpriteAnimation duration if not passing a duration for this frame!");

        _minHeight = Math.Min(_minHeight, sprite.Height);
        _minWidth = Math.Min(_minWidth, sprite.Width);
        Sprites.Add(sprite);
        _spriteDurations.Add(_defaultDuration);
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
        //normalizing the heights because we draw from the top and if I don't we'd have weird effects when a sprites bobs
        //up and down
        var currentFrame = CurrentFrame;
        if (currentFrame >= 0 && currentFrame < Sprites.Count)
        {
            var currentSprite = Sprites[currentFrame];

            //we're normalizing to the smallest sprite to handle cases where were the walking animation involves a bit of an upward bob
            //we apply that upward, not downward so that the down position is the same as our idle position
            if (NormalizeHeight && currentSprite.Height > _minHeight)
                position = new Vector2(position.X, position.Y - (currentSprite.Height - _minHeight));
            //we also allow for optionally normalizing the width! this is especially important for cases where we've flipped the sprite horizontally
            if (NormalizeWidth && currentSprite.Width > _minWidth)
                position = new Vector2(position.X - (currentSprite.Width - _minWidth), position.Y);
        }

        Sprites[currentFrame]?.Draw(spriteBatch, position);
    }
}
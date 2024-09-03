using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics;

public class SpriteAnimation
{
    private readonly float _defaultDuration;
    private readonly bool _shouldLoop;
    private readonly List<Sprite> _sprites = new();
    private readonly List<float> _spriteDurations = new();
    private float _maxHeight;

    private int CurrentFrame
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

    public float PlaybackProgress { get; set; }
    public bool IsPlaying { get; private set; } = true;

    public float Duration => _spriteDurations.Sum();

    public SpriteAnimation(float defaultDuration = float.NaN, bool shouldLoop = true)
    {
        _defaultDuration = defaultDuration;
        _shouldLoop = shouldLoop;
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

        _maxHeight = Math.Max(_maxHeight, sprite.Height);
        _sprites.Add(sprite);
        _spriteDurations.Add(_defaultDuration);
    }

    public void AddFrame(Sprite sprite, float duration)
    {
        _maxHeight = Math.Max(_maxHeight, sprite.Height);
        _sprites.Add(sprite);
        _spriteDurations.Add(duration);
    }

    public void Update(GameTime gameTime)
    {
        if (IsPlaying)
        {
            PlaybackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (PlaybackProgress > Duration)
            {
                if (_shouldLoop)
                    PlaybackProgress -= Duration;
                else
                    Stop();
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        //normalizing the heights because we draw from the top and if I don't we'd have weird effects when a sprites bobs
        //up and down
        var currentSprite = _sprites[CurrentFrame];
        if (currentSprite.Height < _maxHeight)
            position = new Vector2(position.X, position.Y + (_maxHeight - currentSprite.Height));

        _sprites[CurrentFrame]?.Draw(spriteBatch, position);
    }
}
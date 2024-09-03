using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics;

public class SpriteAnimation
{
    private readonly List<Sprite> _sprites = new();
    private readonly List<float> _spriteDurations = new();
    private float _maxHeight;

    public bool ShouldLoop { get; set; } = true;
    public float PlaybackProgress { get; set; }
    public bool IsPlaying { get; private set; } = true;

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

    public void Play()
    {
        IsPlaying = true;
    }

    public void Stop()
    {
        IsPlaying = false;
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

            var duration = _spriteDurations.Sum();
            
            if (PlaybackProgress > duration)
            {
                if (ShouldLoop)
                    PlaybackProgress -= duration;
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
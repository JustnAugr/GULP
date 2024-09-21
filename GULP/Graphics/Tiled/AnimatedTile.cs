using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics.Tiled;

public class AnimatedTile : Tile
{
    private readonly bool _shouldLoop;
    public readonly List<Tile> Tiles = new();
    private readonly List<float> _tileDurations = new();

    public int CurrentFrame
    {
        get
        {
            float durationSum = 0;
            for (int i = 0; i < _tileDurations.Count; i++)
            {
                durationSum += _tileDurations[i];

                if (durationSum >= PlaybackProgress)
                    return i;
            }

            return -1;
        }
    }
    public float PlaybackProgress { get; set; }
    public bool IsPlaying { get; private set; } = true;
    public float Duration { get; private set; }

    public AnimatedTile(Tile tile, bool shouldLoop = true) : base(tile.Texture, tile.X, tile.Y,
        tile.Width,
        tile.Height, tile.Id,
        tile.CollisionBox)
    {
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

    public void AddFrame(Tile tile, float duration)
    {
        Tiles.Add(tile);
        _tileDurations.Add(duration);
        Duration += duration;
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

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, float layerDepth)
    {
        if (Tiles.Count == 0 || !IsPlaying)
            base.Draw(spriteBatch, position, layerDepth);
        else
        {
            Tiles[CurrentFrame]?.Draw(spriteBatch, position, layerDepth);
        }
    }
}
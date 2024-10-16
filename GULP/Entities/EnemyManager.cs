using System;
using System.Collections.Generic;
using System.Linq;
using GULP.Graphics.Tiled;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object = GULP.Graphics.Tiled.Object;

namespace GULP.Entities;

public class EnemyManager : IEntity
{
    private const float TIME_PER_SPAWN = 20f; //every 20 seconds?

    private float _timeSinceLastSpawn = float.MaxValue;
    private readonly Random _random;

    private readonly Texture2D _slimeSpriteSheet;
    public Vector2 Position { get; set; }

    public EnemyManager(Texture2D slimeSpriteSheet)
    {
        _slimeSpriteSheet = slimeSpriteSheet;
        _random = new Random();
    }

    public void Update(GameTime gameTime)
    {
        //every x seconds, spawn a new creature in a random location in a random slimeSpawnObject
        _timeSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_timeSinceLastSpawn >= TIME_PER_SPAWN)
        {
            GameContext.GetComponent(out EntityManager entityManager);
            GameContext.GetComponent(out Map map);
            var slimeSpawnObjects = map.Objects.Where(obj => obj.Type == ObjectType.SlimeSpawn).ToList();
            var spawnObject = slimeSpawnObjects[_random.Next(0, slimeSpawnObjects.Count)];
            var x = spawnObject.Width > 0
                ? _random.Next(spawnObject.X, spawnObject.X + spawnObject.Width)
                : spawnObject.X;
            var y = spawnObject.Height > 0
                ? _random.Next(spawnObject.Y, spawnObject.Y + spawnObject.Height)
                : spawnObject.Y;

            var slime = new Slime(_slimeSpriteSheet, new Vector2(x, y));
            entityManager.AddEntity(slime);

            _timeSinceLastSpawn = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
    }
}
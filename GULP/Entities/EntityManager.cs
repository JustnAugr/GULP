using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class EntityManager
{
    private readonly Map _map;
    private readonly List<IEntity> _entities = new();
    private readonly List<IEntity> _entitiesToAdd = new();
    private readonly List<IEntity> _entitiesToRemove = new();

    //using a vector2 here to represent each tile (in map grid-space)
    //theoretically an entity could be standing on 1->N tiles based on its size
    public readonly Dictionary<Vector2, List<ICreature>> TileCreatureMap = new();

    public IEnumerable<IEntity> Entities => new ReadOnlyCollection<IEntity>(_entities);

    public EntityManager(Map map)
    {
        _map = map;
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            if (!_entitiesToRemove.Contains(entity))
            {
                entity.Update(gameTime);
            }
        }

        foreach (var entity in _entitiesToAdd)
        {
            _entities.Add(entity);
        }

        _entitiesToAdd.Clear();

        foreach (var entity in _entitiesToRemove)
        {
            _entities.Remove(entity);
        }

        _entitiesToRemove.Clear();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            entity.Draw(spriteBatch);
        }
    }

    public void AddEntity(IEntity entity)
    {
        _entitiesToAdd.Add(entity);

        if (entity is ICreature creature)
            AddTileCreaturePosition(creature, creature.Position);
    }

    public void RemoveEntity(IEntity entity)
    {
        _entitiesToRemove.Add(entity);

        if (entity is ICreature creature)
            RemoveTileCreaturePosition(creature, creature.Position);
    }

    public void RemoveTileCreaturePosition(ICreature creature, Vector2 position)
    {
        //get the tiles this entity was previously on, remove it from the tileEntityMap for that tile
        var oldTiles = _map.GetTiles(creature.GetCollisionBox(position));
        foreach (var oldTile in oldTiles)
        {
            TileCreatureMap.TryGetValue(oldTile, out var oldTileEntityList);
            oldTileEntityList?.Remove(creature);
            if (oldTileEntityList?.Count == 0)
                TileCreatureMap.Remove(oldTile);
        }
    }

    public void AddTileCreaturePosition(ICreature creature, Vector2 position)
    {
        var newTiles = _map.GetTiles(creature.GetCollisionBox(position));
        foreach (var tile in newTiles)
        {
            List<ICreature> newTileCreatureList = TileCreatureMap.GetValueOrDefault(tile, new List<ICreature>());
            newTileCreatureList.Add(creature);
            TileCreatureMap.TryAdd(tile, newTileCreatureList);
        }
    }
}
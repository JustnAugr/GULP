using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class EntityManager
{
    private readonly List<IEntity> _entities = new();
    private readonly List<IEntity> _entitiesToAdd = new();
    private readonly List<IEntity> _entitiesToRemove = new();

    public IEnumerable<IEntity> Entities => new ReadOnlyCollection<IEntity>(_entities);

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            if (!_entitiesToRemove.Contains(entity))
                entity.Update(gameTime);
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
    }

    public void RemoveEntity(IEntity entity)
    {
        _entitiesToRemove.Add(entity);
    }
}
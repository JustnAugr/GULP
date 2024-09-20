using System.Diagnostics;
using GULP.Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public interface ICreature : IEntity
{
    float Health { get; set; }
    Vector2 Direction { get; set; }
    SpriteDirection AnimDirection { get; set; }
    CreatureState State { get; }
    bool IsDealingDamage { get; }
    Rectangle GetCollisionBox();
    Rectangle GetCollisionBox(Vector2 position);
    bool Walk(Vector2 direction, GameTime gameTime);
    bool Attack(Vector2 direction, GameTime gameTime);
    bool Die();
    bool ReceiveDamage(float damageValue);
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public interface ICreature
{
    float Health { get; set; }
    CreatureState State { get; }
    bool IsDealingDamage { get; }
    Rectangle GetCollisionBox();
    bool Walk(Vector2 direction, GameTime gameTime);
    bool Attack(Vector2 direction, GameTime gameTime);
    bool Die();
    bool ReceiveDamage(float damageValue);
}
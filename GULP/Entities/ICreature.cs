using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public interface ICreature
{
    float Health { get; set; }
    CreatureState State { get; }
    bool IsDealingDamage { get; set; }
    Rectangle CollisionBox { get; }

    bool Walk(float x, float y, GameTime gameTime);
    bool Attack(float x, float y);
    bool Die();
    bool ReceiveDamage(float damageValue);
}
using System.Diagnostics;
using GULP.Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

//TODO I'm thinking this should be an abstract class given the amount of shared methods between the Player and Slime
public interface ICreature : IEntity
{
    float Health { get; set; }
    Vector2 Direction { get; set; }
    SpriteDirection AnimDirection { get; set; }
    CreatureState State { get; }
    Rectangle GetCollisionBox();
    Rectangle GetCollisionBox(Vector2 position);
    bool Walk(Vector2 direction, GameTime gameTime);
    bool Attack(Vector2 direction, GameTime gameTime);
    bool Die();
    bool ReceiveDamage(float damageValue);
}
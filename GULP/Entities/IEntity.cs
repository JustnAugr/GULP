using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public interface IEntity
{
    Vector2 Position { get; set; }

    void Update(GameTime gameTime);

    void Draw(SpriteBatch spriteBatch);
}
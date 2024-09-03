using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public interface IEntity
{
    int DrawOrder { get; }
    Vector2 Position { get; set; }

    void Update(GameTime gameTime);

    void Draw(SpriteBatch spriteBatch);
}
using GULP.Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GULP.Entities;

public abstract class Creature : IEntity
{
    protected readonly Texture2D SpriteSheet;
    protected Texture2D CollisionBoxTexture;
    protected readonly SpriteAnimationColl AnimationCollection = new();

    public float Health { get; set; }
    public Vector2 Direction { get; set; }
    public Vector2 Position { get; set; }
    public SpriteDirection AnimDirection { get; set; }
    public CreatureState State { get; protected set; }

    public bool IsAttacking => //attacking is going to be a "heavy" action, we can't cancel it
        State == CreatureState.Attacking && AnimationCollection.GetAnimation(State, AnimDirection).IsPlaying;

    protected Creature(Texture2D spriteSheet, Vector2 position)
    {
        SpriteSheet = spriteSheet;
        Position = position;

        //init values
        State = CreatureState.Idling;
        Direction = new Vector2(0, 1);
        AnimDirection = Direction.ToSpriteAnimation();
    }

    public Rectangle GetCollisionBox()
    {
        return GetCollisionBox(Position);
    }

    public abstract Rectangle GetCollisionBox(Vector2 position);

    public Rectangle GetAttackBox()
    {
        return GetAttackBox(Position);
    }

    public abstract Rectangle GetAttackBox(Vector2 position);

    public abstract bool Walk(Vector2 direction, GameTime gameTime);

    public void Idle()
    {
        //if we're currently attacking and mid animation, we can't go to idle
        if (IsAttacking)
            return;

        if (State != CreatureState.Idling)
            AnimationCollection.GetAnimation(State, AnimDirection).PlaybackProgress = 0;

        State = CreatureState.Idling;
        AnimationCollection.GetAnimation(State, AnimDirection).Play();
    }

    public virtual bool Attack(Vector2 direction, GameTime gameTime)
    {
        AnimDirection = direction.ToSpriteAnimation();
        Direction = direction;

        return Attack(gameTime);
    }

    public abstract bool Attack(GameTime gameTime);
    public abstract bool Die();
    public abstract bool ReceiveDamage(float damageValue);

    public void Update(GameTime gameTime)
    {
        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Update(gameTime);

        //say we were attacking and this .Update() finished the attack animation,
        //rather than draw nothing we should go idle
        if (!animation.IsPlaying)
            Idle();
    }

    public void DrawCollisionBox(SpriteBatch spriteBatch)
    {
        var collisionBox = GetCollisionBox();
        spriteBatch.Draw(CollisionBoxTexture, new Vector2(collisionBox.X, collisionBox.Y),
            new Rectangle(0, 0, collisionBox.Width, collisionBox.Height),
            Color.White * .5f, 0f,
            Vector2.Zero, 1, SpriteEffects.None, 1f);
    }

    public abstract void DrawAttackBox(SpriteBatch spriteBatch);

    public void Draw(SpriteBatch spriteBatch)
    {
        //This will get moved into some DebugHelper or something, it allows me to see the player's actual collisionbox
        //drawn on top of the sprite
        if (true)
        {
            DrawCollisionBox(spriteBatch);

            if (IsAttacking)
                DrawAttackBox(spriteBatch);
        }

        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position);
    }
}
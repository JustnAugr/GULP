using System;
using System.Collections.Generic;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GULP.Entities;

public abstract class Creature : IEntity
{
    private const float FRICTION = .25f;
    protected virtual float Acceleration => 0f;
    protected virtual float MaxVelocity => 0f;
    protected virtual float InitialVelocity => 0f;

    protected float Velocity;
    
    protected readonly Map Map;
    protected readonly EntityManager EntityManager;
    
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

    protected Creature(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager)
    {
        SpriteSheet = spriteSheet;
        Position = position;
        Map = map;
        EntityManager = entityManager;

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

    protected HashSet<Rectangle> GetCollisions(Vector2 position)
    {
        var collisionBox = GetCollisionBox(position);
        var tiles = Map.GetTiles(collisionBox);
        //these are the collisions we'd meet at those tiles
        var collisions = Map.GetTileCollisions(tiles);

        foreach (var tile in tiles)
        {
            var creatureListExists = EntityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
            if (creatureListExists && creatureList is { Count: > 0 })
            {
                foreach (var creature in creatureList)
                {
                    if (creature != this)
                        collisions.Add(creature.GetCollisionBox());
                }
            }
        }

        foreach (var collision in collisions)
        {
            if (!collision.Intersects(collisionBox))
            {
                //return anything that's not actually colliding, this is possible if we're on the same tile but not
                //really intersecting it yet
                collisions.Remove(collision);
            }
        }

        return collisions;
    }

    protected void UpdatePosition(Vector2 newPosition)
    {
        //apply our new position and
        //also update our tile->creature position dicts in the entityManager
        var oldPosition = Position;
        Position = newPosition;

        if (oldPosition != Position)
        {
            EntityManager.RemoveTileCreaturePosition(this, oldPosition);
            EntityManager.AddTileCreaturePosition(this, Position);
        }
    }

    public Rectangle GetAttackBox()
    {
        return GetAttackBox(Position);
    }

    public abstract Rectangle GetAttackBox(Vector2 position);

    public bool Walk(Vector2 direction, GameTime gameTime)
    {
        //if we're currently attacking and mid animation, we can't attack-cancel to run
        if (IsAttacking)
            return false;

        //if we were just idilng, we need to build up speed. no speed buildup needed for attacks
        if (State == CreatureState.Idling)
            Velocity = InitialVelocity;

        //reset the old animation to 0 before we switch off of it, so that when we resume it later we resume from the start
        var newAnimationDirection = direction.ToSpriteAnimation();
        if (State is not CreatureState.Walking || AnimDirection != newAnimationDirection)
        {
            AnimationCollection.GetAnimation(State, AnimDirection).PlaybackProgress = 0;
        }

        //we're walking, in a direction, with the animation facing that direction
        State = CreatureState.Walking;
        AnimDirection = direction.ToSpriteAnimation();
        Direction = direction;

        //ensure we're playin our animation if we've just changed from another one
        AnimationCollection.GetAnimation(State, AnimDirection).Play();

        //increase our velocity by our acceleration up to our max velocity
        Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity = Math.Min(Velocity, MaxVelocity);

        //diagonalAdj helps us accomodate moving on two axis, or we'd move super fast on the diag
        var diagonalAdj = direction.X != 0 && direction.Y != 0 ? 1.5f : 1f;

        //full eq for newPos = currentPos + (velocity * acceleration * gameTime * direction)
        var posX = Position.X + (Velocity / diagonalAdj) * direction.X;
        var posY = Position.Y + (Velocity / diagonalAdj) * direction.Y;

        //simple bounding for now to prevent us from going off screen
        var currentSprite = AnimationCollection.GetAnimation(State, AnimDirection).CurrentSprite;
        if (posX < 0 || posX > Map.PixelWidth - currentSprite.Width)
            posX = Position.X;

        if (posY < 0 || posY > Map.PixelHeight - currentSprite.Height)
            posY = Position.Y;

        //Collision Checking v2 
        //these are the tiles we'd be at if we moved in just the Y direction
        //we're only applying the Y movement, so just check the Y direction
        if (direction.Y != 0)
        {
            var collisionsY = GetCollisions(new Vector2(Position.X, posY));
            //if we have at least 1 collision
            if (collisionsY.Count != 0)
            {
                direction.Y = 0;
                Velocity = Math.Max(InitialVelocity,
                    Velocity - FRICTION); //we apply friction until it causes us to get down to init velocity
                posY =
                    Position.Y + Velocity / diagonalAdj * direction.Y; //some friction constant
            }
        }

        //these are the tiles we'd be at if we moved in just the Y direction
        //we're only applying the Y movement, so just check the Y direction
        if (direction.X != 0)
        {
            var collisionsX = GetCollisions(new Vector2(posX, Position.Y));
            //if we have at least 1 collision
            if (collisionsX.Count != 0)
            {
                direction.X = 0;
                Velocity = Math.Max(InitialVelocity,
                    Velocity - FRICTION); //we apply friction until it causes us to get down to init velocity
                posX = Position.X + Velocity / diagonalAdj * direction.X;
            }
        }

        UpdatePosition(new Vector2(posX, posY));
        return true;
    }

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

    public virtual void Update(GameTime gameTime)
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
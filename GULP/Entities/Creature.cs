using System;
using System.Collections.Generic;
using System.Diagnostics;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GULP.Entities;

public abstract class Creature : IEntity
{
    //absolute creature constants
    private const float DAMAGED_COLOR_TIME = .25f;

    //constants to be overriden by each creature
    protected virtual float Acceleration => 0f;
    protected virtual float MaxVelocity => 0f;
    protected virtual float InitialVelocity => 0f;
    protected virtual float Friction => 0f;
    protected virtual float IdleVelocityPenalty => 0f;

    protected float Velocity;

    protected readonly Map Map;
    protected readonly EntityManager EntityManager;

    protected readonly Texture2D SpriteSheet;
    protected Texture2D CollisionBoxTexture;
    protected Texture2D HorizontalBoxTexture;
    protected Texture2D VerticalBoxTexture;
    protected readonly SpriteAnimationColl AnimationCollection = new();

    private bool _damaged;
    private readonly Color _damagedColor = Color.Red * .75f;
    private float _damagedTimer;

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
        return GetCollisions(position, Direction);
    }

    private HashSet<Rectangle> GetCollisions(Vector2 position, Vector2 direction)
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

        //why do we do this? well imagine we're running to the right but someone is running into us from the left
        //we should still be able to run right, so we adjust the collisionBox to just consider the rightmost slice of width=1
        //similar concept for up/down mvmt
        var adjustedCollisionBox = GetDirectionAdjustedCollisionBox(collisionBox, direction);
        foreach (var collision in collisions)
        {
            if (!collision.Intersects(adjustedCollisionBox))
            {
                //return anything that's not actually colliding, this is possible if we're on the same tile but not
                //really intersecting it yet
                collisions.Remove(collision);
            }
        }

        return collisions;
    }

    private Rectangle GetDirectionAdjustedCollisionBox(Rectangle collisionBox, Vector2 direction)
    {
        if (direction.X > 0)
            return new Rectangle(collisionBox.Right, collisionBox.Y, 1, collisionBox.Height);
        if (direction.X < 0)
            return new Rectangle(collisionBox.Left, collisionBox.Y, 1, collisionBox.Height);

        if (direction.Y > 0)
            return new Rectangle(collisionBox.X, collisionBox.Bottom, collisionBox.Width, 1);
        if (direction.Y < 0)
            return new Rectangle(collisionBox.X, collisionBox.Top, collisionBox.Width, 1);

        //this shouldn't ever happen
        return collisionBox;
    }

    private void UpdatePosition(Vector2 newPosition)
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

    protected Rectangle GetAttackBox()
    {
        return GetAttackBox(Position);
    }

    protected abstract Rectangle GetAttackBox(Vector2 position);

    public bool Walk(Vector2 direction, GameTime gameTime)
    {
        //if we're currently attacking and mid animation, we can't attack-cancel to run
        if (IsAttacking)
            return false;
        
        //get the oldAnimation
        var oldAnimation = AnimationCollection.GetAnimation(State, AnimDirection);

        //set our stateful props
        //we're walking, in a direction, with the animation facing that direction
        State = CreatureState.Walking;
        AnimDirection = direction.ToSpriteAnimation();
        Direction = direction;

        //ensure we're playin our animation if we've just changed from another one
        var newAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
        if (oldAnimation != newAnimation)
        {
            oldAnimation.Stop();
            newAnimation.Play();
        }

        //increase our velocity by our acceleration up to our max velocity
        Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity = Math.Min(Velocity, MaxVelocity);

        Move(direction, Velocity, gameTime);
        return true;
    }

    protected void Move(Vector2 direction, float velocity, GameTime gameTime)
    {
        //diagonalAdj helps us accomodate moving on two axis, or we'd move super fast on the diag
        var diagonalAdj = direction.X != 0 && direction.Y != 0 ? 1.5f : 1f;

        //full eq for newPos = currentPos + (velocity * acceleration * gameTime * direction)
        var posX = Position.X + (velocity / diagonalAdj) * direction.X;
        var posY = Position.Y + (velocity / diagonalAdj) * direction.Y;

        var currentSprite = AnimationCollection.GetAnimation(State, AnimDirection).CurrentSprite;

        //before we do any bounds or collisions checking, are we even on screen?
        //this is so if we're spawning creatures off screen, they can wander ON screen
        //TBD on if we'll need this when we move to DFS based pathing (which would presumably spawn them on a tile on screen
        var inBoundsX = (Position.X >= 0 && Position.X <= Map.PixelWidth);
        var inBoundsY = (Position.Y >= 0 && Position.Y <= Map.PixelHeight);

        if (inBoundsX && inBoundsY)
        {
            //simple bounding for now to prevent us from going off screen
            if (posX < 0 || posX > Map.PixelWidth - currentSprite.Width)
                posX = Position.X;

            if (posY < 0 || posY > Map.PixelHeight - currentSprite.Height)
                posY = Position.Y;

            //Collision Checking v2 
            //these are the tiles we'd be at if we moved in just the Y direction
            //we're only applying the Y movement, so just check the Y direction
            if (direction.Y != 0)
            {
                var collisionsY = GetCollisions(new Vector2(Position.X, posY), new Vector2(0, direction.Y));
                //if we have at least 1 collision
                if (collisionsY.Count != 0)
                {
                    direction.Y = 0;
                    Velocity = Math.Max(InitialVelocity,
                        Velocity - Friction *
                        (float)gameTime.ElapsedGameTime
                            .TotalSeconds); //we apply friction until it causes us to get down to init velocity
                    Debug.WriteLine(Velocity);
                    posY =
                        Position.Y + Velocity / diagonalAdj * direction.Y; //some friction constant
                }
            }

            //these are the tiles we'd be at if we moved in just the Y direction
            //we're only applying the Y movement, so just check the Y direction
            if (direction.X != 0)
            {
                var collisionsX = GetCollisions(new Vector2(posX, Position.Y), new Vector2(direction.X, 0));
                //if we have at least 1 collision
                if (collisionsX.Count != 0)
                {
                    direction.X = 0;
                    Velocity = Math.Max(InitialVelocity,
                        Velocity - Friction *
                        (float)gameTime.ElapsedGameTime
                            .TotalSeconds); //we apply friction until it causes us to get down to init velocity
                    Debug.WriteLine(Velocity);
                    posX = Position.X + Velocity / diagonalAdj * direction.X;
                }
            }
        }

        //both our entity map and our self reference of position
        UpdatePosition(new Vector2(posX, posY));
    }

    public void Idle(GameTime gameTime)
    {
        //if we're currently attacking and mid animation, we can't go to idle
        if (IsAttacking)
            return;

        //if we weren't idling we should reset our old animation
        var oldAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
        State = CreatureState.Idling;
        var newAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
        if (oldAnimation != newAnimation)
        {
            oldAnimation.Stop();
            newAnimation.Play();
        }

        //because we've been idling, decrease our velocity over time
        Velocity = Math.Max(Velocity - IdleVelocityPenalty * (float)gameTime.ElapsedGameTime.TotalSeconds,
            InitialVelocity);
    }

    public virtual bool Attack(GameTime gameTime)
    {
        return Attack(Direction, gameTime);
    }

    public bool Attack(Vector2 direction, GameTime gameTime)
    {
        //persist old props
        var previousState = State;
        var oldAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
        
        //set new stateful props
        AnimDirection = direction.ToSpriteAnimation();
        Direction = direction;
        State = CreatureState.Attacking;
        
        //reset our old animation, play our new animation as needed
        var newAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
        if (oldAnimation != newAnimation)
        {
            oldAnimation.Stop();
            newAnimation.Play();
        }

        //if we are in the process of a new attack, check if we've hit anything
        //prevents player from hitting attach 1ce but damage applying for each frame of the attack 
        if (previousState != CreatureState.Attacking)
        {
            //the play here matches what we did for collisions more or less:
            //take our rectangle (the attackbox drawn based on the first frame), get the tiles under it,
            //get all entities at those tiles, check if our attackBox intersects their collisionBox
            //if it does, we consider it a hit on that creature
            var attackBox = GetAttackBox();
            var tiles = Map.GetTiles(attackBox);

            //a set so that an entity standing on 2 tiles, both in the path of our sword doesn't take 2 hits
            var creatureSet = new HashSet<Creature>();
            foreach (var tile in tiles)
            {
                var creatureListExists = EntityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
                if (!creatureListExists || creatureList is not { Count: > 0 })
                    continue;

                //not type==self and intersecting? you're gonna get hit!
                //TODO this would need to be reworked if we had multiple enemy types, unless we're implicitly enabling friendly fire...
                foreach (var creature in creatureList)
                {
                    if (!creatureSet.Contains(creature) && creature.GetType() != GetType() &&
                        creature.GetCollisionBox().Intersects(attackBox))
                    {
                        Debug.WriteLine("HIT! on: " + creature.GetType());
                        creature.ReceiveDamage(25f);
                        creatureSet.Add(creature); //make sure we don't hit again...
                    }
                }
            }
        }

        return true;
    }

    public abstract bool Die();

    public bool ReceiveDamage(float damageValue)
    {
        Health -= damageValue;
        _damaged = true;

        return true;
    }

    public virtual void Update(GameTime gameTime)
    {
        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Update(gameTime);

        //say we just finished attacking after the above ^, then we should Idle until we receive a new input
        if (!animation.IsPlaying)
            Idle(gameTime);

        //time to die edition
        if (Health <= 0)
        {
            Debug.WriteLine(GetType() + " would have died!");
            EntityManager.RemoveEntity(this);
        }

        //Progress our 'damaged' timer if we've been damaged and are showing red
        if (!_damaged) return;
        _damagedTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_damagedTimer > DAMAGED_COLOR_TIME)
        {
            _damaged = false;
            _damagedTimer = 0;
        }
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
        if (false)
        {
            DrawCollisionBox(spriteBatch);

            if (IsAttacking)
                DrawAttackBox(spriteBatch);
        }

        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position, _damaged ? _damagedColor : Color.White);
    }
}
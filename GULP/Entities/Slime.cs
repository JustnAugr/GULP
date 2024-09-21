using System;
using System.Diagnostics;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using GULP.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Slime : ICreature
{
    private const float WALK_VELOCITY = 2.0f;

    private readonly Texture2D _spriteSheet;
    private readonly Map _map;
    private readonly Camera _camera;
    private readonly EntityManager _entityManager;
    private SpriteAnimationColl _animColl;

    public bool IsDealingDamage { get; }
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }
    public SpriteDirection AnimDirection { get; set; }

    public Slime(Texture2D spriteSheet, Vector2 position, Map map, Camera camera, EntityManager entityManager)
    {
        _spriteSheet = spriteSheet;
        Position = position;
        _map = map;
        _camera = camera;
        _entityManager = entityManager;

        State = CreatureState.Idling;

        _animColl = new SpriteAnimationColl();
        InitializeIdleAnimations();
        InitializeWalkAnimation();
    }

    private void InitializeIdleAnimations()
    {
        var ANIM_IDLE_FRAME_DURATION = 1 / 5f;
        var idleDown = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 8, 12, 16, 12));
        idleDown.AddFrame(new Sprite(_spriteSheet, 40, 12, 16, 12));
        idleDown.AddFrame(new Sprite(_spriteSheet, 72, 13, 16, 11));
        idleDown.AddFrame(new Sprite(_spriteSheet, 104, 13, 16, 11));

        var idleUp = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 8, 76, 16, 12));
        idleUp.AddFrame(new Sprite(_spriteSheet, 40, 76, 16, 12));
        idleUp.AddFrame(new Sprite(_spriteSheet, 72, 77, 16, 11));
        idleUp.AddFrame(new Sprite(_spriteSheet, 104, 77, 16, 11));

        var idleLeft = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 8, 44, 16, 12, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 40, 44, 16, 12, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 72, 45, 16, 11, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 104, 45, 16, 11, SpriteEffects.FlipHorizontally));

        var idleRight = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 8, 44, 16, 12));
        idleRight.AddFrame(new Sprite(_spriteSheet, 40, 44, 16, 12));
        idleRight.AddFrame(new Sprite(_spriteSheet, 72, 45, 16, 11));
        idleRight.AddFrame(new Sprite(_spriteSheet, 104, 45, 16, 11));

        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Down, idleDown);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Up, idleUp);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Left, idleLeft);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Right, idleRight);
    }

    private void InitializeWalkAnimation()
    {
        var ANIM_WALK_FRAME_DURATION = 1 / 8f;

        var walkDown = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 6, 110, 20, 10));
        walkDown.AddFrame(new Sprite(_spriteSheet, 42, 104, 12, 16));
        walkDown.AddFrame(new Sprite(_spriteSheet, 73, 102, 14, 18));
        walkDown.AddFrame(new Sprite(_spriteSheet, 104, 105, 16, 15));
        walkDown.AddFrame(new Sprite(_spriteSheet, 136, 108, 16, 12));
        walkDown.AddFrame(new Sprite(_spriteSheet, 168, 109, 16, 11));

        var walkUp = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 6, 174, 20, 10));
        walkUp.AddFrame(new Sprite(_spriteSheet, 42, 168, 12, 16));
        walkUp.AddFrame(new Sprite(_spriteSheet, 73, 166, 14, 18));
        walkUp.AddFrame(new Sprite(_spriteSheet, 104, 169, 16, 15));
        walkUp.AddFrame(new Sprite(_spriteSheet, 136, 172, 16, 12));
        walkUp.AddFrame(new Sprite(_spriteSheet, 168, 173, 16, 11));

        var walkRight = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 6, 142, 20, 12));
        walkRight.AddFrame(new Sprite(_spriteSheet, 42, 136, 12, 16));
        walkRight.AddFrame(new Sprite(_spriteSheet, 74, 134, 12, 18));
        walkRight.AddFrame(new Sprite(_spriteSheet, 104, 137, 16, 15));
        walkRight.AddFrame(new Sprite(_spriteSheet, 136, 140, 16, 12));
        walkRight.AddFrame(new Sprite(_spriteSheet, 168, 141, 16, 11));

        var walkLeft = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 6, 142, 20, 10, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 42, 136, 12, 16, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 74, 134, 12, 18, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 104, 137, 16, 15, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 136, 140, 16, 12, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 168, 141, 16, 11, SpriteEffects.FlipHorizontally));

        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Down, walkDown);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Up, walkUp);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Right, walkRight);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Left, walkLeft);
    }

    public Rectangle GetCollisionBox()
    {
        return GetCollisionBox(Position);
    }

    public Rectangle GetCollisionBox(Vector2 position)
    {
        //get our max height and width sprites for the current animation
        var sprite = _animColl.GetAnimation(State, AnimDirection).CurrentSprite;

        //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
        //towards the feet on the y-axis
        var COLLISION_BOX_WIDTH = 12;
        var COLLISION_BOX_HEIGHT = 10;

        var rect = new Rectangle((int)Math.Floor(position.X) + sprite.Width / 2 - COLLISION_BOX_WIDTH / 2,
            (int)Math.Floor(position.Y) + (int)(sprite.Height / 2) - COLLISION_BOX_HEIGHT/2,
            COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);

        return rect;
    }

    private SpriteDirection GetAnimationDirection(Vector2 direction)
    {
        //TODO shared between player, move it
        var x = direction.X;
        var y = direction.Y;

        //if we're moving left or right (non-diag or diag), we should face that way
        //else just face up or down
        if (x < 0)
            return SpriteDirection.Left;
        if (x > 0)
            return SpriteDirection.Right;
        if (y < 0)
            return SpriteDirection.Up;
        if (y > 0)
            return SpriteDirection.Down;

        return SpriteDirection.Down;
    }

    public bool Walk(Vector2 direction, GameTime gameTime)
    {
        var newAnimationDirection = GetAnimationDirection(direction);
        if (State is not CreatureState.Walking || AnimDirection != newAnimationDirection)
        {
            _animColl.GetAnimation(State, AnimDirection).PlaybackProgress = 0;
        }
        
        //set new values pertaining to the state of this entity
        State = CreatureState.Walking;
        AnimDirection = newAnimationDirection;
        Direction = direction;
        
        //ensure we're playin our animation if we've just changed from another one
        _animColl.GetAnimation(State, AnimDirection).Play();
        
        //diagonalAdj helps us accomodate moving on two axis, or we'd move super fast on the diag
        var diagonalAdj = direction.X != 0 && direction.Y != 0 ? 1.5f : 1f;
        
        //full eq for newPos = currentPos + (velocity * acceleration * gameTime * direction)
        var posX = Position.X + (WALK_VELOCITY / diagonalAdj) * direction.X;
        var posY = Position.Y + (WALK_VELOCITY / diagonalAdj) * direction.Y;
        
        //simple bounding for now to prevent us from going off screen
        var currentSprite = _animColl.GetAnimation(State, AnimDirection).CurrentSprite;
        if (posX < 0 || posX > _map.PixelWidth - currentSprite.Width)
            posX = Position.X;
        
        if (posY < 0 || posY > _map.PixelHeight - currentSprite.Height)
            posY = Position.Y;
        
        
        //Collision Checking v2 //TODO pull this out to a new method, a lot of it is shared between creatures as well
        //these are the tiles we'd be at if we moved in just the Y direction
        var collisionBoxY = GetCollisionBox(new Vector2(Position.X, posY));
        var tilesY = _map.GetTiles(collisionBoxY);
        //these are the collisions we'd meet at those tiles
        var collisionsY = _map.GetTileCollisions(tilesY);

        foreach (var tile in tilesY)
        {
            var creatureListExists = _entityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
            if (creatureListExists && creatureList is { Count: > 0 })
            {
                foreach (var creature in creatureList)
                {
                    if (creature != this)
                        collisionsY.Add(creature.GetCollisionBox());
                }
            }
        }

        foreach (var collision in collisionsY)
        {
            //we're only applying the Y movement, so just check the Y direction
            if (direction.Y != 0)
            {
                if (collision.Intersects(collisionBoxY))
                {
                    direction.Y = 0;
                    posY =
                        Position.Y + WALK_VELOCITY / diagonalAdj * direction.Y; //some friction constant
                }
            }
        }

        //these are the tiles we'd be at if we moved in just the Y direction
        var collisionBoxX = GetCollisionBox(new Vector2(posX, Position.Y));
        var tilesX = _map.GetTiles(collisionBoxX);
        //these are the collisions we'd meet at those tiles
        var collisionsX = _map.GetTileCollisions(tilesX);

        foreach (var tile in tilesX)
        {
            var creatureListExists = _entityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
            if (creatureListExists && creatureList is { Count: > 0 })
            {
                foreach (var creature in creatureList)
                {
                    if (creature != this)
                        collisionsX.Add(creature.GetCollisionBox());
                }
            }
        }

        foreach (var collision in collisionsX)
        {
            //we're only applying the Y movement, so just check the Y direction
            if (direction.X != 0)
            {
                if (collision.Intersects(collisionBoxX))
                {
                    direction.X = 0;
                    posX = Position.X + WALK_VELOCITY / diagonalAdj * direction.X;
                }
            }
        }

        //apply our new position, bounded by the world and by collisions
        //also update our tile->creature position dicts
        var oldPosition = Position;
        Position = new Vector2(posX, posY);

        if (oldPosition != Position)
        {
            _entityManager.RemoveTileCreaturePosition(this, oldPosition);
            _entityManager.AddTileCreaturePosition(this, Position);
        }
        
        return true;
    }

    public void Idle()
    {
        if (State != CreatureState.Idling)
            _animColl.GetAnimation(State, AnimDirection).PlaybackProgress = 0;

        //todo add to iface
        State = CreatureState.Idling;
        _animColl.GetAnimation(State, AnimDirection).Play();
    }

    public bool Attack(Vector2 direction, GameTime gameTime)
    {
        throw new System.NotImplementedException();
    }

    public bool Die()
    {
        throw new System.NotImplementedException();
    }

    public bool ReceiveDamage(float damageValue)
    {
        throw new System.NotImplementedException();
    }

    public void Update(GameTime gameTime)
    {
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //TODO make this into a more general function, as it's the draw culling method
        if (Position.X + 30 < _camera.Left || Position.X > _camera.Right || Position.Y + 30 < _camera.Top ||
            Position.Y > _camera.Bottom)
            return;
        
        if (false)
        {
            //todo why is this so fucking intensive????? lags like crazy
            var rect = GetCollisionBox();

            var boxTexture = new Texture2D(_spriteSheet.GraphicsDevice, rect.Width, rect.Height);
            var boxData = new Color[rect.Width * rect.Height];

            for (int i = 0; i < boxData.Length; i++)
            {
                boxData[i] = Color.Yellow;
            }

            boxTexture.SetData(boxData);
            spriteBatch.Draw(boxTexture, new Vector2(rect.X, rect.Y), new Rectangle(0, 0, rect.Width, rect.Height),
                Color.White, 0f,
                Vector2.Zero, 1, SpriteEffects.None, (Position.Y + rect.Height + 100) / GULPGame.SCREEN_Y_RESOLUTION);
        }
        
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position);
    }
}
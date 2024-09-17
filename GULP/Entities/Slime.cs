using System;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Slime : IEntity, ICreature
{
    private const float WALK_VELOCITY = 2.0f;
    
    private readonly Texture2D _spriteSheet;
    private readonly Map _map;
    private SpriteAnimationColl _animColl;

    public bool IsDealingDamage { get; }
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }
    public SpriteDirection AnimDirection { get; set; }

    public Slime(Texture2D spriteSheet, Vector2 position, Map map)
    {
        _spriteSheet = spriteSheet;
        Position = position;
        _map = map;

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
        var maxHSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Height);
        var maxWSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Width);

        //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
        //towards the feet on the y-axis
        var COLLISION_BOX_WIDTH = 10;
        var COLLISION_BOX_HEIGHT = 10;
        
        var rect = new Rectangle((int)Math.Floor(position.X) + maxWSprite.Width / 2 - COLLISION_BOX_WIDTH / 2,
            (int)Math.Floor(position.Y) + (maxHSprite.Height / 2) - (int)(COLLISION_BOX_HEIGHT / 2.5),
            COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);

        //there can be cases where jamming yourself into a box on the X axis and then trying to move on the Y
        //will cause the player to get stuck bc the collisionBox shifts as the animation does from left/right to up/down
        //handle that by deflating by a pixel to allow us to get out of it
        if (Direction.X == 0 && Direction.Y != 0 && State is CreatureState.Walking)
        {
            rect.Inflate(-1, 0);
        }

        return rect;
    }
    
    private void SetAnimationDirection(Vector2 direction)
    {
        //TODO shared between player, move it
        var x = direction.X;
        var y = direction.Y;

        //if we're moving left or right (non-diag or diag), we should face that way
        //else just face up or down
        if (x < 0)
            AnimDirection = SpriteDirection.Left;
        else if (x > 0)
            AnimDirection = SpriteDirection.Right;
        else if (y < 0)
            AnimDirection = SpriteDirection.Up;
        else if (y > 0)
            AnimDirection = SpriteDirection.Down;
    }

    public bool Walk(Vector2 direction, GameTime gameTime)
    {
        //TODO I want this to be a "heavy" action where the animation gets locked in, even if positionally blocked by collision
        //TODO collision with other entities
        State = CreatureState.Walking;
        SetAnimationDirection(direction);
        Direction = direction;
        
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

        //Collision checking
        //we break up the X and Y collision checking so that we can apply them independently, for example
        //if our player is up against a wall and trying to go diagonal -> we'd apply a sliding effect along the wall
        var collisionDiffTol = 1e-4f;

        //check Y first: get the collisionBox if we accept the new Y, and then check using JUST the Y direction
        var directionPostCollision =
            _map.AdjustDirectionCollisions(GetCollisionBox(new Vector2(Position.X, posY)), new Vector2(0, direction.Y),
                1);
        if (Math.Abs(direction.Y - directionPostCollision.Y) > collisionDiffTol)
        {
            posY =
                Position.Y + WALK_VELOCITY / diagonalAdj * directionPostCollision.Y; //some friction constant
        }

        //now check X: get the collision box if we accept the new X, then check using JUST the X direction
        directionPostCollision =
            _map.AdjustDirectionCollisions(GetCollisionBox(new Vector2(posX, Position.Y)), new Vector2(direction.X, 0),
                1);
        if (Math.Abs(direction.X - directionPostCollision.X) > collisionDiffTol)
        {
            posX = Position.X + WALK_VELOCITY / diagonalAdj * directionPostCollision.X;
        }

        //apply our new position, bounded by the world and by collisions
        Position = new Vector2(posX, posY);
        
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
        //todo should this take a "layer" param to allow us to draw slimes just below players for example?
        
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position);
    }
}
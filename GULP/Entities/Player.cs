using System;
using System.Diagnostics;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Player : IEntity, ICreature
{
    private const float ACCELERATION = 1.0f;
    private const float MAX_VELOCITY = 5.0f;
    private const float INITIAL_VELOCITY = 1.0f;

    //animation frame durations
    private const float ANIM_IDLE_FRAME_DURATION = 1 / 4f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 15f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 10f;

    private const float DAMAGE_DEALING_FRAME = 1;

    //spritesheet and animations
    private readonly Texture2D _spriteSheet;
    private readonly Map _map;
    private SpriteAnimationColl _animColl;

    private float _velocity;

    public int DrawOrder => 10; //above the ground at 0
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }

    public bool IsDealingDamage => IsAttacking &&
                                   Math.Abs(_animColl.GetAnimation(State, AnimDirection).CurrentFrame -
                                            DAMAGE_DEALING_FRAME) < 0.01;

    public Rectangle CollisionBox
    {
        get
        {
            //TODO jesus christ this needs cleaning, some constants
            
            //get our max height and width sprites for the current animation
            var maxHSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Height);
            var maxWSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Width);

            //we use constant size for our collisionbox rather than a variable sprite inset
            var width = 8;
            var height = 9;
            //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
            //towards the feet on the y-axis
            var rect = new Rectangle((int)Math.Floor(Position.X) + maxWSprite.Width / 2 - width / 2,
                (int)Math.Floor(Position.Y) + (maxHSprite.Height / 2) - (int)(height / 2.5), width, height);
            
            //there can be cases where jamming yourself into a box on the X axis and then trying to move on the Y
            //will cause the player to get stuck bc the collisionbox shifts as the animation does from left/right to up/down
            //handle that by deflating by a pixel to allow us to get out of it
            if (Direction.X == 0 && Direction.Y != 0 && State is CreatureState.Walking)
            {
                rect.Inflate(-1, 0);
            }

            return rect;
        }
    }

    public SpriteDirection AnimDirection { get; set; }

    public bool IsAttacking => //attacking is going to be a "heavy" action, we can't cancel it
        State == CreatureState.Attacking && _animColl.GetAnimation(State, AnimDirection).IsPlaying;

    public Player(Texture2D spriteSheet, Vector2 position, Map map)
    {
        _spriteSheet = spriteSheet;
        _map = map;

        //values on initialization
        Position = position;
        Health = 100;
        State = CreatureState.Idling;

        //initialize our animations and store them in a double keyed collection for easy lookup
        _animColl = new SpriteAnimationColl();
        InitializeIdleAnimations();
        InitializeWalkAnimations();
        InitializeAttackAnimations();
    }

    private void InitializeIdleAnimations()
    {
        var idleDown = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 18, 22, 13, 21));
        idleDown.AddFrame(new Sprite(_spriteSheet, 66, 22, 13, 21));
        idleDown.AddFrame(new Sprite(_spriteSheet, 114, 22, 13, 21));
        idleDown.AddFrame(new Sprite(_spriteSheet, 162, 23, 13, 20));
        idleDown.AddFrame(new Sprite(_spriteSheet, 210, 23, 13, 20));
        idleDown.AddFrame(new Sprite(_spriteSheet, 258, 23, 13, 20));

        var idleUp = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 18, 118, 13, 21));
        idleUp.AddFrame(new Sprite(_spriteSheet, 66, 118, 13, 21));
        idleUp.AddFrame(new Sprite(_spriteSheet, 114, 118, 13, 21));
        idleUp.AddFrame(new Sprite(_spriteSheet, 162, 119, 13, 20));
        idleUp.AddFrame(new Sprite(_spriteSheet, 210, 119, 13, 20));
        idleUp.AddFrame(new Sprite(_spriteSheet, 258, 119, 13, 20));

        var idleLeft = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 17, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 65, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 113, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 161, 71, 15, 20, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 209, 71, 15, 20, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(_spriteSheet, 257, 71, 15, 20, SpriteEffects.FlipHorizontally));

        var idleRight = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 17, 70, 15, 21));
        idleRight.AddFrame(new Sprite(_spriteSheet, 65, 70, 15, 21));
        idleRight.AddFrame(new Sprite(_spriteSheet, 113, 70, 15, 21));
        idleRight.AddFrame(new Sprite(_spriteSheet, 161, 71, 15, 20));
        idleRight.AddFrame(new Sprite(_spriteSheet, 209, 71, 15, 20));
        idleRight.AddFrame(new Sprite(_spriteSheet, 257, 71, 15, 20));

        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Down, idleDown);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Up, idleUp);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Left, idleLeft);
        _animColl.AddAnimation(CreatureState.Idling, SpriteDirection.Right, idleRight);
    }

    private void InitializeWalkAnimations()
    {
        var walkDown = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 18, 164, 13, 23));
        walkDown.AddFrame(new Sprite(_spriteSheet, 66, 165, 13, 22));
        walkDown.AddFrame(new Sprite(_spriteSheet, 114, 166, 13, 21));
        walkDown.AddFrame(new Sprite(_spriteSheet, 162, 164, 13, 23));
        walkDown.AddFrame(new Sprite(_spriteSheet, 210, 165, 13, 22));
        walkDown.AddFrame(new Sprite(_spriteSheet, 258, 166, 13, 21));

        var walkUp = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 18, 261, 13, 22));
        walkUp.AddFrame(new Sprite(_spriteSheet, 66, 262, 13, 21));
        walkUp.AddFrame(new Sprite(_spriteSheet, 114, 263, 13, 20));
        walkUp.AddFrame(new Sprite(_spriteSheet, 162, 261, 13, 22));
        walkUp.AddFrame(new Sprite(_spriteSheet, 210, 262, 13, 21));
        walkUp.AddFrame(new Sprite(_spriteSheet, 258, 263, 13, 20));

        var walkRight = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 17, 212, 15, 23));
        walkRight.AddFrame(new Sprite(_spriteSheet, 65, 213, 15, 22));
        walkRight.AddFrame(new Sprite(_spriteSheet, 113, 214, 15, 21));
        walkRight.AddFrame(new Sprite(_spriteSheet, 161, 212, 15, 23));
        walkRight.AddFrame(new Sprite(_spriteSheet, 209, 213, 15, 22));
        walkRight.AddFrame(new Sprite(_spriteSheet, 257, 214, 15, 21));

        var walkLeft = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 17, 212, 15, 23, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 65, 213, 15, 22, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 113, 214, 15, 21, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 161, 212, 15, 23, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 209, 213, 15, 22, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(_spriteSheet, 257, 214, 15, 21, SpriteEffects.FlipHorizontally));

        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Down, walkDown);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Up, walkUp);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Right, walkRight);
        _animColl.AddAnimation(CreatureState.Walking, SpriteDirection.Left, walkLeft);
    }

    private void InitializeAttackAnimations()
    {
        var attackDown = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackDown.AddFrame(new Sprite(_spriteSheet, 15, 311, 16, 20));
        attackDown.AddFrame(new Sprite(_spriteSheet, 65, 310, 20, 26));
        attackDown.AddFrame(new Sprite(_spriteSheet, 114, 311, 19, 21));
        attackDown.AddFrame(new Sprite(_spriteSheet, 162, 312, 13, 19));

        var attackRight = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackRight.AddFrame(new Sprite(_spriteSheet, 19, 359, 16, 20));
        attackRight.AddFrame(new Sprite(_spriteSheet, 56, 358, 34, 23));
        attackRight.AddFrame(new Sprite(_spriteSheet, 107, 358, 20, 21));
        attackRight.AddFrame(new Sprite(_spriteSheet, 161, 360, 15, 19));

        var attackLeft = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackLeft.AddFrame(new Sprite(_spriteSheet, 19, 359, 16, 20, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(_spriteSheet, 56, 358, 34, 23, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(_spriteSheet, 107, 358, 20, 21, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(_spriteSheet, 161, 360, 15, 19, SpriteEffects.FlipHorizontally));

        var attackUp = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackUp.AddFrame(new Sprite(_spriteSheet, 18, 407, 17, 20));
        attackUp.AddFrame(new Sprite(_spriteSheet, 59, 406, 22, 21));
        attackUp.AddFrame(new Sprite(_spriteSheet, 108, 407, 20, 20));
        attackUp.AddFrame(new Sprite(_spriteSheet, 162, 408, 13, 19));

        _animColl.AddAnimation(CreatureState.Attacking, SpriteDirection.Down, attackDown);
        _animColl.AddAnimation(CreatureState.Attacking, SpriteDirection.Right, attackRight);
        _animColl.AddAnimation(CreatureState.Attacking, SpriteDirection.Left, attackLeft);
        _animColl.AddAnimation(CreatureState.Attacking, SpriteDirection.Up, attackUp);
    }

    private void SetAnimationDirection(Vector2 direction)
    {
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
        //if we're currently attacking and mid animation, we can't attack-cancel to run
        if (IsAttacking)
            return false;

        //if we were just idilng, we need to build up speed. no speed buildup needed for attacks
        if (State == CreatureState.Idling)
            _velocity = INITIAL_VELOCITY;

        State = CreatureState.Walking;
        SetAnimationDirection(direction);

        //increase our velocity by our acceleration up to our max velocity
        _velocity += ACCELERATION * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _velocity = Math.Min(_velocity, MAX_VELOCITY);

        //diagonalAdj helps us accomodate moving on two axis, or we'd move super fast on the diag
        var diagonalAdj = direction.X != 0 && direction.Y != 0 ? 1.5f : 1f;

        //full eq for newPos = currentPos + (velocity * acceleration * gameTime * direction)
        var posX = Position.X + (_velocity / diagonalAdj) * direction.X;
        var posY = Position.Y + (_velocity / diagonalAdj) * direction.Y;

        //simple bounding for now to prevent us from going off screen
        var currentSprite = _animColl.GetAnimation(State, AnimDirection).CurrentSprite;
        if (posX < 0 || posX > _map.PixelWidth - currentSprite.Width)
            posX = Position.X;

        if (posY < 0 || posY > _map.PixelHeight - currentSprite.Height)
            posY = Position.Y;

        //saving in case actually doing this movement would cause us to colide
        var oldPosX = Position.X;
        var oldPosY = Position.Y;

        //if we can walk, what's our new direction and position?
        Direction = direction;
        Position = new Vector2(posX, posY);
        
        var doesCollide = _map.DoesCollide(CollisionBox, direction, 1);
        if (doesCollide)
        {
            Debug.WriteLine("COLLISION!");
            //reset position if we would collide by apply this position change
            Position = new Vector2(oldPosX, oldPosY);
        }
        else
        {
            Debug.WriteLine("no collision!");
        }

        return true;
    }

    public void Idle()
    {
        //if we're currently attacking and mid animation, we can't go to idle
        if (IsAttacking)
            return;

        State = CreatureState.Idling;
    }

    public bool Attack(Vector2 direction, GameTime gameTime)
    {
        SetAnimationDirection(direction);
        State = CreatureState.Attacking;

        //we need to make sure to start playing the animation in case we attacked previously and it'd be ended
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Play();

        Direction = direction;
        return true;
    }

    public bool Die()
    {
        //TODO
        return false;
    }

    public bool ReceiveDamage(float damageValue)
    {
        //TODO
        return false;
    }

    public void Update(GameTime gameTime)
    {
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //This will get moved into some DebugHelper or something, it allows me to see the player's actual collisionbox
        //drawn on top of the sprite
        // if (false)
        // {
        //     var rect = CollisionBox;
        //
        //     var boxTexture = new Texture2D(_spriteSheet.GraphicsDevice, rect.Width, rect.Height);
        //     var boxData = new Color[rect.Width * rect.Height];
        //
        //     for (int i = 0; i < boxData.Length; i++)
        //     {
        //         boxData[i] = Color.Yellow;
        //     }
        //
        //     boxTexture.SetData(boxData);
        //     spriteBatch.Draw(boxTexture, new Vector2(rect.X, rect.Y), new Rectangle(0, 0, rect.Width, rect.Height),
        //         Color.White, 0f,
        //         Vector2.Zero, 1, SpriteEffects.None, (Position.Y + rect.Height + 100) / GULPGame.WINDOW_HEIGHT);
        // }
        
        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position);
    }
}
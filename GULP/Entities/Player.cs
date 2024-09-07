using System;
using GULP.Graphics.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Player : IEntity, ICreature
{
    private const float ACCELERATION = 1.0f;
    private const float MAX_VELOCITY = 2.0f;
    private const float INITIAL_VELOCITY = 1.0f;

    //animation frame durations
    private const float ANIM_IDLE_FRAME_DURATION = 1 / 4f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 15f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 10f;

    private const float DAMAGE_DEALING_FRAME = 1;

    //spritesheet and animations
    private readonly Texture2D _spriteSheet;
    private SpriteAnimationColl _animColl;

    private float _velocity;

    public int DrawOrder => 10; //above the ground at 0
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }

    public bool IsDealingDamage => IsAttacking &&
                                   Math.Abs(_animColl.GetAnimation(State, PlayerDirection).CurrentFrame -
                                            DAMAGE_DEALING_FRAME) < 0.01;

    public Rectangle CollisionBox { get; }
    public Direction PlayerDirection { get; set; }

    public bool IsAttacking => //attacking is going to be a "heavy" action, we can't cancel it
        State == CreatureState.Attacking && _animColl.GetAnimation(State, PlayerDirection).IsPlaying;

    public Player(Texture2D spriteSheet, Vector2 position)
    {
        _spriteSheet = spriteSheet;

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

        _animColl.AddAnimation(CreatureState.Idling, Direction.Down, idleDown);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Up, idleUp);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Left, idleLeft);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Right, idleRight);
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

        _animColl.AddAnimation(CreatureState.Walking, Direction.Down, walkDown);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Up, walkUp);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Right, walkRight);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Left, walkLeft);
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

        _animColl.AddAnimation(CreatureState.Attacking, Direction.Down, attackDown);
        _animColl.AddAnimation(CreatureState.Attacking, Direction.Right, attackRight);
        _animColl.AddAnimation(CreatureState.Attacking, Direction.Left, attackLeft);
        _animColl.AddAnimation(CreatureState.Attacking, Direction.Up, attackUp);
    }

    private void SetDirectionFromInput(float x, float y)
    {
        //if we're moving left or right (non-diag or diag), we should face that way
        //else just face up or down
        if (x < 0)
            PlayerDirection = Direction.Left;
        else if (x > 0)
            PlayerDirection = Direction.Right;
        else if (y < 0)
            PlayerDirection = Direction.Up;
        else if (y > 0)
            PlayerDirection = Direction.Down;
    }

    public bool Walk(float x, float y, GameTime gameTime)
    {
        //if we're currently attacking and mid animation, we can't attack-cancel to run
        if (IsAttacking)
            return false;

        //if we were just idilng, we need to build up speed. no speed buildup needed for attacks
        if (State == CreatureState.Idling)
            _velocity = INITIAL_VELOCITY;

        State = CreatureState.Walking;
        SetDirectionFromInput(x, y);

        //increase our velocity by our acceleration up to our max velocity
        _velocity += ACCELERATION * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _velocity = Math.Min(_velocity, MAX_VELOCITY);

        //diagonalAdj helps us accomodate moving on two axis, or we'd move super fast on the diag
        var diagonalAdj = x != 0 && y != 0 ? 1.5f : 1f;
        var posX = Position.X + (_velocity / diagonalAdj) * x;
        var posY = Position.Y + (_velocity / diagonalAdj) * y;

        Position = new Vector2(posX, posY);
        return true;
    }

    public void Idle()
    {
        //if we're currently attacking and mid animation, we can't go to idle
        if (IsAttacking)
            return;

        State = CreatureState.Idling;
    }

    public bool Attack(float x, float y, GameTime gameTime)
    {
        SetDirectionFromInput(x, y);
        State = CreatureState.Attacking;

        //we need to make sure to start playing the animation in case we attacked previously and it'd be ended
        var animation = _animColl.GetAnimation(State, PlayerDirection);
        animation.Play();

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
        var animation = _animColl.GetAnimation(State, PlayerDirection);
        animation.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var animation = _animColl.GetAnimation(State, PlayerDirection);
        animation.Draw(spriteBatch, Position);
    }
}
using GULP.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Player : IEntity, ICreature
{
    private const float ANIM_IDLE_FRAME_DURATION = 1 / 4f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 10f;

    //spritesheet and animations
    private readonly Texture2D _spriteSheet;
    private SpriteAnimationColl _animColl;

    public int DrawOrder { get; } = 10; //above the ground at 0
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }
    public bool IsDealingDamage { get; set; }
    public Rectangle CollisionBox { get; }
    public Direction PlayerDirection { get; set; }

    public Player(Texture2D spriteSheet, Vector2 position)
    {
        _spriteSheet = spriteSheet;

        //values on initialization
        Position = position;
        Health = 100;
        State = CreatureState.Idling;

        _animColl = new SpriteAnimationColl();
        InitializeIdleAnimations();
        InitializeWalkAnimations();
    }

    private void InitializeIdleAnimations()
    {
        var idleDown = new SpriteAnimation();
        idleDown.AddFrame(new Sprite(_spriteSheet, 18, 22, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 66, 22, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 114, 22, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 162, 23, 13, 20), ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 210, 23, 13, 20), ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(_spriteSheet, 258, 23, 13, 20), ANIM_IDLE_FRAME_DURATION);

        var idleUp = new SpriteAnimation();
        idleUp.AddFrame(new Sprite(_spriteSheet, 18, 118, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 66, 118, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 114, 118, 13, 21), ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 162, 119, 13, 20), ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 210, 119, 13, 20), ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(_spriteSheet, 258, 119, 13, 20), ANIM_IDLE_FRAME_DURATION);

        var idleLeft = new SpriteAnimation();
        idleLeft.AddFrame(new Sprite(_spriteSheet, 17, 70, 15, 21, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 65, 70, 15, 21, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 113, 70, 15, 21, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 161, 71, 15, 20, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 209, 71, 15, 20, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(_spriteSheet, 257, 71, 15, 20, SpriteEffects.FlipHorizontally),
            ANIM_IDLE_FRAME_DURATION);

        var idleRight = new SpriteAnimation();
        idleRight.AddFrame(new Sprite(_spriteSheet, 17, 70, 15, 21), ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 65, 70, 15, 21), ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 113, 70, 15, 21), ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 161, 71, 15, 20), ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 209, 71, 15, 20), ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(_spriteSheet, 257, 71, 15, 20), ANIM_IDLE_FRAME_DURATION);

        _animColl.AddAnimation(CreatureState.Idling, Direction.Down, idleDown);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Up, idleUp);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Left, idleLeft);
        _animColl.AddAnimation(CreatureState.Idling, Direction.Right, idleRight);
    }

    private void InitializeWalkAnimations()
    {
        var walkDown = new SpriteAnimation();
        walkDown.AddFrame(new Sprite(_spriteSheet, 18, 164, 13, 23), ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 66, 165, 13, 22), ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 114, 166, 13, 21), ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 162, 164, 13, 23), ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 210, 165, 13, 22), ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(_spriteSheet, 258, 166, 13, 21), ANIM_WALK_FRAME_DURATION);

        var walkUp = new SpriteAnimation();
        walkUp.AddFrame(new Sprite(_spriteSheet, 18, 261, 13, 22), ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 66, 262, 13, 21), ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 114, 263, 13, 20), ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 162, 261, 13, 22), ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 210, 262, 13, 21), ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(_spriteSheet, 258, 263, 13, 20), ANIM_WALK_FRAME_DURATION);

        var walkRight = new SpriteAnimation();
        walkRight.AddFrame(new Sprite(_spriteSheet, 17, 212, 15, 23), ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 65, 213, 15, 22), ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 113, 214, 15, 21), ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 161, 212, 15, 23), ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 209, 213, 15, 22), ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(_spriteSheet, 257, 214, 15, 21), ANIM_WALK_FRAME_DURATION);

        var walkLeft = new SpriteAnimation();
        walkLeft.AddFrame(new Sprite(_spriteSheet, 17, 212, 15, 23, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 65, 213, 15, 22, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 113, 214, 15, 21, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 161, 212, 15, 23, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 209, 213, 15, 22, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(_spriteSheet, 257, 214, 15, 21, SpriteEffects.FlipHorizontally),
            ANIM_WALK_FRAME_DURATION);

        _animColl.AddAnimation(CreatureState.Walking, Direction.Down, walkDown);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Up, walkUp);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Right, walkRight);
        _animColl.AddAnimation(CreatureState.Walking, Direction.Left, walkLeft);
    }

    public bool Walk(float x, float y, GameTime gameTime)
    {
        State = CreatureState.Walking;

        //first change our direction
        if (x < 0)
            PlayerDirection = Direction.Left;
        else if (x > 0)
            PlayerDirection = Direction.Right;
        else if (y < 0)
            PlayerDirection = Direction.Up;
        else
            PlayerDirection = Direction.Down;

        //now apply changes to our position
        float speed = 60; //for now
        if (x != 0 && y != 0)
            speed /= 1.5f;
        
        float posX = Position.X + speed * x * (float)gameTime.ElapsedGameTime.TotalSeconds;
        float posY = Position.Y + speed * y * (float)gameTime.ElapsedGameTime.TotalSeconds;

        Position = new Vector2(posX, posY);
        return true;
    }

    public void Idle()
    {
        State = CreatureState.Idling;
    }

    public bool Attack(float x, float y)
    {
        //TODO
        return false;
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
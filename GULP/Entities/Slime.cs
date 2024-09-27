using System;
using System.Diagnostics;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Slime : Creature
{
    //TODO this is temporary until we have an EnemyManager
    private readonly Player _player;
    protected override float Acceleration => 0f;
    protected override float MaxVelocity => 2f;
    protected override float InitialVelocity => 2f;

    private const int COLLISION_BOX_WIDTH = 12;
    private const int COLLISION_BOX_HEIGHT = 10;
    private const float ANIM_IDLE_FRAME_DURATION = 1 / 5f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 8f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 8f;

    //TODO clean these up, timePerDecision can obviously be a const
    private float _timePerDecision = .2f; //basically the slime's reaction time
    private float _timeSinceLastDecision = float.MaxValue; //when did we last make a decision?
    private CreatureState _lastDecision; //what was our last decision?

    public Slime(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager, Player player) : base(
        spriteSheet,
        position, map, entityManager)
    {
        _player = player;
        InitializeIdleAnimations();
        InitializeWalkAnimations();
        InitializeAttackAnimations();

        //Debugging Texture
        CreateDebugTextures();
    }

    private void CreateDebugTextures()
    {
        var cBoxText = new Texture2D(SpriteSheet.GraphicsDevice, COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);
        var cBoxData = new Color[COLLISION_BOX_WIDTH * COLLISION_BOX_HEIGHT];

        for (int i = 0; i < cBoxData.Length; i++)
        {
            cBoxData[i] = Color.Cyan;
        }

        cBoxText.SetData(cBoxData);
        CollisionBoxTexture = cBoxText;
    }

    private void InitializeIdleAnimations()
    {
        var idleDown = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(SpriteSheet, 8, 12, 16, 12));
        idleDown.AddFrame(new Sprite(SpriteSheet, 40, 12, 16, 12));
        idleDown.AddFrame(new Sprite(SpriteSheet, 72, 13, 16, 11));
        idleDown.AddFrame(new Sprite(SpriteSheet, 104, 13, 16, 11));

        var idleUp = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(SpriteSheet, 8, 76, 16, 12));
        idleUp.AddFrame(new Sprite(SpriteSheet, 40, 76, 16, 12));
        idleUp.AddFrame(new Sprite(SpriteSheet, 72, 77, 16, 11));
        idleUp.AddFrame(new Sprite(SpriteSheet, 104, 77, 16, 11));

        var idleLeft = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(SpriteSheet, 8, 44, 16, 12, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 40, 44, 16, 12, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 72, 45, 16, 11, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 104, 45, 16, 11, SpriteEffects.FlipHorizontally));

        var idleRight = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(SpriteSheet, 8, 44, 16, 12));
        idleRight.AddFrame(new Sprite(SpriteSheet, 40, 44, 16, 12));
        idleRight.AddFrame(new Sprite(SpriteSheet, 72, 45, 16, 11));
        idleRight.AddFrame(new Sprite(SpriteSheet, 104, 45, 16, 11));

        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Down, idleDown);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Up, idleUp);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Left, idleLeft);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Right, idleRight);
    }

    private void InitializeWalkAnimations()
    {
        var walkDown = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(SpriteSheet, 6, 110, 20, 10));
        walkDown.AddFrame(new Sprite(SpriteSheet, 42, 104, 12, 16));
        walkDown.AddFrame(new Sprite(SpriteSheet, 73, 102, 14, 18));
        walkDown.AddFrame(new Sprite(SpriteSheet, 104, 105, 16, 15));
        walkDown.AddFrame(new Sprite(SpriteSheet, 136, 108, 16, 12));
        walkDown.AddFrame(new Sprite(SpriteSheet, 168, 109, 16, 11));

        var walkUp = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(SpriteSheet, 6, 174, 20, 10));
        walkUp.AddFrame(new Sprite(SpriteSheet, 42, 168, 12, 16));
        walkUp.AddFrame(new Sprite(SpriteSheet, 73, 166, 14, 18));
        walkUp.AddFrame(new Sprite(SpriteSheet, 104, 169, 16, 15));
        walkUp.AddFrame(new Sprite(SpriteSheet, 136, 172, 16, 12));
        walkUp.AddFrame(new Sprite(SpriteSheet, 168, 173, 16, 11));

        var walkRight = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(SpriteSheet, 6, 142, 20, 12));
        walkRight.AddFrame(new Sprite(SpriteSheet, 42, 136, 12, 16));
        walkRight.AddFrame(new Sprite(SpriteSheet, 74, 134, 12, 18));
        walkRight.AddFrame(new Sprite(SpriteSheet, 104, 137, 16, 15));
        walkRight.AddFrame(new Sprite(SpriteSheet, 136, 140, 16, 12));
        walkRight.AddFrame(new Sprite(SpriteSheet, 168, 141, 16, 11));

        var walkLeft = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(SpriteSheet, 6, 142, 20, 10, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 42, 136, 12, 16, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 74, 134, 12, 18, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 104, 137, 16, 15, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 136, 140, 16, 12, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 168, 141, 16, 11, SpriteEffects.FlipHorizontally));

        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Down, walkDown);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Up, walkUp);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Right, walkRight);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Left, walkLeft);
    }

    private void InitializeAttackAnimations()
    {
        //todo finish this, dummy
        var attackDown = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackDown.AddFrame(new Sprite(SpriteSheet, 7, 206, 18, 10));
        attackDown.AddFrame(new Sprite(SpriteSheet, 39, 205, 18, 11));
        attackDown.AddFrame(new Sprite(SpriteSheet, 73, 196, 14, 20));
        attackDown.AddFrame(new Sprite(SpriteSheet, 105, 194, 14, 22));
        attackDown.AddFrame(new Sprite(SpriteSheet, 137, 198, 14, 18));
        attackDown.AddFrame(new Sprite(SpriteSheet, 167, 206, 18, 10));

        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Down, attackDown);
    }

    public override Rectangle GetCollisionBox(Vector2 position)
    {
        //TODO clean this up so slimes can slide better when rubbing against surfaces, it's quite bad on fences
        //get our max height and width sprites for the current animation
        var sprite = AnimationCollection.GetAnimation(State, AnimDirection).CurrentSprite;

        //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
        //towards the feet on the y-axis
        var rect = new Rectangle((int)Math.Floor(position.X) + sprite.Width / 2 - COLLISION_BOX_WIDTH / 2,
            (int)Math.Floor(position.Y) + (int)(sprite.Height / 2) - COLLISION_BOX_HEIGHT / 2,
            COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);

        return rect;
    }

    public override Rectangle GetAttackBox(Vector2 position)
    {
        return new Rectangle();
    }

    public override bool Attack(GameTime gameTime)
    {
        //TODO actually attack...
        var previousState = State;
        State = CreatureState.Attacking;

        //we need to make sure to start playing the animation in case we attacked previously and it'd be ended
        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Play();
        return true;
    }

    public override bool Die()
    {
        //TODO
        return false;
    }

    public override bool ReceiveDamage(float damageValue)
    {
        //TODO
        return false;
    }

    public override void DrawAttackBox(SpriteBatch spriteBatch)
    {
        //TODO
        return;
    }

    public override void Update(GameTime gameTime)
    {
        //todo Testing Player Tracking for now
        
        //increment time since last decision
        _timeSinceLastDecision += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        //if we've had enough time to react
        if (_timeSinceLastDecision > _timePerDecision)
        {
            Debug.WriteLine("Decision Time");
            _timeSinceLastDecision = 0f; //reset
            
            //calculate distance and directional vector
            var dist = Vector2.Distance(_player.Position, Position);
            var direction = _player.Position - Position;
            direction.Normalize();
            direction.Round();

            //if we're within 2 tiles, we attack
            if (dist < 32) //two tiles
            {
                Debug.WriteLine("ATTACK: " + direction);
                Attack(direction, gameTime);
                _lastDecision = CreatureState.Attacking;
            }
            else
            {
                //else let's walk toward the player
                Debug.WriteLine("WALK: " + direction);
                Walk(direction, gameTime);
                _lastDecision = CreatureState.Walking;
            }
        }
        else
        {
            //we haven't had time to change our action, continue performing it
            Debug.WriteLine("No Decision");
            switch (_lastDecision)
            {
                case CreatureState.Walking:
                    Debug.WriteLine("WALK: " + Direction);
                    Walk(Direction, gameTime);
                    break;
                case CreatureState.Attacking:
                    Attack(Direction, gameTime);
                    Debug.WriteLine("ATTACK: " + Direction);
                    break;
                default:
                    Idle();
                    break;
            }
        }

        base.Update(gameTime);
    }
}
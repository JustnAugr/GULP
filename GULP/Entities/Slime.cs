using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Slime : Creature
{
    private const float TIME_PER_DECISION = .2f; //basically the slime's reaction time

    private const int COLLISION_BOX_WIDTH = 11;
    private const int COLLISION_BOX_HEIGHT = 7;

    //vertical as in our direction is Y != 0
    private const int VERTICAL_ATTACK_BOX_WIDTH = 18;
    private const int VERTICAL_ATTACK_BOX_HEIGHT = 10;
    private const int HORIZONTAL_ATTACK_BOX_WIDTH = 4;
    private const int HORIZONTAL_ATTACK_BOX_HEIGHT = 11;

    private const float ANIM_IDLE_FRAME_DURATION = 1 / 5f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 8f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 8f;
    private const int COLLISION_BOX_X_OFFSET = 3;
    private const int COLLISION_BOX_Y_OFFSET = 3;

    //Creature Override Constants
    protected override float Acceleration => 0f;
    protected override float MaxVelocity => 2f;
    protected override float InitialVelocity => 2f;

    private readonly Player _player;

    private float _timeSinceLastDecision = float.MaxValue; //when did we last make a decision?
    private CreatureState _lastDecision; //what was our last decision?

    public Slime(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager, Player player) : base(
        spriteSheet,
        position, map, entityManager)
    {
        Health = 100; 

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

        for (var i = 0; i < cBoxData.Length; i++)
        {
            cBoxData[i] = Color.Cyan;
        }

        cBoxText.SetData(cBoxData);
        CollisionBoxTexture = cBoxText;

        var hABoxText = new Texture2D(SpriteSheet.GraphicsDevice, HORIZONTAL_ATTACK_BOX_WIDTH,
            HORIZONTAL_ATTACK_BOX_HEIGHT);
        var hABoxData = new Color[HORIZONTAL_ATTACK_BOX_WIDTH * HORIZONTAL_ATTACK_BOX_HEIGHT];

        for (var i = 0; i < hABoxData.Length; i++)
        {
            hABoxData[i] = Color.Red;
        }

        hABoxText.SetData(hABoxData);
        HorizontalBoxTexture = hABoxText;

        var vABoxText = new Texture2D(SpriteSheet.GraphicsDevice, VERTICAL_ATTACK_BOX_WIDTH,
            VERTICAL_ATTACK_BOX_HEIGHT);
        var vABoxData = new Color[VERTICAL_ATTACK_BOX_WIDTH * VERTICAL_ATTACK_BOX_HEIGHT];

        for (var i = 0; i < vABoxData.Length; i++)
        {
            vABoxData[i] = Color.Red;
        }

        vABoxText.SetData(vABoxData);
        VerticalBoxTexture = vABoxText;
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
        //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
        //towards the feet on the y-axis
        var rect = new Rectangle(
            (int)Math.Floor(position.X) + COLLISION_BOX_X_OFFSET,
            (int)Math.Floor(position.Y),
            COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);

        //bit of an adjustment for when we're walking upwards
        //not doing this by default because the variation in height when going left/right is crazy considering he bobs
        if (State is CreatureState.Walking && AnimDirection is SpriteDirection.Up)
            rect.Y -= COLLISION_BOX_Y_OFFSET;

        return rect;
    }

    protected override Rectangle GetAttackBox(Vector2 position)
    {
        var collisionBox = GetCollisionBox(position);
        float posX = collisionBox.X;
        float posY = collisionBox.Y;

        if (Direction.X != 0)
        {
            posX += (int)Math.Floor((collisionBox.Width * 2f) / 3f);
        }
        else if (Direction.Y != 0)
        {
            posX = posX + collisionBox.Width / 2f - VERTICAL_ATTACK_BOX_WIDTH / 2f; //from middle
            posY += (int)Math.Floor(collisionBox.Height / 2f * Direction.Y); //move down 2/3 size of collisionBox
        }

        var attackBox = new Rectangle(
            (int)Math.Floor(posX),
            (int)Math.Floor(posY),
            Direction.X != 0 ? HORIZONTAL_ATTACK_BOX_WIDTH : VERTICAL_ATTACK_BOX_WIDTH,
            Direction.X != 0 ? HORIZONTAL_ATTACK_BOX_HEIGHT : VERTICAL_ATTACK_BOX_HEIGHT
        );

        return attackBox;
    }

    public override bool Die()
    {
        //TODO
        return false;
    }

    public override void DrawAttackBox(SpriteBatch spriteBatch)
    {
        var attackBox = GetAttackBox();
        spriteBatch.Draw(Direction.X != 0 ? HorizontalBoxTexture : VerticalBoxTexture,
            new Vector2(attackBox.X, attackBox.Y),
            new Rectangle(0, 0, attackBox.Width, attackBox.Height),
            Color.White * .5f, 0f,
            Vector2.Zero, 1, SpriteEffects.None, 1f);
    }

    public override void Update(GameTime gameTime)
    {
        //todo Testing Player Tracking for now
        
        //increment time since last decision
        _timeSinceLastDecision += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        //if we've had enough time to react
        if (_timeSinceLastDecision > TIME_PER_DECISION)
        {
            //Debug.WriteLine("Decision Time");
            _timeSinceLastDecision = 0f; //reset
            
            //calculate distance and directional vector
            var dist = Vector2.Distance(_player.Position, Position);
            var direction = _player.Position - Position;
            direction.Normalize();
            direction.Round();
        
            //if we're within 2 tiles, we attack
            if (dist < 32) //two tiles
            {
                //Debug.WriteLine("ATTACK: " + direction);
                //Attack(direction, gameTime);
                Idle(gameTime);
                _lastDecision = CreatureState.Attacking;
            }
            else
            {
                //else let's walk toward the player
                //Debug.WriteLine("WALK: " + direction);
                Walk(direction, gameTime);
                _lastDecision = CreatureState.Walking;
            }
        }
        else
        {
            //we haven't had time to change our action, continue performing it
            //Debug.WriteLine("No Decision");
            switch (_lastDecision)
            {
                case CreatureState.Walking:
                    //Debug.WriteLine("WALK: " + Direction);
                    Walk(Direction, gameTime);
                    break;
                case CreatureState.Attacking:
                    //Attack(Direction, gameTime);
                    Idle(gameTime);
                    //Debug.WriteLine("ATTACK: " + Direction);
                    break;
                default:
                    Idle(gameTime);
                    break;
            }
        }

        base.Update(gameTime);
    }
}
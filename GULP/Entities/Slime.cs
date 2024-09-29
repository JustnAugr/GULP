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
    private const float TIME_PER_DECISION = .35f; //basically the slime's reaction time

    private const int COLLISION_BOX_WIDTH = 11;
    private const int COLLISION_BOX_HEIGHT = 7;

    private const int UPDOWN_ATTACK_BOX_WIDTH = 20;
    private const int UPDOWN_ATTACK_BOX_HEIGHT = 15;

    private const int LEFTRIGHT_ATTACK_BOX_WIDTH = 18;
    private const int LEFTRIGHT_ATTACK_BOX_HEIGHT = 17;

    private const float ANIM_IDLE_FRAME_DURATION = 1 / 5f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 8f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 8f;
    private const float ANIM_DEATH_FRAME_DURATION = 1 / 8f;

    private const int COLLISION_BOX_X_OFFSET = 3;
    private const int COLLISION_BOX_Y_OFFSET = 3;

    private const int ATTACK_MVMT_FRAME_START = 1;
    private const int ATTACK_MVMT_FRAME_END = 3;
    private const float ATTACK_MVMT_VELOCITY_MULT = 1.25f;

    private const int DAMAGE_DEALING_FRAME = 5;
    private const float BASE_HEALTH = 100;
    private const float DAMAGE_VALUE = 25f;

    //Creature Override Constants
    protected override float Acceleration => 0f;
    protected override float MaxVelocity => 2f;
    protected override float InitialVelocity => 2f;

    private readonly Player _player;

    private int AttackDistanceThreshold => Map.TileWidth * 4;
    private float _timeSinceLastDecision = float.MaxValue; //when did we last make a decision?
    private CreatureState _lastDecision; //what was our last decision?
    private Vector2 _lastDecidedPosition;

    public Slime(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager, Player player) : base(
        spriteSheet,
        position, map, entityManager)
    {
        Health = BASE_HEALTH;

        _player = player;
        InitializeIdleAnimations();
        InitializeWalkAnimations();
        InitializeAttackAnimations();
        InitializeDeathAnimations();

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

        var hABoxText = new Texture2D(SpriteSheet.GraphicsDevice, LEFTRIGHT_ATTACK_BOX_WIDTH,
            LEFTRIGHT_ATTACK_BOX_HEIGHT);
        var hABoxData = new Color[LEFTRIGHT_ATTACK_BOX_WIDTH * LEFTRIGHT_ATTACK_BOX_HEIGHT];

        for (var i = 0; i < hABoxData.Length; i++)
        {
            hABoxData[i] = Color.Red;
        }

        hABoxText.SetData(hABoxData);
        HorizontalBoxTexture = hABoxText;

        var vABoxText = new Texture2D(SpriteSheet.GraphicsDevice, UPDOWN_ATTACK_BOX_WIDTH,
            UPDOWN_ATTACK_BOX_HEIGHT);
        var vABoxData = new Color[UPDOWN_ATTACK_BOX_WIDTH * UPDOWN_ATTACK_BOX_HEIGHT];

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
        var attackDown = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false, normalizeHeight: true);
        attackDown.AddFrame(new Sprite(SpriteSheet, 7, 206, 18, 12));
        attackDown.AddFrame(new Sprite(SpriteSheet, 39, 205, 18, 12));
        attackDown.AddFrame(new Sprite(SpriteSheet, 73, 196, 14, 20));
        attackDown.AddFrame(new Sprite(SpriteSheet, 105, 194, 14, 22));
        attackDown.AddFrame(new Sprite(SpriteSheet, 137, 198, 14, 18));
        attackDown.AddFrame(new Sprite(SpriteSheet, 167, 206, 18, 12));
        attackDown.AddFrame(new Sprite(SpriteSheet, 199, 207, 18, 12));

        var attackRight = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackRight.AddFrame(new Sprite(SpriteSheet, 7, 238, 18, 12));
        attackRight.AddFrame(new Sprite(SpriteSheet, 40, 237, 17, 12));
        attackRight.AddFrame(new Sprite(SpriteSheet, 74, 228, 12, 20));
        attackRight.AddFrame(new Sprite(SpriteSheet, 106, 226, 12, 22));
        attackRight.AddFrame(new Sprite(SpriteSheet, 137, 230, 13, 18));
        attackRight.AddFrame(new Sprite(SpriteSheet, 168, 238, 16, 12));
        attackRight.AddFrame(new Sprite(SpriteSheet, 199, 239, 18, 12));

        var attackLeft = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackLeft.AddFrame(new Sprite(SpriteSheet, 7, 238, 18, 12, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 40, 237, 17, 12, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 74, 228, 12, 20, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 106, 226, 12, 22, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 137, 230, 13, 18, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 168, 238, 16, 12, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 199, 239, 18, 12, SpriteEffects.FlipHorizontally));

        var attackUp = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackUp.AddFrame(new Sprite(SpriteSheet, 7, 270, 18, 12));
        attackUp.AddFrame(new Sprite(SpriteSheet, 39, 269, 18, 12));
        attackUp.AddFrame(new Sprite(SpriteSheet, 73, 260, 14, 20));
        attackUp.AddFrame(new Sprite(SpriteSheet, 105, 258, 14, 22));
        attackUp.AddFrame(new Sprite(SpriteSheet, 137, 262, 14, 18));
        attackUp.AddFrame(new Sprite(SpriteSheet, 167, 270, 18, 12));
        attackUp.AddFrame(new Sprite(SpriteSheet, 199, 271, 18, 12));

        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Down, attackDown);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Right, attackRight);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Left, attackLeft);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Up, attackUp);
    }

    private void InitializeDeathAnimations()
    {
        var death = new SpriteAnimation(ANIM_DEATH_FRAME_DURATION, shouldLoop: false);
        death.AddFrame(new Sprite(SpriteSheet, 9, 394, 14, 14));
        death.AddFrame(new Sprite(SpriteSheet, 41, 389, 13, 19));
        death.AddFrame(new Sprite(SpriteSheet, 72, 387, 14, 20));
        death.AddFrame(new Sprite(SpriteSheet, 104, 390, 17, 18));
        death.AddFrame(new Sprite(SpriteSheet, 135, 402, 18, 6));

        //all the same animation given we only have 1 death animation
        AnimationCollection.AddAnimation(CreatureState.Dead, SpriteDirection.Down, death);
        AnimationCollection.AddAnimation(CreatureState.Dead, SpriteDirection.Right, death);
        AnimationCollection.AddAnimation(CreatureState.Dead, SpriteDirection.Left, death);
        AnimationCollection.AddAnimation(CreatureState.Dead, SpriteDirection.Up, death);
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

        if (AnimDirection is SpriteDirection.Right or SpriteDirection.Left)
        {
            posY = posY + collisionBox.Height / 2f - LEFTRIGHT_ATTACK_BOX_HEIGHT / 2f;

            if (AnimDirection is SpriteDirection.Left)
                posX = collisionBox.X + COLLISION_BOX_WIDTH - LEFTRIGHT_ATTACK_BOX_WIDTH;
        }
        else if (AnimDirection is SpriteDirection.Up or SpriteDirection.Down)
        {
            posX = posX + collisionBox.Width / 2f - UPDOWN_ATTACK_BOX_WIDTH / 2f; //from middle
            posY += collisionBox.Height / 2f; //move down 2/3 size of collisionBox

            if (AnimDirection is SpriteDirection.Up)
                posY -= UPDOWN_ATTACK_BOX_HEIGHT;
        }

        var attackBox = new Rectangle(
            (int)Math.Floor(posX),
            (int)Math.Floor(posY),
            Direction.X != 0 ? LEFTRIGHT_ATTACK_BOX_WIDTH : UPDOWN_ATTACK_BOX_WIDTH,
            Direction.X != 0 ? LEFTRIGHT_ATTACK_BOX_HEIGHT : UPDOWN_ATTACK_BOX_HEIGHT
        );

        return attackBox;
    }

    public override bool Die()
    {
        //todo move to creature once I add the player death animation...
        State = CreatureState.Dead;
        AnimationCollection.GetAnimation(State, AnimDirection).Play();
        return true;
    }

    protected override void DrawAttackBox(SpriteBatch spriteBatch)
    {
        var attackBox = GetAttackBox();
        spriteBatch.Draw(Direction.X != 0 ? HorizontalBoxTexture : VerticalBoxTexture,
            new Vector2(attackBox.X, attackBox.Y),
            new Rectangle(0, 0, attackBox.Width, attackBox.Height),
            Color.White * .5f, 0f,
            Vector2.Zero, 1, SpriteEffects.None, 1f);
    }

    private void AttackAndMove(Vector2 direction, GameTime gameTime)
    {
        //if we're already attacking, don't trigger a new one - it's in progress already
        if (!IsAttacking)
            Attack(direction, gameTime);

        //we only want to move on the middle 3 frames of the Slime as these are the ones when we're in the air
        //else we'll slide on the down ones which is weird
        var currentFrame = AnimationCollection.GetAnimation(State, AnimDirection).CurrentFrame;
        if (IsAttacking && currentFrame is >= ATTACK_MVMT_FRAME_START and <= ATTACK_MVMT_FRAME_END)
            Move(direction, InitialVelocity * ATTACK_MVMT_VELOCITY_MULT, gameTime);
    }

    protected override void TryDealDamage(GameTime gameTime)
    {
        //only apply damage at the end of our attack animation
        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        var isLastAttackFrame = animation.CurrentFrame == DAMAGE_DEALING_FRAME;
        if (isLastAttackFrame && State is CreatureState.Attacking)
        {
            DealDamage(gameTime, DAMAGE_VALUE);
            ShouldDealDamage = false;
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        //todo move this into a separate function named something nice...
        if (State is not CreatureState.Dead)
        {
            //calculate accurate, real distance and directional vector, as from our feet
            //we'll use these for attacks so that we're very precise in attacking
            var player = new Vector2(_player.Position.X, _player.Position.Y + _player.Height);
            player.Round();
            var slime = new Vector2(Position.X, Position.Y + Height);
            slime.Round();
            var dist = Vector2.Distance(player, slime);
            var direction = player - slime;
            direction.Normalize();
            direction.Round();

            //calculate an approximated direction to the tile, to make our walking less choppy and prevent things like
            //rapid 1,1 -> -1,1 back and forth switches
            //we'll use this for walking to prevent pixel specific changes from making us look left/right randomly
            var playerTile = new Vector2((int)Math.Floor(_player.Position.X) / Map.TileWidth * Map.TileWidth,
                (int)Math.Floor(_player.Position.Y + _player.Height) / Map.TileHeight * Map.TileHeight);
            var slimeTile = new Vector2((int)Math.Floor(Position.X) / Map.TileWidth * Map.TileWidth,
                (int)Math.Floor(Position.Y + Height) / Map.TileHeight * Map.TileHeight);
            var tileDirection = playerTile - slimeTile;
            tileDirection.Normalize();
            tileDirection.Round();

            //increment time since last decision
            _timeSinceLastDecision += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //if we've had enough time to react
            if (_timeSinceLastDecision > TIME_PER_DECISION)
            {
                _timeSinceLastDecision = 0f; //reset

                //if we're within threshold, AttackAndMove to the player
                if (dist < AttackDistanceThreshold)
                {
                    _lastDecidedPosition = direction; //store our direction
                    _lastDecision = CreatureState.Attacking; //store our decision
                    AttackAndMove(direction, gameTime); //attack and move
                }
                //only if we've finished attacking, else we'll continue the attack and make a decision next time since we're busy
                else if (!IsAttacking)
                {
                    //else let's walk toward the player
                    _lastDecidedPosition = tileDirection;
                    _lastDecision = CreatureState.Walking;
                    Walk(tileDirection, gameTime);
                }
            }
            else
            {
                //we haven't had time to change our action, continue performing it
                switch (_lastDecision)
                {
                    case CreatureState.Walking:
                        Walk(_lastDecidedPosition, gameTime); //keep walking the same direction
                        break;
                    case CreatureState.Attacking:
                        //if we finished the attack we were performing, and the player is now far away
                        //we'll change to walking but in the same distance
                        //this is so we don't indefinitely attack at the player even as they get further and further away
                        if (!IsAttacking && dist > AttackDistanceThreshold)
                        {
                            Walk(_lastDecidedPosition, gameTime);
                        }
                        else
                        {
                            //else attack and move at where we last saw them!
                            AttackAndMove(_lastDecidedPosition, gameTime);
                        }

                        break;
                    default:
                        Idle(gameTime); //theoretically won't happen but a safe default
                        break;
                }
            }
        }
    }
}
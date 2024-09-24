using System;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Slime : Creature
{
    protected override float Acceleration => 0f;
    protected override float MaxVelocity => 2f;
    protected override float InitialVelocity => 2f;
    
    private const int COLLISION_BOX_WIDTH = 12;
    private const int COLLISION_BOX_HEIGHT = 10;

    public Slime(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager) : base(spriteSheet,
        position, map, entityManager)
    {
        InitializeIdleAnimations();
        InitializeWalkAnimation();

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
        var ANIM_IDLE_FRAME_DURATION = 1 / 5f;
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

    private void InitializeWalkAnimation()
    {
        var ANIM_WALK_FRAME_DURATION = 1 / 8f;

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

    public override Rectangle GetCollisionBox(Vector2 position)
    {
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
        return true;
    }

    public override bool Attack(Vector2 direction, GameTime gameTime)
    {
        throw new System.NotImplementedException();
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
}
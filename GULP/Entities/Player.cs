﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Player : Creature
{
    //movement constants
    protected override float Acceleration => 1f;
    protected override float MaxVelocity => 3f;
    protected override float InitialVelocity => 1f;

    //animation frame durations
    private const float ANIM_IDLE_FRAME_DURATION = 1 / 4f;
    private const float ANIM_WALK_FRAME_DURATION = 1 / 15f;
    private const float ANIM_ATTACK_FRAME_DURATION = 1 / 10f;

    //collision and attack box sizes
    private const int COLLISION_BOX_WIDTH = 15;
    private const int COLLISION_BOX_HEIGHT = 9;
    private const int VERTICAL_ATTACK_BOX_WIDTH = 22;
    private const int VERTICAL_ATTACK_BOX_HEIGHT = 10;
    private const int HORIZONTAL_ATTACK_BOX_WIDTH = 15;
    private const int HORIZONTAL_ATTACK_BOX_HEIGHT = 18;

    private Texture2D _verticalAttackBoxTexture;
    private Texture2D _horizontalAttackBoxTexture;

    public Player(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager) : base(spriteSheet,
        position, map, entityManager)
    {
        //values on initialization
        Health = 100;

        //initialize our animations and store them in a double keyed collection for easy lookup
        InitializeIdleAnimations();
        InitializeWalkAnimations();
        InitializeAttackAnimations();

        //Debug Rectangles
        CreateDebugRects();
    }

    private void CreateDebugRects()
    {
        //create a rectangle representing the collisionBox
        var cBoxText = new Texture2D(SpriteSheet.GraphicsDevice, COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);
        var cBoxData = new Color[COLLISION_BOX_WIDTH * COLLISION_BOX_HEIGHT];

        for (int i = 0; i < cBoxData.Length; i++)
        {
            cBoxData[i] = Color.Cyan;
        }

        cBoxText.SetData(cBoxData);
        CollisionBoxTexture = cBoxText;

        //and a rectangle for the vertical attack box
        var vABoxText = new Texture2D(SpriteSheet.GraphicsDevice, VERTICAL_ATTACK_BOX_WIDTH,
            VERTICAL_ATTACK_BOX_HEIGHT);
        var vABoxData = new Color[VERTICAL_ATTACK_BOX_WIDTH * VERTICAL_ATTACK_BOX_HEIGHT];

        for (int i = 0; i < vABoxData.Length; i++)
        {
            vABoxData[i] = Color.Red;
        }

        vABoxText.SetData(vABoxData);
        _verticalAttackBoxTexture = vABoxText;

        //and the horizontal attack box
        var hABoxText = new Texture2D(SpriteSheet.GraphicsDevice, HORIZONTAL_ATTACK_BOX_WIDTH,
            HORIZONTAL_ATTACK_BOX_HEIGHT);
        var hABoxData = new Color[HORIZONTAL_ATTACK_BOX_WIDTH * HORIZONTAL_ATTACK_BOX_HEIGHT];

        for (int i = 0; i < hABoxData.Length; i++)
        {
            hABoxData[i] = Color.Red;
        }

        hABoxText.SetData(hABoxData);
        _horizontalAttackBoxTexture = hABoxText;
    }

    private void InitializeIdleAnimations()
    {
        var idleDown = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleDown.AddFrame(new Sprite(SpriteSheet, 18, 22, 13, 21));
        idleDown.AddFrame(new Sprite(SpriteSheet, 66, 22, 13, 21));
        idleDown.AddFrame(new Sprite(SpriteSheet, 114, 22, 13, 21));
        idleDown.AddFrame(new Sprite(SpriteSheet, 162, 23, 13, 20));
        idleDown.AddFrame(new Sprite(SpriteSheet, 210, 23, 13, 20));
        idleDown.AddFrame(new Sprite(SpriteSheet, 258, 23, 13, 20));

        var idleUp = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleUp.AddFrame(new Sprite(SpriteSheet, 18, 118, 13, 21));
        idleUp.AddFrame(new Sprite(SpriteSheet, 66, 118, 13, 21));
        idleUp.AddFrame(new Sprite(SpriteSheet, 114, 118, 13, 21));
        idleUp.AddFrame(new Sprite(SpriteSheet, 162, 119, 13, 20));
        idleUp.AddFrame(new Sprite(SpriteSheet, 210, 119, 13, 20));
        idleUp.AddFrame(new Sprite(SpriteSheet, 258, 119, 13, 20));

        var idleLeft = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleLeft.AddFrame(new Sprite(SpriteSheet, 17, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 65, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 113, 70, 15, 21, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 161, 71, 15, 20, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 209, 71, 15, 20, SpriteEffects.FlipHorizontally));
        idleLeft.AddFrame(new Sprite(SpriteSheet, 257, 71, 15, 20, SpriteEffects.FlipHorizontally));

        var idleRight = new SpriteAnimation(ANIM_IDLE_FRAME_DURATION);
        idleRight.AddFrame(new Sprite(SpriteSheet, 17, 70, 15, 21));
        idleRight.AddFrame(new Sprite(SpriteSheet, 65, 70, 15, 21));
        idleRight.AddFrame(new Sprite(SpriteSheet, 113, 70, 15, 21));
        idleRight.AddFrame(new Sprite(SpriteSheet, 161, 71, 15, 20));
        idleRight.AddFrame(new Sprite(SpriteSheet, 209, 71, 15, 20));
        idleRight.AddFrame(new Sprite(SpriteSheet, 257, 71, 15, 20));

        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Down, idleDown);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Up, idleUp);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Left, idleLeft);
        AnimationCollection.AddAnimation(CreatureState.Idling, SpriteDirection.Right, idleRight);
    }

    private void InitializeWalkAnimations()
    {
        var walkDown = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkDown.AddFrame(new Sprite(SpriteSheet, 18, 164, 13, 23));
        walkDown.AddFrame(new Sprite(SpriteSheet, 66, 165, 13, 22));
        walkDown.AddFrame(new Sprite(SpriteSheet, 114, 166, 13, 21));
        walkDown.AddFrame(new Sprite(SpriteSheet, 162, 164, 13, 23));
        walkDown.AddFrame(new Sprite(SpriteSheet, 210, 165, 13, 22));
        walkDown.AddFrame(new Sprite(SpriteSheet, 258, 166, 13, 21));

        var walkUp = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkUp.AddFrame(new Sprite(SpriteSheet, 18, 261, 13, 22));
        walkUp.AddFrame(new Sprite(SpriteSheet, 66, 262, 13, 21));
        walkUp.AddFrame(new Sprite(SpriteSheet, 114, 263, 13, 20));
        walkUp.AddFrame(new Sprite(SpriteSheet, 162, 261, 13, 22));
        walkUp.AddFrame(new Sprite(SpriteSheet, 210, 262, 13, 21));
        walkUp.AddFrame(new Sprite(SpriteSheet, 258, 263, 13, 20));

        var walkRight = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkRight.AddFrame(new Sprite(SpriteSheet, 17, 212, 15, 23));
        walkRight.AddFrame(new Sprite(SpriteSheet, 65, 213, 15, 22));
        walkRight.AddFrame(new Sprite(SpriteSheet, 113, 214, 15, 21));
        walkRight.AddFrame(new Sprite(SpriteSheet, 161, 212, 15, 23));
        walkRight.AddFrame(new Sprite(SpriteSheet, 209, 213, 15, 22));
        walkRight.AddFrame(new Sprite(SpriteSheet, 257, 214, 15, 21));

        var walkLeft = new SpriteAnimation(ANIM_WALK_FRAME_DURATION);
        walkLeft.AddFrame(new Sprite(SpriteSheet, 17, 212, 15, 23, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 65, 213, 15, 22, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 113, 214, 15, 21, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 161, 212, 15, 23, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 209, 213, 15, 22, SpriteEffects.FlipHorizontally));
        walkLeft.AddFrame(new Sprite(SpriteSheet, 257, 214, 15, 21, SpriteEffects.FlipHorizontally));

        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Down, walkDown);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Up, walkUp);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Right, walkRight);
        AnimationCollection.AddAnimation(CreatureState.Walking, SpriteDirection.Left, walkLeft);
    }

    private void InitializeAttackAnimations()
    {
        var attackDown = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackDown.AddFrame(new Sprite(SpriteSheet, 15, 311, 16, 20));
        attackDown.AddFrame(new Sprite(SpriteSheet, 65, 310, 20, 26));
        attackDown.AddFrame(new Sprite(SpriteSheet, 114, 311, 19, 21));
        attackDown.AddFrame(new Sprite(SpriteSheet, 162, 312, 13, 19));

        var attackRight = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackRight.AddFrame(new Sprite(SpriteSheet, 19, 359, 16, 20));
        attackRight.AddFrame(new Sprite(SpriteSheet, 56, 358, 34, 23));
        attackRight.AddFrame(new Sprite(SpriteSheet, 107, 358, 20, 21));
        attackRight.AddFrame(new Sprite(SpriteSheet, 161, 360, 15, 19));

        //normalize the width so our sprite doesn't move but our attack DOES extend farther to the left
        var attackLeft = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false, normalizeWidth: true);
        attackLeft.AddFrame(new Sprite(SpriteSheet, 19, 359, 16, 20, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 56, 358, 34, 23, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 107, 358, 20, 21, SpriteEffects.FlipHorizontally));
        attackLeft.AddFrame(new Sprite(SpriteSheet, 161, 360, 15, 19, SpriteEffects.FlipHorizontally));

        var attackUp = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false);
        attackUp.AddFrame(new Sprite(SpriteSheet, 18, 407, 17, 20));
        attackUp.AddFrame(new Sprite(SpriteSheet, 59, 406, 22, 21));
        attackUp.AddFrame(new Sprite(SpriteSheet, 108, 407, 20, 20));
        attackUp.AddFrame(new Sprite(SpriteSheet, 162, 408, 13, 19));

        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Down, attackDown);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Right, attackRight);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Left, attackLeft);
        AnimationCollection.AddAnimation(CreatureState.Attacking, SpriteDirection.Up, attackUp);
    }

    public override Rectangle GetCollisionBox(Vector2 position)
    {
        Rectangle collisionBox;
        if (State is not CreatureState.Attacking)
        {
            //get our max height and width sprites for the current animation
            var maxHSprite = AnimationCollection.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Height);
            var maxWSprite = AnimationCollection.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Width);

            //draw our box in the middle of what the largest sprite for this animation would be, favoring a bit more
            //towards the feet on the y-axis
            collisionBox = new Rectangle((int)Math.Floor(position.X) + maxWSprite.Width / 2 - COLLISION_BOX_WIDTH / 2,
                (int)Math.Floor(position.Y) + (maxHSprite.Height / 2) - (int)(COLLISION_BOX_HEIGHT / 2.5),
                COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);

            //there can be cases where jamming yourself into a box on the X axis and then trying to move on the Y
            //will cause the player to get stuck bc the collisionBox shifts as the animation does from left/right to up/down
            //handle that by deflating by a pixel to allow us to get out of it
            if (Direction.X == 0 && Direction.Y != 0 && State is CreatureState.Walking)
            {
                collisionBox.Inflate(-1, 0);
            }
        }
        else
        {
            //when attacking, our collisionBox uses the first frame for reference as the attackBox is built from this 
            //collisionBox, and hence when we apply damage will also be applied from a reference of this first frame
            var currentAnimation = AnimationCollection.GetAnimation(State, AnimDirection);
            var currentSprite = currentAnimation.Sprites[0];

            //we'll need to adjust some of the positions to correspond with how we're modifying the draw calls
            var posX = position.X;
            var posY = position.Y;

            //the attack left animation is flipped horizontally,
            //and then gets its X position adjusted to position.X - (currentSprite.Width - _minWidth)
            if (AnimDirection is SpriteDirection.Left)
            {
                posX -= (currentSprite.Width - currentAnimation.MinWidth);
            }
            else if (AnimDirection is SpriteDirection.Down)
            {
                posY -= (currentSprite.Height - currentAnimation.MinHeight);
            }

            collisionBox = new Rectangle(
                (int)Math.Floor(posX) + currentSprite.Width / 2 - COLLISION_BOX_WIDTH / 2,
                (int)Math.Floor(posY) + currentSprite.Height / 2 - COLLISION_BOX_HEIGHT / 2,
                COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);
        }

        return collisionBox;
    }

    public override Rectangle GetAttackBox(Vector2 position)
    {
        //A box representing the area of our sword swing when attacking, variable in size depending on if we're 
        //facing up/down or left/right

        //offset it from the collisionbox, as we consider this the "core" area of our Player that the sword will emit from
        //the collision box for attacking uses the first frame as reference, so our attackbox is based on that first frame
        var collisionBox = GetCollisionBox(position);
        float posX = collisionBox.X;
        float posY = collisionBox.Y;

        //our AnimationDirection is going to either be to the left or to the right
        //so we offset the attackbox from this width
        if (Direction.X != 0)
        {
            //make sure we're properly drawing to the left or right of our player collisionBox
            var widthOffset = (int)Math.Floor(Direction.X) == 1 ? collisionBox.Width : HORIZONTAL_ATTACK_BOX_WIDTH;
            //not entirely outside of the collisionBox, bc we don't want to give him a CRAZY far out swing range
            posX += (int)Math.Floor(widthOffset * Direction.X);
            //draw around center of collisionBox, then slowly focused more downward as we have a downward arc
            posY = posY + collisionBox.Height / 1.25f - HORIZONTAL_ATTACK_BOX_HEIGHT / 2f;
        }
        else if (Direction.Y != 0) //our AnimationDirection is going to be up or down, offset attackbox from this height
        {
            //similar to above, but we keep it in the middle
            var heightOffset = (int)Math.Floor(Direction.Y) == 1 ? collisionBox.Height : VERTICAL_ATTACK_BOX_HEIGHT;
            posX = posX + collisionBox.Width / 2f - VERTICAL_ATTACK_BOX_WIDTH / 2f;
            posY += (int)Math.Floor(heightOffset * Direction.Y);
        }

        var attackBox = new Rectangle(
            (int)Math.Floor(posX),
            (int)Math.Floor(posY),
            Direction.X != 0 ? HORIZONTAL_ATTACK_BOX_WIDTH : VERTICAL_ATTACK_BOX_WIDTH,
            Direction.X != 0 ? HORIZONTAL_ATTACK_BOX_HEIGHT : VERTICAL_ATTACK_BOX_HEIGHT
        );

        return attackBox;
    }

    public override bool Attack(GameTime gameTime)
    {
        var previousState = State;
        State = CreatureState.Attacking;

        //we need to make sure to start playing the animation in case we attacked previously and it'd be ended
        var animation = AnimationCollection.GetAnimation(State, AnimDirection);
        animation.Play();

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
            //TODO this takes twice as long technically so let's revisit this later
            var creatureSet = new HashSet<Creature>();

            foreach (var tile in tiles)
            {
                var creatureListExists = EntityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
                if (!creatureListExists || creatureList is not { Count: > 0 })
                    continue;
                //not the player and intersecthing? you're gonna get hit!
                foreach (var creature in creatureList)
                    if (creature is not Player && creature.GetCollisionBox().Intersects(attackBox))
                        creatureSet.Add(creature);
            }

            foreach (var creature in creatureSet)
            {
                Debug.WriteLine("HIT! on: " + creature.GetType());
            }
        }

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
        var attackBox = GetAttackBox();
        spriteBatch.Draw(Direction.X != 0 ? _horizontalAttackBoxTexture : _verticalAttackBoxTexture,
            new Vector2(attackBox.X, attackBox.Y),
            new Rectangle(0, 0, attackBox.Width, attackBox.Height),
            Color.White * .5f, 0f,
            Vector2.Zero, 1, SpriteEffects.None, 1f);
    }
}
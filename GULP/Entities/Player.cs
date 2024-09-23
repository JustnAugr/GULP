using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GULP.Graphics.Sprites;
using GULP.Graphics.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Entities;

public class Player : ICreature
{
    //movement constants
    private const float ACCELERATION = 1.0f;
    private const float MAX_VELOCITY = 3.0f;
    private const float INITIAL_VELOCITY = 1.0f;

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

    private readonly Map _map;
    private readonly EntityManager _entityManager;
    private readonly Texture2D _spriteSheet;
    private SpriteAnimationColl _animColl;
    private float _velocity;
    private Texture2D _collisionBoxTexture;
    private Texture2D _verticalAttackBoxTexture;
    private Texture2D _horizontalAttackBoxTexture;

    public bool IsAttacking => //attacking is going to be a "heavy" action, we can't cancel it
        State == CreatureState.Attacking && _animColl.GetAnimation(State, AnimDirection).IsPlaying;

    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }
    public float Health { get; set; }
    public CreatureState State { get; private set; }

    public SpriteDirection AnimDirection { get; set; }

    public Player(Texture2D spriteSheet, Vector2 position, Map map, EntityManager entityManager)
    {
        _spriteSheet = spriteSheet;
        _map = map;
        _entityManager = entityManager;

        //values on initialization
        Position = position;
        Health = 100;
        State = CreatureState.Idling;
        Direction = new Vector2(0, 1); //we start looking down
        SetAnimationDirection(Direction); //see above

        //initialize our animations and store them in a double keyed collection for easy lookup
        _animColl = new SpriteAnimationColl();
        InitializeIdleAnimations();
        InitializeWalkAnimations();
        InitializeAttackAnimations();

        //Debug Rectangles
        CreateDebugRects();
    }

    private void CreateDebugRects()
    {
        //create a rectangle representing the collisionBox
        var cBoxText = new Texture2D(_spriteSheet.GraphicsDevice, COLLISION_BOX_WIDTH, COLLISION_BOX_HEIGHT);
        var cBoxData = new Color[COLLISION_BOX_WIDTH * COLLISION_BOX_HEIGHT];

        for (int i = 0; i < cBoxData.Length; i++)
        {
            cBoxData[i] = Color.Cyan;
        }

        cBoxText.SetData(cBoxData);
        _collisionBoxTexture = cBoxText;
        
        //and a rectangle for the vertical attack box
        var vABoxText = new Texture2D(_spriteSheet.GraphicsDevice, VERTICAL_ATTACK_BOX_WIDTH,
            VERTICAL_ATTACK_BOX_HEIGHT);
        var vABoxData = new Color[VERTICAL_ATTACK_BOX_WIDTH * VERTICAL_ATTACK_BOX_HEIGHT];

        for (int i = 0; i < vABoxData.Length; i++)
        {
            vABoxData[i] = Color.Red;
        }

        vABoxText.SetData(vABoxData);
        _verticalAttackBoxTexture = vABoxText;

        //and the horizontal attack box
        var hABoxText = new Texture2D(_spriteSheet.GraphicsDevice, HORIZONTAL_ATTACK_BOX_WIDTH,
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

        //normalize the width so our sprite doesn't move but our attack DOES extend farther to the left
        var attackLeft = new SpriteAnimation(ANIM_ATTACK_FRAME_DURATION, shouldLoop: false, normalizeWidth: true);
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

    public Rectangle GetCollisionBox()
    {
        return GetCollisionBox(Position);
    }

    public Rectangle GetCollisionBox(Vector2 position)
    {
        Rectangle collisionBox;
        if (State is not CreatureState.Attacking)
        {
            //get our max height and width sprites for the current animation
            var maxHSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Height);
            var maxWSprite = _animColl.GetAnimation(State, AnimDirection).Sprites.MaxBy(s => s.Width);

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
            var currentAnimation = _animColl.GetAnimation(State, AnimDirection);
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

    public Rectangle GetAttackBox()
    {
        return GetAttackBox(Position);
    }

    public Rectangle GetAttackBox(Vector2 position)
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

    public bool Walk(Vector2 direction, GameTime gameTime)
    {
        //if we're currently attacking and mid animation, we can't attack-cancel to run
        if (IsAttacking)
            return false;

        //if we were just idilng, we need to build up speed. no speed buildup needed for attacks
        if (State == CreatureState.Idling)
            _velocity = INITIAL_VELOCITY;

        //we're walking, in a direction, with the animation facing that direction
        State = CreatureState.Walking;
        SetAnimationDirection(direction);
        Direction = direction;

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

        //Collision Checking v2
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
                    _velocity = Math.Max(INITIAL_VELOCITY,
                        _velocity - .25f); //we apply friction until it causes us to get down to init velocity
                    posY =
                        Position.Y + _velocity / diagonalAdj * direction.Y; //some friction constant
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
                    _velocity = Math.Max(INITIAL_VELOCITY,
                        _velocity - .25f); //we apply friction until it causes us to get down to init velocity
                    posX = Position.X + _velocity / diagonalAdj * direction.X;
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
        //if we're currently attacking and mid animation, we can't go to idle
        if (IsAttacking)
            return;

        State = CreatureState.Idling;
    }

    public bool Attack(GameTime gameTime)
    {
        var previousState = State;
        State = CreatureState.Attacking;

        //we need to make sure to start playing the animation in case we attacked previously and it'd be ended
        var animation = _animColl.GetAnimation(State, AnimDirection);
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
            var tiles = _map.GetTiles(attackBox);

            //a set so that an entity standing on 2 tiles, both in the path of our sword doesn't take 2 hits
            //TODO this takes twice as long technically so let's revisit this later
            var creatureSet = new HashSet<ICreature>();

            foreach (var tile in tiles)
            {
                var creatureListExists = _entityManager.TileCreatureMap.TryGetValue(tile, out var creatureList);
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

    public bool Attack(Vector2 direction, GameTime gameTime)
    {
        SetAnimationDirection(direction);
        Direction = direction;

        return Attack(gameTime);
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

        //say we were attacking and this .Update() finished the attack animation,
        //rather than draw nothing we should go idle
        if (!animation.IsPlaying)
            Idle();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //This will get moved into some DebugHelper or something, it allows me to see the player's actual collisionbox
        //drawn on top of the sprite
        if (true)
        {
            var collisionBox = GetCollisionBox();
            spriteBatch.Draw(_collisionBoxTexture, new Vector2(collisionBox.X, collisionBox.Y),
                new Rectangle(0, 0, collisionBox.Width, collisionBox.Height),
                Color.White * .5f, 0f,
                Vector2.Zero, 1, SpriteEffects.None, 1f);

            //only draw this if we're attacking
            if (IsAttacking)
            {
                var attackBox = GetAttackBox();
                spriteBatch.Draw(Direction.X != 0 ? _horizontalAttackBoxTexture : _verticalAttackBoxTexture,
                    new Vector2(attackBox.X, attackBox.Y),
                    new Rectangle(0, 0, attackBox.Width, attackBox.Height),
                    Color.White * .5f, 0f,
                    Vector2.Zero, 1, SpriteEffects.None, 1f);
            }
        }

        var animation = _animColl.GetAnimation(State, AnimDirection);
        animation.Draw(spriteBatch, Position);
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GULP.Graphics;

public class NPatch
{
    private readonly int _patchWidth;
    private readonly int _patchHeight;
    private readonly float _layerDepth;

    private readonly List<Rectangle> _patches = new();
    private readonly List<List<int>> _grid;

    private Texture2D Texture { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>
    /// Slices texture to allow for creating auto-resizing drawable textures with differing X and Y patch lengths
    /// </summary>
    /// <param name="texture">the base texture of source patches</param>
    /// <param name="positionX">the X position of the top left corner of the top left patch</param>
    /// <param name="positionY">the Y position of the top left corner of the top left patch</param>
    /// <param name="buffer">the number of pixels between one patch's right side and the next patch's left side,
    /// and also top versus left</param>
    /// <param name="sourcePatchesX">Number of patches that make up source texture on the X axis, including corners</param>
    /// <param name="sourcePatchesY">Number of patches that make up source texture on the Y axis, including corners</param>
    /// <param name="patchWidth">Width of a single patch in pixels</param>
    /// <param name="patchHeight">Height of a single patch in pixels</param>
    /// <param name="destPixelWidth">Width of entire destination texture after stretching along X axis</param>
    /// <param name="destPixelHeight">Height of entire destination texture after stretching along Y axis</param>
    /// <param name="layerDepth">Layer depth to pass to sprite batch for draw call</param>
    public NPatch(Texture2D texture, int positionX, int positionY, int buffer, int sourcePatchesX, int sourcePatchesY,
        int patchWidth, int patchHeight, int destPixelWidth, int destPixelHeight, float layerDepth = 1f)
    {
        Texture = texture;
        _patchWidth = patchWidth;
        _patchHeight = patchHeight;
        _layerDepth = layerDepth;
        Width = destPixelWidth;
        Height = destPixelHeight;

        //in total we'll have xPatchCount * yPatchCount patches
        //patch (0) is top left
        //patch (xPatchCount-1) is top right
        //patch (xPatchCount * (yPatchCount-1)) is bottom left
        //patch ((xPatchCount * yPatchCount) - 1) is bottom right

        //y goes from 0 to yPatchCount * patchHeight
        //x goes from 0 to xPatchCount * patchWidth

        //initialize our source patch rectangles, which will represent the parts of the Texture we're drawing
        for (var y = positionY; y < positionY + sourcePatchesY * patchHeight; y += _patchHeight + buffer)
        {
            for (var x = positionX; x < positionX + sourcePatchesX * patchWidth; x += _patchWidth + buffer)
            {
                _patches.Add(new Rectangle(x, y, patchWidth, patchHeight));
            }
        }

        //how many patches in the middle of our top left and top right ones?
        //and how about between our top left and bottom left?
        var xMult = (int)Math.Ceiling(Width / (float)_patchWidth -
                                      2); //-2 because far left and far right patches don't get tiled
        var yMult = (int)Math.Ceiling(Height / (float)_patchHeight -
                                      2); //-2 because far top and far right don't get tiled

        //taking knowledge about our patches, create a 2D array of the indices of what patch we should be drawing and where
        //based on how we want to stretch our patches along the x and y axis
        _grid = InitIndexGrid(sourcePatchesX, sourcePatchesY, xMult, yMult);
    }

    private List<List<int>> InitIndexGrid(int xPatches, int yPatches, int xMult, int yMult)
    {
        //we can't stretch if our source patches don't allow stretching in that direction:
        //ie stretching along the X without a middle texture between left/right or along the Y without a middle texture
        //between the top/down
        if (xPatches < 3 && xMult > 0)
            throw new ArgumentException("Can't expand on the x-axis without a middle x row!");
        if (yPatches < 3 && yMult > 0)
            throw new ArgumentException("Can't expand on the y-axis without a middle y row!");

        var result = new List<List<int>>();

        //certain of our patches won't vary and can be immediately decided by knowing how many x and y patches we'll have,
        //such as our corners
        var topLeftIdx = 0;
        var topRightIdx = xPatches - 1;
        var botLeftIdx = xPatches * (yPatches - 1);
        var botRightIdx = (xPatches * yPatches) - 1;

        #region TopPatches

        //our top list of indices will have the topleft corner, xMult number of topMid patches repeated, and then
        //the topRight corner
        var top = new List<int> { topLeftIdx };
        var topMidIdx = 1;
        for (var i = 0; i < xMult; i++)
        {
            top.Add(topMidIdx++); //add in our topMid corner xMult times
            if (topMidIdx >= topRightIdx) //reset in case we've surpassed 
                topMidIdx = 1;
        }

        top.Add(topRightIdx);
        result.Add(top);

        #endregion

        #region MiddlePatches

        var midLeftIdx = xPatches; //constant based on math of our grid since it's 0 indexed, xPatches will wrap around
        //the middle region will get stretched on the Y axis yMult times
        for (var y = 0; y < yMult; y++)
        {
            var mid = new List<int> { midLeftIdx };
            var midRightIdx = midLeftIdx + (xPatches - 1);
            var midIdx = midLeftIdx + 1;
            //we also stretch on the X axis xMult times for each yMult
            for (var x = 0; x < xMult; x++)
            {
                mid.Add(midIdx++);
                if (midIdx >= midRightIdx)
                    midIdx = midLeftIdx + 1;
            }

            mid.Add(midRightIdx);
            midLeftIdx += xPatches;
            if (midLeftIdx >= botLeftIdx)
                midLeftIdx = xPatches;
            result.Add(mid);
        }

        #endregion

        #region BottomPatches

        //near replica of what we did for TopPatches
        var bottom = new List<int> { botLeftIdx };
        var botMidIdx = botLeftIdx + 1;
        for (var i = 0; i < xMult; i++)
        {
            bottom.Add(botMidIdx++);
            if (botMidIdx >= botRightIdx)
                botMidIdx = botLeftIdx + 1;
        }

        bottom.Add(botRightIdx);
        result.Add(bottom);

        #endregion

        return result;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        var xPatches = (int)Math.Ceiling(Width / (float)_patchWidth);
        var yPatches = (int)Math.Ceiling(Height / (float)_patchHeight);

        //we then can simply draw the 3d grid we initialized based on the input destination width and height
        for (var y = 0; y < yPatches; y++)
        {
            for (var x = 0; x < xPatches; x++)
            {
                //position calculated here based on offset of what tile we're drawing + the widths/heights
                DrawSlice(spriteBatch, _patches[_grid[y][x]],
                    new Vector2(position.X + (x * _patchWidth), position.Y + (y * _patchHeight)));
            }
        }
    }

    private void DrawSlice(SpriteBatch spriteBatch, Rectangle patch, Vector2 position)
    {
        spriteBatch.Draw(Texture, position, patch, Color.White, 0, new Vector2(0, 0), 1,
            SpriteEffects.None, _layerDepth);
    }
}
using System;
using System.Collections.Generic;
using GULP.Entities;

namespace GULP.Graphics.Sprites;

public class SpriteAnimationColl
{
    private Dictionary<CreatureState, Dictionary<SpriteDirection, SpriteAnimation>> _spriteAnimations = new();

    public void AddAnimation(CreatureState state, SpriteDirection direction, SpriteAnimation animation)
    {
        var directionDict = _spriteAnimations.GetValueOrDefault(state, new Dictionary<SpriteDirection, SpriteAnimation>());

        if (directionDict.ContainsKey(direction))
            throw new ArgumentException(
                "Can't include another entry for this direction and state, it already exists: direction " + direction +
                " state " + state);

        directionDict.Add(direction, animation);
        if (!_spriteAnimations.ContainsKey(state))
            _spriteAnimations.Add(state, directionDict);
    }

    public SpriteAnimation GetAnimation(CreatureState state, SpriteDirection direction)
    {
        if (!_spriteAnimations.ContainsKey(state))
            return null;

        return _spriteAnimations.GetValueOrDefault(state, null).GetValueOrDefault(direction, null);
    }
}
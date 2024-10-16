using System;
using System.Collections.Generic;
using GULP.Entities;
using GULP.Graphics.Interface;

namespace GULP.Systems;

public static class GameContext
{
    //todo better would probably be an InterfaceManager component than doing this here
    public static bool IsMenuOpen { get; set; }
    public static IInterface OpenMenu { get; set; }

    private static readonly Dictionary<string, object> Components = new();

    private static Player _player;

    public static Player Player
    {
        get => _player;
        set
        {
            if (_player != null)
                throw new ArgumentException("Player already set!");
            else
                _player = value;
        }
    }

    public static void AddComponent<T>(T component)
    {
        if (!Components.TryAdd(component.GetType().Name, component))
            throw new ArgumentException($"Component {component.GetType().Name} is already registered");
    }

    public static void GetComponent<T>(out T component)
    {
        component = (T)Components[typeof(T).Name];
    }
}
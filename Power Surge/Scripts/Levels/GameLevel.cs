using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Abstract class to be inherited by all game levels
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public abstract partial class GameLevel : Node2D
{
    protected bool optionsOpen = false;

    protected void checkOptionsMenu()
    {
        // esc pressed
        if (Input.IsActionJustPressed("input_menu") && !optionsOpen)
        {
            GD.Print("Options opened!");
            optionsOpen = true;
            var optionsScene = GD.Load<PackedScene>("res://Scenes/Screens/options_screen.tscn");
            var optionsInstance = optionsScene.Instantiate();
            GetTree().CurrentScene.GetNode<Control>("UI/Control").AddChild(optionsInstance);
            optionsInstance.TreeExited += OnOptionsClosed;
        }
    }
    protected void OnOptionsClosed()
    {
        optionsOpen = false;
    }
}
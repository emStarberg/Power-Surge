using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Moves to next level when contacted by player.
//   For tutorial
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Flag : Area2D, IWorldObject
{
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GetTree().ChangeSceneToFile("res://Scenes/Cutscenes/post_tutorial.tscn");
		}
	}
}

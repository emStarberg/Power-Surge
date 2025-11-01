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
			GameSettings.Instance.TutorialComplete = true;
			GameSettings.Instance.UnlockedLevels[0] = "tutorial";
			GameSettings.Instance.UnlockedLevels[1] = "1-1";
			GameSettings.Instance.SaveGame();
			GetTree().ChangeSceneToFile("res://Scenes/Cutscenes/post_tutorial.tscn");
		}
	}
}

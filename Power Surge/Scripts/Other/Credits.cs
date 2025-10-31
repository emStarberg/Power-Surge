using Godot;
using System;

public partial class Credits : Control
{
	/// <summary>
	/// Called by animation player when credits have finished rolling
	/// </summary>
	public void OnAnimationFinished()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Screens/title_screen.tscn");
	}
}

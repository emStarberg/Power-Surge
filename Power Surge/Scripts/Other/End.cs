using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//  The end of a level, passes data onto the end screen through GameData
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class End : Area2D
{

	private Player player;

	public override void _Ready()
	{
		player = GetParent().GetNode<Player>("Player");
	}

	public void OnBodyEntered(Node2D body)
	{
		// Move to end screen when player passes through
		if (body is Player player)
		{
			GameData.Instance.CurrentLevel = GetParent().Name; ;
			GameData.Instance.LevelFragments = player.GetFragmentCount();
			GameData.Instance.LevelPower = player.GetPower();
			GameData.Instance.LevelTime = 0; // Update this later
			GD.Print("End");
			// Get scene
		}
	}
}

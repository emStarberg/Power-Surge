using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//  	Battery pack increases player's power level by 20% when walked over.
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class BatteryPack : Area2D, IWorldObject
{
	AudioStreamPlayer2D pickUpSound;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pickUpSound = GetNode<AudioStreamPlayer2D>("Sound");
	}
	/// <summary>
	/// Add 20% power when colliding with player
	/// </summary>
	/// <param name="body"></param>
	public void OnBodyEntered(Node2D body)
	{
		// Check collision is with player
		if (body.Name == "Player")
		{
			if (body is Player player)
			{
				pickUpSound.Play();
				player.IncreasePower(20);
				QueueFree();
			}
		}
	}
}

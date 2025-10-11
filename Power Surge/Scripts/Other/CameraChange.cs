using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Area2D that changes the camera mode when passed through by player
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class CameraChange : Area2D
{
	public string DirectionEnteredFrom = "right";  // Only works if the player completely passes through
	public void OnBodyEntered(Node2D body)
	{
		if(body is Player player)
		{
			GD.Print("here");
			DirectionEnteredFrom = player.GetDirection();
		}
	}
}

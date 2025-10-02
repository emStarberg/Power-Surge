using Godot;
using System;

public partial class Fragment : Area2D
{
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.AddFragment();
			QueueFree();
		}
	}
}

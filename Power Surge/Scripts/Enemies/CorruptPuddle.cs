using Godot;
using System;

public partial class CorruptPuddle : Area2D
{
	private bool playerDetected = false;
	private float timer = 0;
	private Player player;

	public override void _Ready()
	{
		player = GetParent().GetParent().GetNode<Player>("Player");
	}

	public override void _Process(double delta)
	{
		timer += (float)delta;
		// Hurt every two seconds
		if (timer >= 2f && playerDetected)
		{
			player.Hurt(5, 1f, 0.2f);
			timer = 0f;
		}
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			playerDetected = true;
		}
	}
	
	public void OnBodyExited(Node2D body)
	{
		if(body is Player player)
		{
			playerDetected = false;
		}
	}
	
}

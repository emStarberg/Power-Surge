using Godot;
using System;

public partial class CorruptPuddle : Area2D
{
	private bool playerDetected = false;
	private float timer = 0;
	private Player player;
	private PointLight2D light;

	public override void _Ready()
	{
		player = GetParent().GetParent().GetNodeOrNull<Player>("Player");
		light = GetNode<PointLight2D>("Light");

		if(player == null)
		{
			player = GetParent().GetParent().GetParent().GetNode<Player>("Player");
		}
	}

	public override void _Process(double delta)
	{
		light.Visible = GameData.Instance.GlowEnabled;
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
		if (body is Player player)
		{
			playerDetected = false;
		}
	}

	/// <summary>
	/// Used by Lab Boss
	/// </summary>
	public void Enable()
	{
		Visible = true;
		GetNode<CollisionShape2D>("Collider").Disabled = false;

	}
	
	/// <summary>
	/// Used by Lab Boss
	/// </summary>
	public void Disable()
	{
		Visible = false;
		GetNode<CollisionShape2D>("Collider").Disabled = true;
	}
	
}

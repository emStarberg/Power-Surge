using Godot;
using System;
/// <summary>
/// Energy Deposit. Restore power to the player when stood on
/// </summary>
public partial class EnergyPuddle : Area2D, IWorldObject
{
	private bool playerDetected = false;
	private float timer = 0;
	private Player player;
	private PointLight2D light;

	public override void _Ready()
	{
		player = GetParent().GetParent().GetNode<Player>("Player");
		light = GetNode<PointLight2D>("Light");
	}

	public override void _Process(double delta)
	{
		light.Visible = GameData.Instance.GlowEnabled;
		timer += (float)delta;
		// Heal every two seconds
		if (timer >= 2f && playerDetected)
		{
			player.IncreasePower(3);
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

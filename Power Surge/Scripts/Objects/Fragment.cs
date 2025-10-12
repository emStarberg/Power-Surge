using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Fragment object that can be collected by the player, there are 3 in each level
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Fragment : Area2D
{
	private PointLight2D light;

	public override void _Ready()
	{
		light = GetNode<PointLight2D>("Light");
	}
	
	public override void _Process(double delta)
	{
		GetNode<PointLight2D>("Light").Visible = GameData.Instance.GlowEnabled;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.AddFragment();
			QueueFree();
		}
	}
}

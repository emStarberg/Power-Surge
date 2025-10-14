using Godot;
using System;
using System.Drawing;
//------------------------------------------------------------------------------
// <summary>
//   3 activators must be active to open the gate in level 2-2
//   Activates when the player makes contact, can't be deactivated
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Activator : Area2D, IWorldObject
{
	private bool active = false;
	private AnimatedSprite2D animation;
	private PointLight2D lightGreen, lightRed;
	public override void _Ready()
	{
		animation = GetNode<AnimatedSprite2D>("Animation");
		animation.Animation = "off";

		lightGreen = GetNode<PointLight2D>("Light Green");
		lightRed = GetNode<PointLight2D>("Light Red");

		lightGreen.Visible = false;
		lightRed.Visible = true;
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			Activate();
		}
	}
	
	private void Activate()
	{
		active = true;
		lightGreen.Visible = true;
		lightRed.Visible = false;
		animation.Animation = "active";
		animation.Play();
	}

}

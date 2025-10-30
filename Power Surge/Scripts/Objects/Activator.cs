using Godot;
using System;
using System.Diagnostics;
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
	[Export] public Gate Gate;
	private bool active = false, timerStarted = false;
	private AnimatedSprite2D animation;
	private PointLight2D lightGreen, lightRed;
	private Camera camera;
	private float timer = 0;


	public override void _Ready()
	{
		animation = GetNode<AnimatedSprite2D>("Animation");
		animation.Animation = "off";

		lightGreen = GetNode<PointLight2D>("Light Green");
		lightRed = GetNode<PointLight2D>("Light Red");

		lightGreen.Visible = false;
		lightRed.Visible = true;

		camera = GetParent().GetParent().GetNode<Camera>("Camera");
	}

	public override void _Process(double delta)
	{
		if (timerStarted)
		{
			timer += (float)delta;
			if(timer >= 1.3f && !this.Gate.IsOn)
			{
				Gate.UpdateState(true);
	
			}
			if(timer >= 2.5f && this.Gate.IsOn)
			{
				timerStarted = false;
				timer = 0;
				camera.CancelPan();
			}
		}
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
		if (!active)
		{
			active = true;
			lightGreen.Visible = true;
			lightRed.Visible = false;
			animation.Animation = "active";
			animation.Play();

			camera.Pan(Gate.GlobalPosition, 2f);
			timerStarted = true;
		}

	}
	
	public bool IsActive()
	{
		return active;
	}

}

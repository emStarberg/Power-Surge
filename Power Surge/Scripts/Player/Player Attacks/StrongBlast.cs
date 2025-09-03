using Godot;
using System;
using System.Diagnostics.CodeAnalysis;

public partial class StrongBlast : Area2D
{
	public string AttackName { get; set; } = "Strong Blast";
	private string direction;
	private AnimatedSprite2D animatedSprite;
	private bool doMove = false;
	private float speed = 350f;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("Anim_StrongBlast");
	}

	public override void _Process(double delta)
	{
		if (doMove)
		{
			if (direction == "left")
				Position += new Vector2(-speed * (float)delta, 0);
			else if (direction == "right")
				Position += new Vector2(speed * (float)delta, 0);
		}
	}

	public void Activate(string dir)
	{
		doMove = true;
		direction = dir;
		if (dir == "right")
		{
			animatedSprite.FlipH = true;
			animatedSprite.Position += new Vector2(-20, 0);
		}
		animatedSprite.Animation = "start";
		animatedSprite.Play();
	}

	public void OnAnimFinished()
	{
		if (animatedSprite.Animation == "start")
		{
			animatedSprite.Animation = "moving"; // "moving" plays on a loop
			animatedSprite.Play();
		}
		else // Is using contact animation
		{
			QueueFree(); // Destroy self
		}
	}

	public void Stop()
	{
		doMove = false;
		animatedSprite.Animation = "contact";
		if (direction == "left")
		{
			animatedSprite.Position += new Vector2(-10, 0);
		}
		else
		{
			animatedSprite.Position += new Vector2(10, 0);
		}
		animatedSprite.Play();
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body.Name != "Player")
		{
			Stop();
		}
		
	}


}

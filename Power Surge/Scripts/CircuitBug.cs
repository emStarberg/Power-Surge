using Godot;
using System;
using System.Security.Cryptography;

public partial class CircuitBug : CharacterBody2D
{
	public const float Speed = 50.0f;
	private Vector2 velocity;
	private string direction = "left"; // Direction facing
	private AnimatedSprite2D run;

	public override void _Ready()
	{
		run = GetNode<AnimatedSprite2D>("Animations/Anim_Run");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Add gravity
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Move based on direction
		if (direction == "right")
		{
			velocity = new Vector2(Speed, Velocity.Y);
		}
		else if (direction == "left")
		{
			velocity = new Vector2(-Speed, Velocity.Y);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void OnAreaEntered(Area2D area)
	{
		// Check if area is in the "Barrier" group
		if (area.IsInGroup("Barrier"))
		{
			if (direction == "right")
			{
				direction = "left";
				run.FlipH = false;
			}
			else
			{
				direction = "right";
				run.FlipH = true;
			}
		}
	}
}

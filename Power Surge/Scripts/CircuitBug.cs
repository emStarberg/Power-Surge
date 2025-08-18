using Godot;
using System;
using System.Security.Cryptography;

public partial class CircuitBug : CharacterBody2D
{
	public const float Speed = 50.0f;
	private Vector2 velocity;
	private string direction = "left"; // Direction facing
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
}

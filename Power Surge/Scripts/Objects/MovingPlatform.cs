using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
//------------------------------------------------------------------------------
// <summary>
//   Can be stood on by the player, moves left and right when activated by a switch
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class MovingPlatform : SwitchOperatedObject
{
	[Export] float MaxDistance = 100; // Maximum x value platform can go relative to starting position (Should start positioned at minimum x value)
	[Export] float Speed = 100;
	private float maxDistance, minDistance;
	private string direction = "right";
	private bool returning = false;

	// Riders handling
	private Area2D riderDetector;
	private List<CharacterBody2D> riders = new();
	private Vector2 previousPosition;

	private RayCast2D leftRay, rightRay;

	public override void _Ready()
	{
		minDistance = Position.X;
		maxDistance = minDistance + MaxDistance;
		previousPosition = Position;
		riderDetector = GetNodeOrNull<Area2D>("Rider Detector");

		leftRay = GetNode<RayCast2D>("Left Ray");
		rightRay = GetNode<RayCast2D>("Right Ray");
	}

	public override void _Process(double delta)
	{
		if (IsOn)
		{
			leftRay.ForceRaycastUpdate();
			rightRay.ForceRaycastUpdate();

			if (leftRay.IsColliding() && direction == "left")
			{
				var collider = leftRay.GetCollider();

				if (collider is Enemy)
				{
					direction = "right";
				}
			}
			
			
			if (rightRay.IsColliding() && direction == "right")
			{
				var collider = rightRay.GetCollider();

				if(collider is Enemy)
				{
					direction = "left";
				}
			}


			if (direction == "right")
			{
				// Move right until maxDistance, then switch direction
				Position += Vector2.Right * Speed * (float)delta;
				if (Position.X >= maxDistance)
				{
					Position = new Vector2(maxDistance, Position.Y);
					direction = "left";
				}
			}
			else // left
			{
				// Move left until minDistance, then switch direction
				Position += Vector2.Left * Speed * (float)delta;
				if (Position.X <= minDistance)
				{
					Position = new Vector2(minDistance, Position.Y);
					direction = "right";
				}
			}
		}
		else if (returning)
		{
			// Move horizontally back toward minDistance, then stop
			if (Position.X <= minDistance)
			{
				Position = new Vector2(minDistance, Position.Y);
				returning = false;
			}
			else
			{
				Position += Vector2.Left * Speed * (float)delta;
			}
		}

		// Move any riders by the same delta the platform moved this frame
		Vector2 deltaPos = Position - previousPosition;
		if (deltaPos != Vector2.Zero && riders.Count > 0)
		{
			// Move each rider by the platform delta to keep them on top.
			foreach (var rider in riders.ToArray())
			{
				if (rider == null || !IsInstanceValid(rider))
					continue;

				// Defensive: only move if rider still overlaps the detector's area
				if (riderDetector == null || riderDetector.OverlapsBody(rider))
				{
					// translate rider in world space by same delta
					rider.GlobalPosition += deltaPos;
					// Optionally, adjust rider Velocity so physics doesn't fight the translation:
					// rider.Velocity = new Vector2(rider.Velocity.X + deltaPos.x / (float)delta, rider.Velocity.Y);
				}
				else
				{
					// rider left top area unexpectedly; remove
					riders.Remove(rider);
				}
			}
		}

		previousPosition = Position;
	}

	protected override void OnStateSwitched()
	{
		if (!IsOn)
		{
			// when turned off, start returning to the start (minDistance)
			direction = "left";
			returning = true;
		}
	}

	// Rider detector handlers
	private void OnRiderEntered(Node body)
	{
		if (body is CharacterBody2D cb && !riders.Contains(cb))
			riders.Add(cb);
	}

	private void OnRiderExited(Node body)
	{
		if (body is CharacterBody2D cb)
			riders.Remove(cb);
	}
}

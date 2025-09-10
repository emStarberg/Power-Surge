using Godot;
using System;

public partial class VoltageSentinel : Enemy
{
	public const float Speed = 15.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction facing
	private bool playerDetected = false; // Whether player has been detected
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn"); // For spawning projectiles
	private RayCast2D groundRay, wallRay, playerRay; // Ground detection, wall/object detection, player detection

	private Vector2 startPosition;
	private float walkedDistance = 0f;
	private float walkDistance = 75f; // Distance to walk before turning
	private bool isWalking = true;
	private float stopTime = 2f;
	private float stopTimer = 0f;

	public override void _Ready()
	{
		animation = GetNode<AnimatedSprite2D>("Animations");
		groundRay = GetNode<RayCast2D>("RayCasts/GroundRay");
		wallRay = GetNode<RayCast2D>("RayCasts/WallRay");
		startPosition = GlobalPosition;

		animation.Animation = "walk";
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		if (isWalking)
		{
			// Move based on direction
			float moveStep = Speed * (float)delta;
			if (direction == "right")
				velocity = new Vector2(Speed, Velocity.Y);
			else
				velocity = new Vector2(-Speed, Velocity.Y);

			// Check for wall ahead
			wallRay.ForceRaycastUpdate();
			if (wallRay.IsColliding())
			{
				direction = direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
				walkedDistance = 0f;
				startPosition = GlobalPosition;
			}

			// Check for ground ahead
			groundRay.ForceRaycastUpdate();
			if (!groundRay.IsColliding())
			{
				direction = direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
				walkedDistance = 0f;
				startPosition = GlobalPosition;
			}

			// Track distance walked
			walkedDistance = (GlobalPosition - startPosition).Length();
			if (walkedDistance >= walkDistance)
			{
				isWalking = false;
				animation.Animation = "idle";
				stopTimer = stopTime;
			}

			Velocity = velocity;
			MoveAndSlide();
		}
		else
		{
			// Stopping phase
			velocity = new Vector2(0, Velocity.Y);
			Velocity = velocity;
			MoveAndSlide();

			stopTimer -= (float)delta;
			if (stopTimer <= 0f)
			{
				// Reverse direction and start walking again
				direction = direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
				isWalking = true;
				animation.Animation = "walk";
				walkedDistance = 0f;
				startPosition = GlobalPosition;
			}
		}

	}
}

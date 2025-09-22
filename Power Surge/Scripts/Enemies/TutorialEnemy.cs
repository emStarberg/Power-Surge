using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Controls the enemy appearing in the tutorial level
// 	 Inherits Enemy
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class TutorialEnemy : Enemy
{
	public const float Speed = 50.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction bug is facing
	private bool isRunning = true; // Whether bug is running
	private bool playerDetected = false; // Whether player has been detected
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn"); // For spawning projectiles
	private RayCast2D groundRay, wallRay; // Ground detection, wall/object detection, player detection
	private bool projectileSpawnedThisAttack = false; // Prevents spawning > 1 projectile per animation


	public override void _Ready()
	{
		groundRay = GetNode<RayCast2D>("GroundRay");
		wallRay = GetNode<RayCast2D>("WallRay");
		animation = GetNode<AnimatedSprite2D>("Animations");

		hurtCooldownTimer = new Timer();
		hurtCooldownTimer.WaitTime = 0.5f;
		hurtCooldownTimer.OneShot = true;
		AddChild(hurtCooldownTimer);

		animation.FrameChanged += OnAnimationFrameChanged;
		animation.AnimationFinished += OnAnimationFinished;

		health = 25;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isAlive)
		{
			// Gravity
			if (!IsOnFloor())
				velocity += GetGravity() * (float)delta;

			if (isRunning)
			{
				// Move based on direction
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
				}

				// Check for ground ahead
				groundRay.ForceRaycastUpdate();
				if (!groundRay.IsColliding())
				{
					direction = direction == "right" ? "left" : "right";
					Scale = new Vector2(-Scale.X, Scale.Y);
				}
			}
			Velocity = velocity;
			MoveAndSlide();
		}
	}

	/// <summary>
	/// Use projectile attack
	/// </summary>
	public void Attack()
	{
		animation.Animation = "attack";
		animation.Play();
		isRunning = false;
		// Stop
		velocity = new Vector2(0, 0);
	}

	
	/// <summary>
	/// Spawn a projectile on 13th frame to line up with animation
	/// </summary>
	private void OnAnimationFrameChanged()
	{
		if (animation.Frame == 13 && !projectileSpawnedThisAttack && animation.Animation == "attack")
		{
			projectileSpawnedThisAttack = true;
			// Spawn new projectile
			Node projectileInstance = projectile.Instantiate();
			((Node2D)projectileInstance).GlobalPosition = GlobalPosition;
			GetTree().Root.CallDeferred("add_child", projectileInstance);

			if (projectileInstance is ProjectileCB projectileScript)
			{
				// Fire projectile
				projectileScript.Fire(direction);
			}
		}

		// Reset flag when animation loops or finishes
		if (animation.Frame == 0)
		{
			projectileSpawnedThisAttack = false;
		}
	}

    /// <summary>
    /// Called when any animation finishes
    /// </summary>
	public void OnAnimationFinished()
	{
		if (animation.Animation == "death")
		{
			QueueFree();
		}
	}
} 

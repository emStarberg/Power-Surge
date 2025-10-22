using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Controls the Circuit Bug enemy.
// 	 Inherits Enemy
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class CircuitBug : Enemy
{
	public const float Speed = 50.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction bug is facing
	private bool isRunning = true; // Whether bug is running
	private bool playerDetected = false; // Whether player has been detected
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn"); // For spawning projectiles
	private RayCast2D groundRay, wallRay, playerRay; // Ground detection, wall/object detection, player detection
	private bool projectileSpawnedThisAttack = false; // Prevents spawning > 1 projectile per animation
	private AudioStreamPlayer2D attackSound;


	public override void _Ready()
	{
		player = GetParent().GetParent().GetNode<Player>("Player");
		playerRay = GetNode<RayCast2D>("PlayerRay");
		groundRay = GetNode<RayCast2D>("GroundRay");
		wallRay = GetNode<RayCast2D>("WallRay");
		animation = GetNode<AnimatedSprite2D>("Animations");
		attackSound = GetNode<AudioStreamPlayer2D>("Sounds/Attack");
		hurtSound = GetNode<AudioStreamPlayer2D>("Sounds/Hurt");

		hurtCooldownTimer = new Timer();
		hurtCooldownTimer.WaitTime = 0.5f;
		hurtCooldownTimer.OneShot = true;
		AddChild(hurtCooldownTimer);

		animation.FrameChanged += OnAnimationFrameChanged;
		animation.AnimationFinished += OnAnimationFinished;

		maxHealth = 10;
		health = maxHealth;

		UpdateVolume();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isAlive)
		{				
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
				// apply gravity to vertical velocity and clamp to terminal velocity
				velocity.Y = Mathf.Min(velocity.Y + gravity * (float)delta, maxFallSpeed);
			}
			else
			{
				// ensure small downward velocity is cleared when on floor
				if (velocity.Y > 0)
					velocity.Y = 0;
			}

			if (isRunning && IsOnFloor())
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
					GD.Print("Wall ray colliding");
					direction = direction == "right" ? "left" : "right";
					Scale = new Vector2(-Scale.X, Scale.Y);
				}

				// Check for ground ahead
				groundRay.ForceRaycastUpdate();
				if (!groundRay.IsColliding())
				{
					GD.Print("Ground ray colliding");
					direction = direction == "right" ? "left" : "right";
					Scale = new Vector2(-Scale.X, Scale.Y);
				}
			}

			// Player detection via raycast
			playerRay.ForceRaycastUpdate();
			if (playerRay.IsColliding() && playerRay.GetCollider() is Node2D collider && collider.Name == "Player")
			{
				if (!playerDetected)
				{
					GD.Print("Player detected by raycast");
					Attack();
					playerDetected = true;
				}
			}
			else
			{
				if (playerDetected)
				{
					GD.Print("Player lost by raycast");
					playerDetected = false;
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
	/// Spawn projectile on 13th frame to line up with animation
	/// </summary>
	private void OnAnimationFrameChanged()
	{
		if (animation.Frame == 13 && !projectileSpawnedThisAttack && animation.Animation == "attack")
		{
			attackSound.Play();
			projectileSpawnedThisAttack = true;
			// Spawn new projectile
			Node projectileInstance = projectile.Instantiate();
			((Node2D)projectileInstance).GlobalPosition = GlobalPosition += new Vector2(-5,0);
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
	/// Called when an animation finishes.
	/// Behaves differently depending on animation
	/// </summary>
	public void OnAnimationFinished()
	{
		if (animation.Animation == "death")
		{
			QueueFree();
		}
		else if (animation.Animation == "attack")
		{
			if (!playerDetected)
			{
				// Return to running
				animation.Animation = "run";
				animation.Play();
				isRunning = true;
			}
			else
			{
				// Attack again if player still detected
				Attack();
			}
		}
	}

	public override void UpdateVolume()
	{
		attackSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
		hurtSound.VolumeDb = GameSettings.Instance.GetFinalSfx() - 10;
	}
}

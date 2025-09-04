using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Controls the Circuit Bug enemy
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class CircuitBug : CharacterBody2D
{
	public const float Speed = 50.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction bug is facing
	private AnimatedSprite2D runAnim, attackAnim, currentAnimation; // Bug animations
	private bool isRunning = true; // Whether bug is running
	private bool playerDetected = false; // Whether player has been detected
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn"); // For spawning projectiles
	private RayCast2D groundRay, wallRay, playerRay; // Ground detection, wall/object detection, player detection

	public override void _Ready()
	{
		playerRay = GetNode<RayCast2D>("PlayerRay");
		groundRay = GetNode<RayCast2D>("GroundRay");
		wallRay = GetNode<RayCast2D>("WallRay");
		runAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Run");
		attackAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Attack");
		currentAnimation = runAnim;
		currentAnimation.Play();

		attackAnim.FrameChanged += OnAttackAnimFrameChanged;
	}

	public override void _PhysicsProcess(double delta)
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

	/// <summary>
	/// Use projectile attack
	/// </summary>
	public void Attack()
	{
		GD.Print("attack");
		SetAnimation(attackAnim);
		runAnim.Visible = false;
		isRunning = false;
		// Stop
		velocity = new Vector2(0, 0);
	}

	/// <summary>
	/// Resume running if player no longer detected after attack animation finished
	/// </summary>
	public void OnAttackAnimFinished()
	{
		if (!playerDetected)
		{
			// Return to running
			SetAnimation(runAnim);
			attackAnim.Visible = false;
			isRunning = true;
		}
		else
		{
			// Attack again if player still detected
			Attack();
		}
	}
	/// <summary>
	/// Set the current animation plaing
	/// </summary>
	/// <param name="anim">Animation to play</param>
	public void SetAnimation(AnimatedSprite2D anim)
	{
		currentAnimation = anim;
		currentAnimation.Visible = true;
		currentAnimation.Play();
	}
	
	private bool projectileSpawnedThisAttack = false; // Prevents spawning > 1 projectile per animation
	/// <summary>
	/// Spawn a projectile on 13th frame to line up with animation
	/// </summary>
	private void OnAttackAnimFrameChanged()
	{
		if (attackAnim.Frame == 13 && !projectileSpawnedThisAttack)
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
		if (attackAnim.Frame == 0)
		{
			projectileSpawnedThisAttack = false;
		}
	}
}

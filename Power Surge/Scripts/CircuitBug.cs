using Godot;
using System;

public partial class CircuitBug : CharacterBody2D
{
	public const float Speed = 50.0f;
	private Vector2 velocity;
	private string Direction = "left";
	private AnimatedSprite2D RunAnim, AttackAnim, CurrentAnimation;
	private bool IsRunning = true;
	private bool PlayerDetected = false;
	private PackedScene Projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn");
	private RayCast2D groundRay;
	private RayCast2D wallRay;
	private RayCast2D playerRay;
	

	public override void _Ready()
	{
		playerRay = GetNode<RayCast2D>("PlayerRay");
		groundRay = GetNode<RayCast2D>("GroundRay");
		wallRay = GetNode<RayCast2D>("WallRay");
		RunAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Run");
		AttackAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Attack");
		CurrentAnimation = RunAnim;
		CurrentAnimation.Play();

		AttackAnim.FrameChanged += OnAttackAnimFrameChanged;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Gravity
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		if (IsRunning)
		{
			// Move based on direction
			if (Direction == "right")
				velocity = new Vector2(Speed, Velocity.Y);
			else
				velocity = new Vector2(-Speed, Velocity.Y);


			// Check for wall ahead
			wallRay.ForceRaycastUpdate();
			if (wallRay.IsColliding())
			{
				Direction = Direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
			}

			// Check for ground ahead
			groundRay.ForceRaycastUpdate();
			if (!groundRay.IsColliding())
			{
				Direction = Direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
			}

			
		}

		// Player detection via raycast
			playerRay.ForceRaycastUpdate();
			if (playerRay.IsColliding() && playerRay.GetCollider() is Node2D collider && collider.Name == "Player")
			{
				if (!PlayerDetected)
				{
					GD.Print("Player detected by raycast");
					Attack();
					PlayerDetected = true;
				}
			}
			else
			{
				if (PlayerDetected)
				{
					GD.Print("Player lost by raycast");
					PlayerDetected = false;
				}
			}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Attack()
	{
		GD.Print("attack");
		SetAnimation(AttackAnim);
		RunAnim.Visible = false;
		IsRunning = false;
		// Stop
		velocity = new Vector2(0, 0);
	}

	public void OnAttackAnimFinished()
	{
		if (!PlayerDetected)
		{
			// Return to running
			SetAnimation(RunAnim);
			AttackAnim.Visible = false;
			IsRunning = true;
		}
		else
		{
			Attack();
		}
	}


	public void SetAnimation(AnimatedSprite2D anim)
	{
		CurrentAnimation = anim;
		CurrentAnimation.Visible = true;
		CurrentAnimation.Play();
	}
	private bool projectileSpawnedThisAttack = false;
	private void OnAttackAnimFrameChanged()
{
	// Only spawn once per attack animation
	if (AttackAnim.Frame == 13 && !projectileSpawnedThisAttack)
	{
		projectileSpawnedThisAttack = true;

		Node projectileInstance = Projectile.Instantiate();
		((Node2D)projectileInstance).GlobalPosition = GlobalPosition;
		GetTree().Root.CallDeferred("add_child", projectileInstance);

		if (projectileInstance is ProjectileCB projectileScript)
		{
			projectileScript.Fire(Direction);
		}
	}

	// Reset flag when animation loops or finishes
	if (AttackAnim.Frame == 0)
	{
		projectileSpawnedThisAttack = false;
	}
}
}

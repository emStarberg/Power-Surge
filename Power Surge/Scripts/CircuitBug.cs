using Godot;
using System;
using System.Security.Cryptography;

public partial class CircuitBug : CharacterBody2D
{
	[Export] public float Gravity = 500f; // Gravity force      
	[Export] public float MaxFallSpeed = 1000f; // Terminal velocity
	public const float Speed = 50.0f;
	private Vector2 velocity;
	private string Direction = "left"; // Direction facing
	private AnimatedSprite2D RunAnim, AttackAnim, CurrentAnimation;
	private bool IsRunning = true;
	private bool PlayerDetected = false;
	private PackedScene Projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn");


	public override void _Ready()
	{
		RunAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Run");
		AttackAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Attack");
		CurrentAnimation = RunAnim;
		CurrentAnimation.Play();

		// Connect the frame changed signal
		AttackAnim.FrameChanged += OnAttackAnimFrameChanged;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Add gravity
		// Apply gravity to Y velocity.
			velocity.Y += Gravity * (float)delta;
			// Clamp vertical velocity to terminal velocity.
			velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		if (IsRunning)
		{
			// Move based on direction
			if (Direction == "right")
			{
				velocity.X = Speed;
			}
			else if (Direction == "left")
			{
				velocity.X = -Speed;
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

	public void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("Ground"))
		{
			// Change direction
			Direction = Direction == "right" ? "left" : "right";
			// Flip horizontally
			Scale = new Vector2(-Scale.X, Scale.Y);
		}
	}

	public void OnPlayerDetectionEntered(Node2D body)
	{
		if (body.Name == "Player")
		{
			GD.Print("Player entered");
			Attack();
			PlayerDetected = true;
		}
	}

	public void OnPlayerDetectionExited(Node2D body)
	{
		if (body.Name == "Player")
		{
			GD.Print("Player exited");
			PlayerDetected = false;
		}
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

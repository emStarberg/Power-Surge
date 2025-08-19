using Godot;
using System;
using System.Security.Cryptography;

public partial class CircuitBug : CharacterBody2D
{
	public const float Speed = 50.0f;
	private Vector2 velocity;
	private string direction = "left"; // Direction facing
	private AnimatedSprite2D RunAnim, AttackAnim, CurrentAnimation;
	private bool IsRunning = true;
	private bool PlayerDetected = false;


	public override void _Ready()
	{
		RunAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Run");
		AttackAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Attack");
		CurrentAnimation = RunAnim;
		CurrentAnimation.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Add gravity
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (IsRunning)
		{
			// Move based on direction
			if (direction == "right")
			{
				velocity = new Vector2(Speed, Velocity.Y);
			}
			else if (direction == "left")
			{
				velocity = new Vector2(-Speed, Velocity.Y);
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

	public void OnAreaEntered(Area2D area){
		// Check if area is in the "Barrier" group
		if (area.IsInGroup("Barrier"))
		{
			// Change direction
			direction = direction == "right" ? "left" : "right";
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
}

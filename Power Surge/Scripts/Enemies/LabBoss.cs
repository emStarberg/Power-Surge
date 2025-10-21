using Godot;
using System;
using System.Data.Common;
//------------------------------------------------------------------------------
// <summary>
//   Boss at the end of level 3-2.
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class LabBoss : Enemy
{
	private Area2D hitBox; // Area where boss can be hit
	private Sprite2D sign; // Part that flashes red when hit
	private String currentAction = "idle"; // What the boss is currently doing (move, idle, shock, shoot, die)
	private String direction = "left";
	private Vector2 velocity; // For updating Velocity property
	private AnimationPlayer animationPlayer;
	private const float Speed = 100.0f; // Movement speed
	private bool movementPaused = false; // For short pauses in between "jumps"
	private bool gravityEnabled = true;
	private float timer = 0; // For idle and move
	private int attackStartSide = 0;
	private Vector2 originalScale;
	private Node2D visualRoot; // Nodes that should be flipped. AnimationPlayer causes odd behaviour, this is a workaround
	private Node2D corruptPuddles; // For shock attack
	private Node2D electricity; // For jump damage


	public override void _Ready()
	{
		visualRoot = GetNode<Node2D>("VisualRoot");
		originalScale = visualRoot.Scale;

		hitBox = GetNode<Area2D>("Hit Box");
		animationPlayer = GetNode<AnimationPlayer>("Animation Player");
		sign = GetNode<Sprite2D>("VisualRoot/Sign");
		hurtSound = GetNode<AudioStreamPlayer2D>("VisualRoot/Sounds/Hurt");

		health = 250;

		animationPlayer.CurrentAnimation = "Idle";
		animationPlayer.Play();

		player = GetParent().GetParent().GetNode<Player>("Player");

		corruptPuddles = GetParent().GetNode<Node2D>("Corrupt Puddles");
		DisablePuddles();

		electricity = GetNode<Node2D>("VisualRoot/Electricity");
	}


	public override void _PhysicsProcess(double delta)
	{
		velocity = Velocity;

		// Face player if not attacking
		if (currentAction == "move" || currentAction == "idle")
		{
			FacePlayerIfNeeded();
			timer += (float)delta;
			if (timer >= 3)
			{
				timer = 0;
				ChooseNewAction();
			}
		}

		if (gravityEnabled)
		{
			if (!IsOnFloor())
				velocity.Y = Mathf.Min(velocity.Y + gravity * (float)delta, maxFallSpeed);
			else if (velocity.Y > 0)
				velocity.Y = 0;
		}

		// Don't move when not in the air
		if (movementPaused)
		{
			velocity.X = 0;
		}
		else
		{
			switch (currentAction)
			{
				case "idle":
					velocity.X = 0;
					break;

				case "move":
					velocity.X = (direction == "left") ? -Speed : Speed;
					break;
			}
		}

		// Apply movement
		Velocity = velocity;
		MoveAndSlide();
	}

	/// <summary>
	/// Stop all movement
	/// </summary>
	public void PauseMovement()
	{
		GD.Print("paused");
		movementPaused = true;
	}

	/// <summary>
	/// Resume all movement
	/// </summary>
	public void UnpauseMovement()
	{
		GD.Print("unpaused");
		movementPaused = false;
	}


	/// <summary>
	/// Called when enemy is hit by an attack
	/// </summary>
	/// <param name="amount">Amount of health to subtract</param>
	public override void Hurt(float amount)
	{
		if (!canBeHurt)
			return;

		hurtSound.Play();
		health -= amount;
		if (health <= 0)
		{
			Die();
		}
		else
		{
			FlashHurtEffectSign();
			canBeHurt = false;
		}
	}

	/// <summary>
	/// Flash with red overlay when hurt
	/// </summary>
	protected async void FlashHurtEffectSign()
	{
		if (this.isAlive)
		{
			for (int i = 0; i < 3; i++)
			{
				sign.Modulate = new Color(1, 0, 0);
				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
				sign.Modulate = new Color(1, 1, 1);
				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
			}
			canBeHurt = true;
		}

	}


	/// <summary>
	/// Enable gravity
	/// </summary>
	public void EnableGravity()
	{
		gravityEnabled = true;
	}

	/// <summary>
	/// Disable gravity when mid jump
	/// </summary>
	public void DisableGravity()
	{
		gravityEnabled = false;
	}

	/// <summary>
	/// Crush player if jumped on
	/// </summary>
	/// <param name="body"></param>
	public void OnFootBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Hurt(2000, 0.2f, 0.2f);
		}
	}

	/// <summary>
	/// Choose a new action to do
	/// </summary>
	public void ChooseNewAction()
	{
		// Select a random new action
		Velocity = new Vector2(0, Velocity.Y);
		Random rng = new Random();

		// Only attack if didn't attack last time
		if (currentAction == "idle" || currentAction == "move")
		{
			int random = rng.Next(1, 5);
			switch (random)
			{
				case 1:
					currentAction = "move";
					break;

				case 2:
					currentAction = "move";
					break;

				case 3:
					currentAction = "shock";
					break;

				case 4:
					currentAction = "shoot";
					break;
			}
		}
		else
		{
			int random = rng.Next(1, 4);
			switch (random)
			{
				case 1:
					currentAction = "idle";
					break;

				case 2:
					currentAction = "move";
					break;

				case 3:
					currentAction = "shock";
					break;
			}
		}

		movementPaused = currentAction != "move";

		// Set up new action
		switch (currentAction)
		{
			case "idle":
				animationPlayer.CurrentAnimation = "Idle";
				animationPlayer.Play();
				Velocity = new Vector2(0, Velocity.Y);
				break;

			case "move":
				animationPlayer.CurrentAnimation = "Jump";
				animationPlayer.Play(); ;
				FacePlayerIfNeeded();
				break;

			case "shock":
				animationPlayer.CurrentAnimation = "Shock";
				attackStartSide = player != null ? Math.Sign(player.GlobalPosition.X - GlobalPosition.X) : 0;
				animationPlayer.Play();
				Velocity = new Vector2(0, Velocity.Y);
				break;

			case "shoot":
				animationPlayer.CurrentAnimation = "Shoot";
				attackStartSide = player != null ? Math.Sign(player.GlobalPosition.X - GlobalPosition.X) : 0;
				animationPlayer.Play();
				Velocity = new Vector2(0, Velocity.Y);
				break;
		}
	}

	/// <summary>
	/// Called by animationPlayer when an animation finishes
	/// </summary>
	public void OnAnimationFinished()
	{
		if (attackStartSide != 0)
		{
			int currentSide = player != null ? Math.Sign(player.GlobalPosition.X - GlobalPosition.X) : 0;
			if (currentSide != 0 && currentSide != attackStartSide)
			{
				SetDirection(currentSide > 0 ? "right" : "left");
			}
			attackStartSide = 0;
		}
		ChooseNewAction();
	}

	/// <summary>
	/// Turn to face the player if not already
	/// </summary>
	/// <param name="deadzone">Zone where player position doesn't change direction</param>
	private void FacePlayerIfNeeded(float deadzone = 8f)
	{
		if (player == null)
			return;
		float dx = player.GlobalPosition.X - GlobalPosition.X;
		if (dx > deadzone)
			SetDirection("right");
		else if (dx < -deadzone)
			SetDirection("left");
	}

	/// <summary>
	/// Set direction
	/// </summary>
	/// <param name="dir">Direction to set to ("left" or "right")</param>
	public void SetDirection(string dir)
	{
		if (dir == direction) return;
		direction = dir;

		float sx = Mathf.Abs(originalScale.X);
		visualRoot.Scale = (direction == "right")
			? new Vector2(-sx, originalScale.Y)
			: new Vector2(sx, originalScale.Y);
	}

	/// <summary>
	/// Enable all corrupt puddles (shock attack)
	/// </summary>
	private void EnablePuddles()
	{
		foreach (Node n in corruptPuddles.GetChildren())
		{
			if (n is CorruptPuddle puddle)
			{
				puddle.Enable();

			}
		}
	}

	/// <summary>
	/// Disable all corrupt puddles (shock attack)
	/// </summary>
	private void DisablePuddles()
	{
		foreach (Node n in corruptPuddles.GetChildren())
		{
			if (n is CorruptPuddle puddle)
			{
				puddle.Disable();

			}
		}
	}

	/// <summary>
	/// When a body (player) touches the electricity given off when landing from a jump
	/// </summary>
	/// <param name="body"></param>
	public void OnElectricityBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Hurt(8, 0.2f, 0.2f);
		}
	}

	/// <summary>
	/// Enable electricity
	/// </summary>
	public void EnableElectricity()
	{
		foreach (Node n in electricity.GetChildren())
		{
			if (n is AnimatedSprite2D anim)
			{
				anim.Visible = true;
				anim.Play();
			}
			else if (n is Area2D area)
			{
				area.GetNode<CollisionShape2D>("Collider").Disabled = false;
			}
		}
	}
	
	public void OnElectricityAnimFinished()
	{
		foreach (Node n in electricity.GetChildren())
		{
			if (n is AnimatedSprite2D anim)
			{
				anim.Visible = false;
			}
			else if (n is Area2D area)
			{
				area.GetNode<CollisionShape2D>("Collider").Disabled = true;
			}
		}
	}
}

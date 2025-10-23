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
	[Export] public TextureProgressBar HealthBar;
	[Export] public Camera camera;
	private Area2D hitBox; // Area where boss can be hit
	private Sprite2D sign; // Part that flashes red when hit
	private String currentAction = "idle"; // What the boss is currently doing (move, idle, shock, shoot, die)
	private String direction = "left";
	private Vector2 velocity; // For updating Velocity property
	private AnimationPlayer animationPlayer;
	private const float Speed = 100.0f; // Movement speed
	private bool movementPaused = false; // For short pauses in between "jumps"
	private bool gravityEnabled = true;
	private bool canMove = true;
	private float timer = 0; // For idle and move
	private int attackStartSide = 0;
	private Vector2 originalScale;
	private Node2D visualRoot; // Nodes that should be flipped. AnimationPlayer causes odd behaviour, this is a workaround
	private Node2D corruptPuddles; // For shock attack
	private Node2D electricity; // For jump damage
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_lab_boss.tscn"); // For projectiles
	private AudioStreamPlayer2D shootSound, shockSound, jumpSound;
	private RayCast2D wallRay;
	private bool hasStarted = false;


	public override void _Ready()
	{
		UpdateVolume();
		visualRoot = GetNode<Node2D>("VisualRoot");
		originalScale = visualRoot.Scale;

		hitBox = GetNode<Area2D>("Hit Box");
		animationPlayer = GetNode<AnimationPlayer>("Animation Player");
		sign = GetNode<Sprite2D>("VisualRoot/Sign");
		hurtSound = GetNode<AudioStreamPlayer2D>("Sounds/Hurt");
		shootSound = GetNode<AudioStreamPlayer2D>("Sounds/Shoot");
		shockSound = GetNode<AudioStreamPlayer2D>("Sounds/Shock");
		jumpSound = GetNode<AudioStreamPlayer2D>("Sounds/Jump");

		wallRay = GetNode<RayCast2D>("VisualRoot/Wall Ray");

		health = 200;

		animationPlayer.CurrentAnimation = "Idle";
		animationPlayer.Play();

		player = GetParent().GetParent().GetNode<Player>("Player");

		corruptPuddles = GetParent().GetNode<Node2D>("Corrupt Puddles");
		DisablePuddles();

		electricity = GetNode<Node2D>("VisualRoot/Electricity");

		Visible = false;
	}


	public override void _PhysicsProcess(double delta)
	{
		if (isAlive && hasStarted)
		{
			wallRay.ForceRaycastUpdate();
			if (wallRay.IsColliding())
			{
				if(wallRay.GetCollider() is TileMapLayer)
				{
					if (canMove)
					{
						canMove = false;
						if(currentAction == "move")
						ChooseNewAction();
					}
					
				}
			}
			else
			{
				canMove = true;
			}
			HealthBar.Value = health;
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
	}

	/// <summary>
	/// Stop all movement
	/// </summary>
	public void PauseMovement()
	{
		movementPaused = true;
	}

	/// <summary>
	/// Resume all movement
	/// </summary>
	public void UnpauseMovement()
	{
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
			player.Hurt(2000, 0.2f, 0.2f); // Kill player

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
				if (canMove)
				{
					animationPlayer.CurrentAnimation = "Jump";
					animationPlayer.Play(); ;
					FacePlayerIfNeeded();
				}
				else
				{
					ChooseNewAction();
				}
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
		if (isAlive)
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
		}else if (animationPlayer.CurrentAnimation == "death")
		{
			QueueFree();
		}
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
				var collider = area.GetNodeOrNull<CollisionShape2D>("Collider");
				if (collider != null)
				{
					collider.CallDeferred("set_disabled", false);
				}
			}
		}
	}

	/// <summary>
	/// Enable electricity
	/// </summary>
	public void DisableElectricity()
	{
		foreach (Node n in electricity.GetChildren())
		{
			if (n is AnimatedSprite2D anim)
			{
				anim.Visible = false;
			}
			else if (n is Area2D area)
			{
				var collider = area.GetNodeOrNull<CollisionShape2D>("Collider");
				if (collider != null)
				{
					collider.CallDeferred("set_disabled", true);
				}
			}
		}
	}

	/// <summary>
	/// Called when electricity animations finish
	/// Remove electricity
	/// </summary>
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
				var collider = area.GetNodeOrNull<CollisionShape2D>("Collider");
				if (collider != null)
				{
					collider.CallDeferred("set_disabled", true);
				}
			}
		}
	}

	public void ShootProjectile()
	{
		Node attackInstance = projectile.Instantiate();
		((ProjectileLabBoss)attackInstance).GlobalPosition = GlobalPosition + new Vector2(0, 20);
		GetTree().Root.AddChild(attackInstance);
		shootSound.Play();
		if (attackInstance is ProjectileLabBoss p)
		{
			p.Activate(direction);
		}

	}

	/// <summary>
	/// Called when health = 0
	/// </summary>
	public override void Die()
	{
		camera.Shake(2, 0.4f);
		isAlive = false;
		canBeHurt = false;
		animationPlayer.CurrentAnimation = "Death";
		sign.Visible = false;
		DisablePuddles();
		DisableElectricity();
		PauseMovement();
	}

	public override void UpdateVolume()
	{
		foreach (Node n in GetNode<Node2D>("Sounds").GetChildren())
		{
			if (n is AudioStreamPlayer2D sound)
			{
				sound.VolumeDb = GameSettings.Instance.GetFinalSfx();
				if (sound.Name == "Jump" && sound.VolumeDb != 0)
				{
					sound.VolumeDb += 5;
				}
			}
		}
	}

	public void Start()
	{
		camera.Shake(2f, 0.2f);
		hasStarted = true;
		Visible = true;
		HealthBar.Visible = true;
	}

	public void OnDeathStart()
	{
		if (GetParent().GetParent() is Level3_2 level)
		{
			level.OnBossDeathStart();
		}
	}
	
	public void OnDeathFinish()
	{
		if (GetParent().GetParent() is Level3_2 level)
		{
			level.OnBossDeathFinish();
		}
	}

}

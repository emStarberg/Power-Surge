using Godot;
using System;

public partial class VoltageSentinel : Enemy
{
	public float Speed = 15.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction facing
	private bool playerDetectedForAttack = false; // Whether player has been detected
	private RayCast2D groundRay, wallRay, playerRayFront, playerRayBack; // Ground detection, wall/object detection, player detection

	private Vector2 startPosition;
	private float walkedDistance = 0f;
	private float walkDistance = 75f; // Distance to walk before turning
	private bool isWalking = true;
	private float stopTime = 2f;
	private float stopTimer = 0f;

	private Player targetPlayer = null;
	private bool isFollowingPlayer = false;
	private bool canDamagePlayer = false;
	private AudioStreamPlayer2D attackSound;
	private AnimatedSprite2D electricityAnimation;
	private PointLight2D light;
	private Camera camera;
	private bool attacking = false;

	public override void _Ready()
	{
		hurtSound = GetNode<AudioStreamPlayer2D>("Sounds/Hurt");
		attackSound = GetNode<AudioStreamPlayer2D>("Sounds/Attack");
		animation = GetNode<AnimatedSprite2D>("Animations");
		groundRay = GetNode<RayCast2D>("RayCasts/GroundRay");
		wallRay = GetNode<RayCast2D>("RayCasts/WallRay");
		playerRayFront = GetNode<RayCast2D>("RayCasts/PlayerRay Front");
		playerRayBack = GetNode<RayCast2D>("RayCasts/PlayerRay Back");
		startPosition = GlobalPosition;
		player = GetParent().GetParent().GetNode<Player>("Player");
		targetPlayer = GetParent().GetParent().GetNode<Player>("Player");
		camera = GetParent().GetParent().GetNode<Camera>("Camera");
		animation.Animation = "walk";
		electricityAnimation = GetNode<AnimatedSprite2D>("Electricity");
		light = electricityAnimation.GetNode<PointLight2D>("Light");
		healAmount = 15;

		maxHealth = 40;
		health = maxHealth;

		if (!GameData.Instance.GlowEnabled)
		{
			light.Visible = false;
		}

		animation.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!isAlive)
			return;

		// update detection rays once per physics frame
		playerRayFront.ForceRaycastUpdate();
		playerRayBack.ForceRaycastUpdate();
		groundRay.ForceRaycastUpdate();
		wallRay.ForceRaycastUpdate();

		// --- apply gravity consistently ---
		// ensure you have 'gravity' and 'maxFallSpeed' from Enemy base
		if (!IsOnFloor())
		{
			velocity.Y = Mathf.Min(velocity.Y + gravity * (float)delta, maxFallSpeed);
		}
		else
		{
			// clear small downward velocity when on floor
			if (velocity.Y > 0f)
				velocity.Y = 0f;
		}

		// Player detection via raycast
		if (playerRayFront.IsColliding() && playerRayFront.GetCollider() is Node2D colliderNode && colliderNode.Name == "Player")
		{
			if (!playerDetectedForAttack)
			{
				if (!attacking)
					Attack();
				playerDetectedForAttack = true;
			}
		}
		else
		{
			playerDetectedForAttack = false;
		}

		// Damage when in attack frames
		if (playerRayFront.IsColliding() && playerRayFront.GetCollider() is Player player && canDamagePlayer)
		{
			player.Hurt(25, 2f, 0.2f);
			canDamagePlayer = false;
		}
		else if (playerRayBack.IsColliding() && playerRayBack.GetCollider() is Player player2 && canDamagePlayer)
		{
			player2.Hurt(25, 2f, 0.2f);
			canDamagePlayer = false;
		}

		// Movement / AI
		if (!attacking)
		{
			if (isFollowingPlayer && targetPlayer != null && animation.Animation != "attack")
			{
				// ensure ground ahead before following
				if (!groundRay.IsColliding())
				{
					isFollowingPlayer = false;
					targetPlayer = null;
					Speed = 15f;
					animation.Animation = "idle";
					animation.Play();
					// leave vertical velocity as-is (gravity already applied)
				}
				else
				{
					Vector2 toPlayer = (targetPlayer.GlobalPosition - GlobalPosition).Normalized();
					velocity.X = toPlayer.X * Speed;
				}
			}
			else if (isWalking && animation.Animation != "attack")
			{
				// Patrol movement
				velocity.X = (direction == "right") ? Speed : -Speed;

				// Wall check - ignore player bodies
				if (wallRay.IsColliding())
				{
					var col = wallRay.GetCollider();
					if (!(col is Node2D node && node.Name == "Player"))
					{
						direction = direction == "right" ? "left" : "right";
						Scale = new Vector2(-Scale.X, Scale.Y);
						walkedDistance = 0f;
						startPosition = GlobalPosition;
					}
				}

				// Ground edge check
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
					velocity.X = 0f;
				}
			}
			else if (animation.Animation != "attack")
			{
				// Stopping/idle phase
				velocity.X = 0f;
				stopTimer -= (float)delta;
				if (stopTimer <= 0f && !isFollowingPlayer)
				{
					direction = direction == "right" ? "left" : "right";
					Scale = new Vector2(-Scale.X, Scale.Y);
					isWalking = true;
					animation.Animation = "walk";
					walkedDistance = 0f;
					startPosition = GlobalPosition;
				}
			}
		}

		Velocity = velocity;
		if(!attacking)
		MoveAndSlide();
	}


	/// <summary>
	/// Attack the player
	/// </summary>

	public void Attack()
	{
		attacking = true;
		isWalking = false;
		animation.Animation = "attack";
		animation.Play();
	}

	/// <summary>
	/// Called when any sentinel animations finish
	/// </summary>
	public void OnAnimationFinished()
	{
		if (animation.Animation == "death")
		{
			CallDeferred("queue_free");
			return;
		}
		if (isAlive)
		{
			if (animation.Animation == "attack")
			{
				playerRayFront.ForceRaycastUpdate();
				playerRayBack.ForceRaycastUpdate();

				bool playerBehind = false;
				if (targetPlayer != null)
				{
					float playerX = targetPlayer.GlobalPosition.X;
					float selfX = GlobalPosition.X;
					playerBehind = (direction == "right" && playerX < selfX) || (direction == "left" && playerX > selfX);
				}
				else
				{
					bool backHits = playerRayBack.IsColliding() && playerRayBack.GetCollider() is Player;
					bool frontHits = playerRayFront.IsColliding() && playerRayFront.GetCollider() is Player;
					playerBehind = backHits && !frontHits;
				}

				if (playerBehind)
				{
					direction = direction == "right" ? "left" : "right";
					Scale = new Vector2(-Scale.X, Scale.Y);
				}

				if (!playerDetectedForAttack)
				{
					attacking = false;
					animation.Animation = "walk";
					animation.Play();
					isWalking = true;
				}
				else
				{
					if (isAlive)
					{
						Attack();
					}
				}
			}
		}
	}

	public void OnElectricityAnimFinished()
	{
		electricityAnimation.Stop();
		electricityAnimation.Visible = false;
	}

	/// <summary>
	/// For detecting the player, if detected should follow player
	/// </summary>
	/// <param name="body">Body detected</param>
	public void OnPlayerDetectionBodyEntered(Node2D body)
	{
		if (body is Player player && !attacking && isAlive)
		{
			Speed = 25f;
			targetPlayer = player;
			isFollowingPlayer = true;
			animation.Animation = "walk";
			animation.Play();

			// Flip direction if player is behind
			float playerX = player.GlobalPosition.X;
			float selfX = GlobalPosition.X;
			if ((direction == "right" && playerX < selfX) || (direction == "left" && playerX > selfX))
			{
				direction = direction == "right" ? "left" : "right";
				Scale = new Vector2(-Scale.X, Scale.Y);
			}
		}
	}

	/// <summary>
	/// Called when a body is no longer detected
	/// </summary>
	/// <param name="body"></param>
	public void OnPlayerDetectionBodyExited(Node2D body)
	{
		if (body == targetPlayer && !attacking && isAlive)
		{
			Speed = 15f;
			isFollowingPlayer = false;
			targetPlayer = null;
		}
	}

	/// <summary>
	/// Called on every animation frame change. For determining when to damage the player
	/// </summary>
	public void OnAnimationFrameChanged()
	{
		if (animation.Animation == "attack" && isAlive)
		{
			// Can only hurt player between frames 7 and 14
			if (animation.Frame == 7)
			{
				electricityAnimation.Visible = true;
				electricityAnimation.Play();
				attackSound.Play();
				camera.Shake(5, 0.4f);
				canDamagePlayer = true;
			}
			else if (animation.Frame == 14)
			{
				canDamagePlayer = false;
			}
		}
		else
		{
			canDamagePlayer = false;
		}
	}

	/// <summary>
	/// Stops getting stuck on top of player
	/// </summary>
	/// <param name="body"></param>
	public void OnFloorCheckBodyEntered(Node2D body)
	{
		if(body is Player)
		{
			Position += new Vector2(20, 0);
		}
	}

	
	public override void UpdateVolume()
	{
		attackSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
		hurtSound.VolumeDb = GameSettings.Instance.GetFinalSfx() + 10;
	}
}

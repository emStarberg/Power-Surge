using Godot;
using System;

public partial class VoltageSentinel : Enemy
{
	public float Speed = 15.0f; // Movement speed
	private Vector2 velocity; // For updating Velocity property
	private string direction = "left"; // Direction facing
	private bool playerDetectedForAttack = false; // Whether player has been detected
	private PackedScene projectile = GD.Load<PackedScene>("Scenes/projectile_cb.tscn"); // For spawning projectiles
	private RayCast2D groundRay, wallRay, playerRay; // Ground detection, wall/object detection, player detection

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

	private Camera camera;

	public override void _Ready()
	{
		hurtSound = GetNode<AudioStreamPlayer2D>("Sounds/Hurt");
		attackSound = GetNode<AudioStreamPlayer2D>("Sounds/Attack");
		animation = GetNode<AnimatedSprite2D>("Animations");
		groundRay = GetNode<RayCast2D>("RayCasts/GroundRay");
		wallRay = GetNode<RayCast2D>("RayCasts/WallRay");
		playerRay = GetNode<RayCast2D>("RayCasts/PlayerRay");
		startPosition = GlobalPosition;
		player = GetParent().GetParent().GetNode<Player>("Player");
		targetPlayer = GetParent().GetParent().GetNode<Player>("Player");
		camera = GetParent().GetParent().GetNode<Camera>("Camera");
		animation.Animation = "walk";

		maxHealth = 30;
		health = maxHealth;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isAlive)
		{


			if (playerRay.IsColliding() && playerRay.GetCollider() is Player player && canDamagePlayer)
			{
				player.Hurt(25, 2f, 0.2f);
				canDamagePlayer = false;
			}


			if (!IsOnFloor())
				velocity += GetGravity() * (float)delta;


			if (isFollowingPlayer && targetPlayer != null && animation.Animation != "attack")
			{
				groundRay.ForceRaycastUpdate();
				if (!groundRay.IsColliding())
				{
					// Stop following if no ground ahead
					isFollowingPlayer = false;
					targetPlayer = null;
					Speed = 15f;
					animation.Animation = "idle";
					animation.Play();
					return; // Exit early so it doesn't move off the edge
				}

				Vector2 toPlayer = (targetPlayer.GlobalPosition - GlobalPosition).Normalized();
				velocity = new Vector2(toPlayer.X * Speed, Velocity.Y);
				Velocity = velocity;
				MoveAndSlide();
			}

			if (isWalking && animation.Animation != "attack")
			{
				if (!isFollowingPlayer)
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
						var Collider = wallRay.GetCollider();
						if (!(Collider is Node2D node && node.Name == "Player"))
						{
							direction = direction == "right" ? "left" : "right";
							Scale = new Vector2(-Scale.X, Scale.Y);
							walkedDistance = 0f;
							startPosition = GlobalPosition;
						}
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
				}

				Velocity = velocity;
				MoveAndSlide();


			}
			else if (animation.Animation != "attack")
			{
				// Stopping phase
				velocity = new Vector2(0, Velocity.Y);
				Velocity = velocity;
				MoveAndSlide();

				stopTimer -= (float)delta;
				if (stopTimer <= 0f && !isFollowingPlayer)
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

			// Player detection via raycast
			playerRay.ForceRaycastUpdate();
			if (playerRay.IsColliding() && playerRay.GetCollider() is Node2D collider && collider.Name == "Player")
			{
				if (!playerDetectedForAttack)
				{
					GD.Print("Player detected by raycast");
					Attack();
					playerDetectedForAttack = true;
				}
			}
			else
			{
				if (playerDetectedForAttack)
				{
					GD.Print("Player lost by raycast");
					playerDetectedForAttack = false;
				}
			}
		}
	}


	/// <summary>
	/// Attack the player
	/// </summary>

	public void Attack()
	{
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
			QueueFree();
		}
		if (animation.Animation == "attack")
		{
			if (!playerDetectedForAttack)
			{
				animation.Animation = "walk";
				animation.Play();
				isWalking = true;
			}
			else
			{
				Attack();
			}
		}
		
	}

	/// <summary>
	/// For detecting the player, if detected should follow player
	/// </summary>
	/// <param name="body">Body detected</param>
	public void OnPlayerDetectionBodyEntered(Node2D body)
	{
		if (body is Player player)
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
		if (body == targetPlayer)
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
		if (animation.Animation == "attack")
		{
			// Can only hurt player between frames 7 and 14
			if (animation.Frame == 7)
			{
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

	
	public override void UpdateVolume()
	{
		attackSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
		hurtSound.VolumeDb = GameSettings.Instance.GetFinalSfx() + 10;
	}
}

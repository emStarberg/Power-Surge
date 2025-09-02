using System;
using Godot;
//------------------------------------------------------------------------------
// <summary>
//   methods for all player controls and animations
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class PlayerMove : CharacterBody2D
{
	[Export] public float Speed = 200f; // Movement speed          
	[Export] public float JumpStrength = -300f; // Jump velocity
	[Export] public float Gravity = 1000f; // Gravity force      
	[Export] public float MaxFallSpeed = 1000f; // Terminal velocity
	[Export] public int MaxPower = 200; // Maximum power level 200%
	[Export] public TextureProgressBar PowerMeter; // Power meter
	[Export] public Label percentageLabel; // Label under power meter
	private int power = 100; // Percentage of power left
	private Vector2 velocity; // For changing player's Velocity property
	private AnimatedSprite2D currentAnim, idleAnim, deathAnim, hurtAnim, dashAnim, weakPulseAnimRight, weakPulseAnimLeft, strongBlastAnim; // Player's animations
	private StaticBody2D shield; // Player's shield ability when activated
	private PackedScene jumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn"); // For spawning jump animations
	private PackedScene dashAnimation = GD.Load<PackedScene>("Scenes/dash_animation.tscn"); // For spawning dash animations
	private int numJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground hit
	private float fallTime = 0f; // For checking if the player has fallen off the map
	private bool alive = true; // True if player has died
	private bool isDashing = false; // Whether player is dashing
	private float dashSpeed = 800f; // Speed of dash
	private float direction = 0.0f; // Direction player is facing (-1 = left, 1 = right)
	private string attackSelected = "weak pulse";
	private string facing = "left";


	public override void _Ready()
	{
		// Set up animations
		hurtAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Hurt");
		deathAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Death");
		dashAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Dash");
		idleAnim = GetNode<AnimatedSprite2D>("Animations/Anim_Idle");
		weakPulseAnimLeft = GetNode<AnimatedSprite2D>("Animations/Attacks/Anim_WeakPulseLeft");
		weakPulseAnimRight = GetNode<AnimatedSprite2D>("Animations/Attacks/Anim_WeakPulseRight");
		currentAnim = idleAnim;
		currentAnim.Play();

		// Set up shield
		shield = GetNode<StaticBody2D>("Shield");
		shield.GetNode<CollisionShape2D>("Collider").Disabled = true;

	}

	public override void _PhysicsProcess(double delta)
	{
		if (alive)
		{
			// Don't allow negative numbers
			if (power < 0)
			{
				power = 0;
			}
			if (power <= 100)
			{
				PowerMeter.Value = power;
				if (PowerMeter is PowerMeter powerMeter)
				{
					if (powerMeter.GetPowerSurgeMode())
					{
						powerMeter.SetPowerSurgeMode(false);
					}
				}

			}
			else
			{
				// PowerMeter is limited to 100, and displays a new sprite when power is greater than 100
				PowerMeter.Value = 100;
				if (PowerMeter is PowerMeter powerMeter)
				{
					if (!powerMeter.GetPowerSurgeMode())
					{
						powerMeter.SetPowerSurgeMode(true);
					}
				}

			}


			// Check whether to dash
			if (Input.IsActionJustPressed("input_dash"))
			{
				// No stationary dashes
				if (velocity.X != 0)
				{
					// Begin dash
					Dash();
				}
			}
			if (isDashing)
			{
				// Continue dashing until animation has finished
				velocity.X = direction * dashSpeed;
				// Ignore gravity during dash
				velocity.Y = 0;
			}
			else
			{
				// Apply gravity to Y velocity.
				velocity.Y += Gravity * (float)delta;
				// Clamp vertical velocity to terminal velocity.
				velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);
				direction = 0;
				// Get input direction
				if (Input.IsActionPressed("input_left"))
				{
					direction -= 1.0f;
					facing = "left";
				}
					
				if (Input.IsActionPressed("input_right"))
				{
					direction += 1.0f;
					facing = "right";
				}
					

				// Handle horizontal movement
				velocity.X = direction * Speed;

				// Handle jump
				if (Input.IsActionJustPressed("input_jump"))
				{
					Jump();
				}

				// Check how long the player has fallen for
				if (!IsOnFloor() && !isDashing)
				{
					fallTime += (float)delta;
				}
				else
					fallTime = 0f;

				if (fallTime > 3f)
				{
					// Die after 3 seconds of fall time
					Die();
				}

				if (Input.IsActionJustPressed("input_shield"))
				{
					// Activate shield
					GetNode<StaticBody2D>("Shield").Visible = true;
					shield.GetNode<CollisionShape2D>("Collider").Disabled = false;
				}
				if (Input.IsActionJustReleased("input_shield"))
				{
					// Deactivate shield
					shield.Visible = false;
					shield.GetNode<CollisionShape2D>("Collider").Disabled = true;
				}
			}

			if (Input.IsActionJustPressed("input_attack"))
			{
				Attack();
			}
			// Update label to correct percentage
			percentageLabel.Text = power + "%";
			// If run out of power, die
			if (power <= 0)
			{
				Die();
			}

			// Update velocity
			Velocity = velocity;
			// Move
			MoveAndSlide();
		}
	}

	/// <summary>
	/// Makes the player jump directly upwards with an animation
	/// </summary>
	public void Jump()
	{
		if (IsOnFloor() || numJumps < 2)
		{
			// Reset number of jumps if on floor
			if (IsOnFloor())
			{
				numJumps = 0;
			}
			// Increase Y velocity
			velocity.Y = JumpStrength;
			// Create jump animation
			Node jumpAnimInstance = jumpAnimation.Instantiate();
			((Node2D)jumpAnimInstance).GlobalPosition = GlobalPosition + new Vector2(2, 0);
			GetTree().Root.AddChild(jumpAnimInstance);
			// Increase no. of jumps, for counting double jumps
			numJumps++;
			DecreasePower(2);
		}
	}

	/// <Summary>
	/// Makes player die by playing animation
	/// </Summary>
	public void Die()
	{
		alive = false;
		GD.Print("Dead!");
		HideAllAnimations();
		deathAnim.Visible = true;
		deathAnim.Play();
	}

	/// <summary>
	/// Play an animation and take damage when hit by enemy attack
	/// </summary>
	/// <param name="damage"> Damage player should take </param>
	/// <param name="shakeAmount"> Camera shake amount </param>
	/// <param name="shakeDuration">Camera shake duration</param>
	public void Hurt(int damage, float shakeAmount, float shakeDuration)
	{
		SwitchAnim(hurtAnim);

		// Camera shake
		var camera = GetParent().GetNode<Camera>("Camera");
		camera.Shake(shakeAmount, shakeDuration);
		DecreasePower(damage);
	}

	/// <Summary>
	/// Resets level once death animation finished
	/// </Summary>
	public void OnDeathAnimFinished()
	{
		deathAnim.Visible = false;
		// Create timer to wait before reloading
		Timer timer = new()
		{
			WaitTime = 0.6f,
			OneShot = true
		};
		AddChild(timer);
		timer.Timeout += () =>
		{
			// Reload scene
			var scenePath = GetTree().CurrentScene.SceneFilePath;
			GetTree().ChangeSceneToFile(scenePath);
		};
		timer.Start();
	}
	/// <Summary>
	/// The player "dashes" left or right depending on the direction they're facing
	/// </Summary>
	public void Dash()
	{
		if (!isDashing && alive)
		{
			SwitchAnim(dashAnim);
			isDashing = true;
			dashAnim.Play();
			Node dashAnimInstance = dashAnimation.Instantiate();
			((Node2D)dashAnimInstance).GlobalPosition = GlobalPosition;
			if (direction == 1)
			{
				dashAnim.FlipH = false;
				((Node2D)dashAnimInstance).Scale = new Vector2(-1, 1); // Flip horizontally
			}
			else
			{
				dashAnim.FlipH = true;
			}
			GetTree().Root.AddChild(dashAnimInstance);
		}
		DecreasePower(3);
	}

	/// <Summary>
	/// When hurt animation has finished, resume idle animation
	/// </Summary>
	public void OnHurtAnimFinished()
	{
		if (alive)
		{
			SwitchAnim(idleAnim);
		}
		else
		{
			hurtAnim.Visible = false;
		}
	}

	/// <Summary>
	/// When dash animation has finished, stop moving and resume idle animation
	/// </Summary>
	public void OnDashAnimFinished()
	{
		isDashing = false;
		velocity.X = 0; // Stop horizontal movement after dash

		if (alive)
		{
			SwitchAnim(idleAnim);
		}
		else
		{
			dashAnim.Visible = false;
		}

	}
	/// <summary>
	/// Decrease player power level
	/// </summary>
	/// <param name="amount">Amount to decrease by</param>
	public void DecreasePower(int amount)
	{
		power -= amount;
	}
	/// <summary>
	/// Increase player power level
	/// </summary>
	/// <param name="amount"></param>
	public void IncreasePower(int amount)
	{
		power += amount;
	}
	/// <summary>
	/// Set Visible = false on all animatedsprite2d
	/// </summary>
	private void HideAllAnimations()
	{
		dashAnim.Visible = false;
		deathAnim.Visible = false;
		idleAnim.Visible = false;
		hurtAnim.Visible = false;
	}

	private void SwitchAnim(AnimatedSprite2D to)
	{
		currentAnim.Visible = false;
		currentAnim.Stop();
		currentAnim = to; 
		currentAnim.Visible = true;
		currentAnim.Play();
	}

	private void Attack()
	{
		if (attackSelected == "weak pulse")
		{
			GD.Print(facing);
			if (facing == "left")
			{
				weakPulseAnimLeft.Visible = true;
				weakPulseAnimLeft.Play();
			}
			else
			{
				weakPulseAnimRight.Visible = true;
				weakPulseAnimRight.Play();
				
			}
		}
	}

	public void OnWeakPulseAnimFinished()
	{
		weakPulseAnimLeft.Visible = false;
		weakPulseAnimRight.Visible = false;
	}
}

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
	[Export] public Node2D animFolder; // Folder where animations are kept
	[Export] public int MaxPower = 200; // Maximum power level 200%
	[Export] public TextureProgressBar PowerMeter; // Power meter
	[Export] public Label percentageLabel;
	private int power = 100; // Percentage of power left
	private Vector2 velocity; // For changing player's Velocity property
	private AnimatedSprite2D idleAnim, deathAnim, hurtAnim, dashAnim; // Player's animations
	private StaticBody2D shield; // Player's shield ability when activated
	private PackedScene jumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn"); // For spawning jump animations
	private PackedScene dashAnimation = GD.Load<PackedScene>("Scenes/dash_animation.tscn"); // For spawning dash animations
	private int numJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground is hit
	private float fallTime = 0f; // For checking if the player has fallen off the map
	private bool alive = true; // True if the player has died
	private bool isDashing = false; // Whether the player is currently dashing
	private float dashSpeed = 800f; // Speed of dash
	private float direction = 0.0f; // Direction player is facing (-1 = left, 1 = right)


	public override void _Ready()
	{
		// Set up animations
		hurtAnim = animFolder.GetNode<AnimatedSprite2D>("Anim_Hurt");
		deathAnim = animFolder.GetNode<AnimatedSprite2D>("Anim_Death");
		dashAnim = animFolder.GetNode<AnimatedSprite2D>("Anim_Dash");
		idleAnim = animFolder.GetNode<AnimatedSprite2D>("Anim_Idle");
		idleAnim.Play();
		// Set up shield
		shield = GetNode<StaticBody2D>("Shield");
		shield.GetNode<CollisionShape2D>("Collider").Disabled = true;

	}

	public override void _PhysicsProcess(double delta)
	{
		if (alive)
		{
			if (power < 100)
			{
				PowerMeter.Value = power;
			}
			else
			{
				PowerMeter.Value = 100;
			}
			// Check whether to dash
			if (Input.IsActionJustPressed("input_dash"))
			{
				// Begin dash
				Dash();
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

				// Get input direction
				direction = 0.0f;
				if (Input.IsActionPressed("input_left"))
					direction -= 1.0f;
				if (Input.IsActionPressed("input_right"))
					direction += 1.0f;

				// Handle horizontal movement
				velocity.X = direction * Speed;

				// Handle jump
				if (Input.IsActionJustPressed("input_jump"))
				{
					Jump();
				}

				// Check how long the player has fallen for
				if (!IsOnFloor())
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
			// Update label to correct percentage
			percentageLabel.Text = PowerMeter.Value.ToString() + "%";
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
			((Node2D)jumpAnimInstance).GlobalPosition = GlobalPosition;
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
		idleAnim.Stop();
		idleAnim.Visible = false;
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
		idleAnim.Visible = false;
		hurtAnim.Visible = true;
		hurtAnim.Play();

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
			idleAnim.Visible = false;
			isDashing = true;
			dashAnim.Visible = true;
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
		hurtAnim.Visible = false;
		hurtAnim.Stop();
		if (alive)
		{
			idleAnim.Visible = true;
		}
	}

	/// <Summary>
	/// When dash animation has finished, stop moving and resume idle animation
	/// </Summary>
	public void OnDashAnimFinished()
	{
		isDashing = false;
		velocity.X = 0; // Stop horizontal movement after dash
		dashAnim.Visible = false;
		idleAnim.Visible = true;
	}
	/// <summary>
	/// Decrease player power level
	/// </summary>
	/// <param name="amount">Amount to decrease by</param>
	public void DecreasePower(int amount)
	{
		power -= amount;
	}
}

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
	[Export] public Node2D _animFolder; // Folder where animations are kept
	private Vector2 velocity; // For changing player's Velocity property
	private AnimatedSprite2D IdleAnim, DeathAnim, HurtAnim, DashAnim; // Player's animations
	private StaticBody2D Shield; // Player's shield ability when activated
	private PackedScene JumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn"); // For spawning jump animations
	private PackedScene DashAnimation = GD.Load<PackedScene>("Scenes/dash_animation.tscn"); // For spawning dash animations
	private int NumJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground is hit
	private float FallTime = 0f; // For checking if the player has fallen off the map
	private bool Alive = true; // True if the player has died
	private bool IsDashing = false; // Whether the player is currently dashing
	private float DashSpeed = 800f; // Speed of dash
	float Direction = 0.0f; // Direction player is facing (-1 = left, 1 = right)
	public override void _Ready()
	{
		// Set up animations
		HurtAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Hurt");
		DeathAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Death");
		DashAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Dash");
		IdleAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Idle");
		IdleAnim.Play();
		// Set up shield
		Shield = GetNode<StaticBody2D>("Shield");
		Shield.GetNode<CollisionShape2D>("Collider").Disabled = true;
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Alive)
		{
			// Check whether to dash
			if (Input.IsActionJustPressed("input_dash"))
			{
				// Begin dash
				Dash();
			}
			if (IsDashing)
			{
				// Continue dashing until animation has finished
				velocity.X = Direction * DashSpeed;
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
				Direction = 0.0f;
				if (Input.IsActionPressed("input_left"))
					Direction -= 1.0f;
				if (Input.IsActionPressed("input_right"))
					Direction += 1.0f;

				// Handle horizontal movement
				velocity.X = Direction * Speed;

				// Handle jump
				if (Input.IsActionJustPressed("input_jump"))
				{
					Jump();
				}

				// Check how long the player has fallen for
				if (!IsOnFloor())
				{
					FallTime += (float)delta;
				}
				else
					FallTime = 0f;

				if (FallTime > 3f)
				{
					// Die after 3 seconds of fall time
					Die();
				}

				if (Input.IsActionJustPressed("input_shield"))
				{
					// Activate shield
					GetNode<StaticBody2D>("Shield").Visible = true;
					Shield.GetNode<CollisionShape2D>("Collider").Disabled = false;
				}
				if (Input.IsActionJustReleased("input_shield"))
				{
					// Deactivate shield
					Shield.Visible = false;
					Shield.GetNode<CollisionShape2D>("Collider").Disabled = true;
				}
			}

			// Update velocity
			Velocity = velocity;
			// Move
			MoveAndSlide();
		}
	}


	/// <Summary>
	/// Makes the player jump directly upwards with an animation
	/// </Summary>
	public void Jump()
	{
		if (IsOnFloor() || NumJumps < 2)
		{
			// Reset number of jumps if on floor
			if (IsOnFloor())
			{
				NumJumps = 0;
			}
			// Increase Y velocity
			velocity.Y = JumpStrength;
			// Create jump animation
			Node jumpAnimInstance = JumpAnimation.Instantiate();
			((Node2D)jumpAnimInstance).GlobalPosition = GlobalPosition;
			GetTree().Root.AddChild(jumpAnimInstance);
			// Increase no. of jumps, for counting double jumps
			NumJumps++;
		}

	}

	/// <Summary>
	/// Makes player die by playing animation
	/// </Summary>
	public void Die()
	{
		Alive = false;
		GD.Print("Dead!");
		IdleAnim.Stop();
		IdleAnim.Visible = false;
		DeathAnim.Visible = true;
		DeathAnim.Play();
	}

	/// <Summary>
	/// Play an animation and take damage when hit by enemy attack
	/// </Summary>
	public void Hurt(float damage, float shakeAmount, float shakeDuration)
	{
		IdleAnim.Visible = false;
		HurtAnim.Visible = true;
		HurtAnim.Play();

		// Camera shake
		var camera = GetParent().GetNode<Camera>("Camera");
		camera.Shake(shakeAmount, shakeDuration);
	}

	/// <Summary>
	/// Resets level once death animation finished
	/// </Summary>
	public void OnDeathAnimFinished()
	{
		DeathAnim.Visible = false;
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
		if (!IsDashing && Alive)
		{
			IdleAnim.Visible = false;
			IsDashing = true;
			DashAnim.Visible = true;
			DashAnim.Play();
			Node dashAnimInstance = DashAnimation.Instantiate();
			((Node2D)dashAnimInstance).GlobalPosition = GlobalPosition;
			if (Direction == 1)
			{
				DashAnim.FlipH = false;
				((Node2D)dashAnimInstance).Scale = new Vector2(-1, 1); // Flip horizontally
			}
			else
			{
				DashAnim.FlipH = true;
			}
			GetTree().Root.AddChild(dashAnimInstance);
		}
	}

	/// <Summary>
	/// When hurt animation has finished, resume idle animation
	/// </Summary>
	public void OnHurtAnimFinished()
	{
		HurtAnim.Visible = false;
		HurtAnim.Stop();
		IdleAnim.Visible = true;
	}

	/// <Summary>
	/// When dash animation has finished, stop moving and resume idle animation
	/// </Summary>
	public void OnDashAnimFinished()
	{
		IsDashing = false;
		velocity.X = 0; // Stop horizontal movement after dash
		DashAnim.Visible = false;
		IdleAnim.Visible = true;
	}
}

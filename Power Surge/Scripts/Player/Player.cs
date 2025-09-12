using System;
using Godot;
//------------------------------------------------------------------------------
// <summary>
//   methods for all player controls and animations
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Player : CharacterBody2D
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
	private AnimatedSprite2D animation; // Player's animations
	private StaticBody2D shield; // Player's shield ability when activated
	private PackedScene jumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn"); // For spawning jump animations
	private PackedScene dashAnimation = GD.Load<PackedScene>("Scenes/dash_animation.tscn"); // For spawning dash animations
	private PackedScene strongBlast = GD.Load<PackedScene>("Scenes/strong_blast.tscn");
	private PackedScene weakPulse = GD.Load<PackedScene>("Scenes/weak_pulse.tscn");
	private int numJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground hit
	private float fallTime = 0f; // For checking if the player has fallen off the map
	private bool alive = true; // True if player has died
	private bool isDashing = false; // Whether player is dashing
	private float dashSpeed = 800f; // Speed of dash
	private float direction = 0.0f; // Direction player is facing (-1 = left, 1 = right)
	private string[] attackNames = { "weak pulse", "strong blast" }; // List of player attacks
	private string attackSelected = "weak pulse";
	private int attackIndex = 0;
	private string facing = "left";
	private Sprite2D attackIcon;


	public override void _Ready()
	{
		// Set up animations
		animation = GetNode<AnimatedSprite2D>("Animations");
		animation.Play();

		// Set up shield
		shield = GetNode<StaticBody2D>("Shield");
		shield.GetNode<CollisionShape2D>("Collider").Disabled = true;

		attackIcon = GetParent().GetNode<Sprite2D>("UI/Control/Attacks-ui/Attack Sprite");
		attackIcon.Texture = GD.Load<Texture2D>("res://Assets/UI/Icons/weak pulse.png");

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

			if (Input.IsActionJustPressed("input_cycle_forward"))
			{
				CycleAttack("forward");
			}

			if (Input.IsActionJustPressed("input_cycle_backward"))
			{
				CycleAttack("backward");
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

			if (Input.IsActionJustPressed("input_attack"))
			{
				Attack();
			}
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
		animation.Animation = "death";
	}

	/// <summary>
	/// Play an animation and take damage when hit by enemy attack
	/// </summary>
	/// <param name="damage"> Damage player should take </param>
	/// <param name="shakeAmount"> Camera shake amount </param>
	/// <param name="shakeDuration">Camera shake duration</param>
	public void Hurt(int damage, float shakeAmount, float shakeDuration)
	{
		animation.Animation = "hurt";

		// Camera shake
		var camera = GetParent().GetNode<Camera>("Camera");
		camera.Shake(shakeAmount, shakeDuration);
		DecreasePower(damage);
	}

	/// <Summary>
	/// The player "dashes" left or right depending on the direction they're facing
	/// </Summary>
	public void Dash()
	{
		if (!isDashing && alive)
		{
			animation.Animation = "dash";
			isDashing = true;
			Node dashAnimInstance = dashAnimation.Instantiate();
			((Node2D)dashAnimInstance).GlobalPosition = GlobalPosition;
			if (direction == 1)
			{
				animation.FlipH = false;
				((Node2D)dashAnimInstance).Scale = new Vector2(-1, 1); // Flip horizontally
			}
			else
			{
				animation.FlipH = true;
			}
			GetTree().Root.AddChild(dashAnimInstance);
		}
		DecreasePower(3);
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
	/// Use selected attack
	/// </summary>
	private void Attack()
	{
		// WEAK PULSE
		if (attackSelected == "weak pulse")
		{
			Node attackInstance = weakPulse.Instantiate();
			((WeakPulse)attackInstance).GlobalPosition = GlobalPosition;
			AddChild(attackInstance);
			if (attackInstance is WeakPulse b)
			{
				b.Activate(facing);
				DecreasePower(5);
			}
		}

		// STRONG BLAST
		if (attackSelected == "strong blast")
		{
			Node attackInstance = strongBlast.Instantiate();
			((StrongBlast)attackInstance).GlobalPosition = GlobalPosition + new Vector2(0, -2);
			GetTree().Root.AddChild(attackInstance); // <-- Add to root, not player
			if (attackInstance is StrongBlast b)
			{
				b.Activate(facing);
				DecreasePower(15);
			}
		}


	}

	/// <summary>
	/// Switches between attacks
	/// </summary>
	/// <param name="direction"></param>
	private void CycleAttack(string direction)
	{
		int index = Array.IndexOf(attackNames, attackSelected);
		if (direction == "forward")
		{
			index++;
			if (index >= attackNames.Length)
			{
				index = 0;
			}
		}
		else if (direction == "backward")
		{
			index--;
			if (index < 0)
			{
				index = attackNames.Length - 1;
			}
		}


		attackSelected = attackNames[index];
		attackIcon.Texture = GD.Load<Texture2D>("res://Assets/UI/Icons/" + attackSelected + ".png");
	}

	public void OnAnimationFinished()
	{
		if (animation.Animation == "hurt")
		{
			if (alive)
			{
				animation.Animation = "idle";
				animation.Play();
			}
			else
			{
				animation.Visible = false;
			}
		}
		else if (animation.Animation == "death")
		{
			animation.Visible = false;
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
		else if (animation.Animation == "dash")
		{
			isDashing = false;
			velocity.X = 0; // Stop horizontal movement after dash

			if (alive)
			{
				animation.Animation = "idle";
				animation.Play();
			}
			else
			{
				animation.Visible = false;
			}
		}
	}
}

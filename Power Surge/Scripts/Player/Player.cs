using System;
using System.Collections.Generic;
using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Methods for all player controls and animations
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f; // Movement speed          
	[Export] public float JumpStrength = -300f; // Jump velocity
	[Export] public float Gravity = 1000f; // Gravity force      
	[Export] public float MaxFallSpeed = 500f; // Terminal velocity
	[Export] public int MaxPower = 200; // Maximum power level 200%
	[Export] public TextureProgressBar PowerMeter; // Power meter
	[Export] public Label percentageLabel; // Label under power meter
	[Export] public Control FragmentSlots;
	[Export] public bool TutorialMode = false, PowerSurgeEnabled = true;

	public string VerticalFacing = "down"; // For when camera is in vertical mode

	private int power = 100; // Percentage of power left
	private AnimatedSprite2D animation; // Player's animations
	private StaticBody2D shield; // Player's shield ability when activated
	private Camera camera;
	private AudioStreamPlayer2D backgroundMusic; // Music is disabled while in power surge mode
	
	// Jump
	private int numJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground hit
	private float fallTime = 0f; // For checking if the player has fallen off the map
	private PackedScene jumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn"); // For spawning jump animations

	// Dash
	private PackedScene dashAnimation = GD.Load<PackedScene>("Scenes/dash_animation.tscn"); // For spawning dash animations
	private float dashSpeed = 800f; // Speed of dash
	private bool isDashing = false, canDash = false; // Player can't dash again without touching ground in between

	// States
	private bool alive = true, powerSurgeActive = false, invincible = false;

	// Direction/Movement
	private float direction = 0.0f; // Direction player is facing (-1 = left, 1 = right)
	private string facing = "right";
	private Vector2 velocity; // For changing player's Velocity property
	
	// Attacks
	private string[] attackNames = {"weak pulse", "strong blast"}; // List of player attacks
	private string attackSelected = "weak pulse";
	private int attackIndex = 0;
	private Sprite2D attackIcon;
	private PackedScene strongBlast = GD.Load<PackedScene>("Scenes/strong_blast.tscn");
	private PackedScene weakPulse = GD.Load<PackedScene>("Scenes/weak_pulse.tscn");
	private PackedScene powerSurgeBlast = GD.Load<PackedScene>("Scenes/power_surge_blast.tscn");
	
	// Fragments
	private int fragmentCount = 0;
	private List<TextureRect> fragmentSlots = new List<TextureRect>();

	// Power Surge
	private Label powerSurgeTimer;
	private float powerSurgeTime = 15f;

	// Audio
	private AudioStreamPlayer2D jumpSound, weakPulseSound, dashSound, hurtSound, strongBlastSound, powerSurgeMusic, fragmentSound, powerSurgeAttackSound;

	// Timers
	private float timer = 0, regenTimer = 0;

	// FOR TUTORIAL
	public List<String> disabledInputs = new List<string>();
	private float mileage = 0; // How far the player has moved left/right
	public bool HasDashed = false, HasJumped = false, HasCycled = false, HasAttacked = false;
	public bool Paused = false;


	public override void _Ready()
	{
		// Set up sounds
		jumpSound = GetNode<AudioStreamPlayer2D>("Sounds/Jump");
		weakPulseSound = GetNode<AudioStreamPlayer2D>("Sounds/Weak Pulse");
		strongBlastSound = GetNode<AudioStreamPlayer2D>("Sounds/Strong Blast");
		dashSound = GetNode<AudioStreamPlayer2D>("Sounds/Dash");
		hurtSound = GetNode<AudioStreamPlayer2D>("Sounds/Hurt");
		powerSurgeMusic = GetNode<AudioStreamPlayer2D>("Sounds/Power Surge");
		fragmentSound = GetNode<AudioStreamPlayer2D>("Sounds/Collect Fragment");
		powerSurgeAttackSound = GetNode<AudioStreamPlayer2D>("Sounds/Power Surge Attack");
		backgroundMusic = GetParent().GetNode<AudioStreamPlayer2D>("Background Music");
		// Set up animations
		animation = GetNode<AnimatedSprite2D>("Animations");
		animation.Play();

		// Set up shield
		shield = GetNode<StaticBody2D>("Shield");
		shield.GetNode<CollisionShape2D>("Collider").Disabled = true;

		attackIcon = GetParent().GetNode<Sprite2D>("UI/Control/Attacks-ui/Attack Sprite");
		attackIcon.Texture = GD.Load<Texture2D>("res://Assets/UI/Icons/weak pulse.png");

		camera = GetParent().GetNode<Camera>("Camera");

		UpdateVolume();

		// Load all empty fragment slots
		foreach (Node child in FragmentSlots.GetChildren())
		{
			if (child is TextureRect t)
			{
				fragmentSlots.Add(t);
			}
		}

		powerSurgeTimer = GetNode<Label>("Timer");
		powerSurgeTimer.Visible = false;

		// Disable glow light for outdoor levels
		GetNode<PointLight2D>("Light").Visible = GameData.Instance.GlowEnabled;		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (camera.IsPanning())
		{
			Paused = true;
		}
		if (alive)
		{
			// Apply gravity to Y velocity.
			velocity.Y += Gravity * (float)delta;
			// Clamp vertical velocity to terminal velocity.
			velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);
			// Update velocity
			Velocity = velocity;
			// Move
			MoveAndSlide();


			// Regens slowly if standing still
			if (!Paused && Velocity == new Vector2(0, 0))
			{
				regenTimer += (float)delta;
				if (regenTimer >= 2f && power < 100)
				{
					power++;
					regenTimer = 0f;
				}
			}

			if (Paused)
			{
				velocity.X = 0;
				return;
			}
			if (power < 0)
			{
				power = 0;
			}
			if (alive)
			{
				// Don't allow negative numbers
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
					if (powerSurgeActive)
					{
						StopPowerSurgeTimer();
					}
				}
				else
				{
					// PowerMeter is limited to 100, and displays a new sprite when power is greater than 100
					PowerMeter.Value = 100;
					if (PowerMeter is PowerMeter powerMeter && PowerSurgeEnabled)
					{
						if (!powerMeter.GetPowerSurgeMode())
						{
							powerMeter.SetPowerSurgeMode(true);
						}
						if (!powerSurgeActive)
						{
							StartPowerSurgeTimer();
						}
					}

				}
				if (powerSurgeActive)
				{
					powerSurgeTime -= (float)delta;
					if (powerSurgeTime < 0)
						powerSurgeTime = 0;

					powerSurgeTimer.Text = ((int)Math.Ceiling(powerSurgeTime)).ToString();

					if (powerSurgeTime <= 0)
					{
						Die();
					}
				}

				// Check whether to dash
				if (Input.IsActionJustPressed("input_dash") && !disabledInputs.Contains("input_dash"))
				{
					// No stationary dashes
					if (velocity.X != 0 && canDash)
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
					direction = 0;
					// Get input direction
					if (Input.IsActionPressed("input_left") && !disabledInputs.Contains("input_left"))
					{
						direction -= 1.0f;
						mileage++; // For tutorial
						facing = "left";
					}

					if (Input.IsActionPressed("input_right") && !disabledInputs.Contains("input_left"))
					{
						direction += 1.0f;
						mileage++; // For tutorial
						facing = "right";
					}


					// Handle horizontal movement
					velocity.X = direction * Speed;

					// Handle jump
					if (Input.IsActionJustPressed("input_jump") && !disabledInputs.Contains("input_jump"))
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

					if (fallTime > 2.5f)
					{
						// Die after 3 seconds of fall time
						Die();
					}

					if (Input.IsActionJustPressed("input_shield") && !disabledInputs.Contains("input_shield"))
					{
						// Activate shield
						/*GetNode<StaticBody2D>("Shield").Visible = true;
						shield.GetNode<CollisionShape2D>("Collider").Disabled = false;*/
					}
					if (Input.IsActionJustReleased("input_shield"))
					{
						// Deactivate shield
						shield.Visible = false;
						shield.GetNode<CollisionShape2D>("Collider").Disabled = true;
					}
				}

				if (Input.IsActionJustPressed("input_cycle_forward") && !disabledInputs.Contains("input_cycle_forward"))
				{
					CycleAttack("forward");
				}

				if (Input.IsActionJustPressed("input_cycle_backward") && !disabledInputs.Contains("input_cycle_backward"))
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

				if (Input.IsActionJustPressed("input_attack") && !disabledInputs.Contains("input_attack"))
				{
					if (powerSurgeActive)
					{
						PowerSurgeAttack();
					}
					else
					{
						Attack();
					}
				}
				if (Velocity.Y > 300f)
				{
					VerticalFacing = "down";
				}
				if (IsOnFloor() && !canDash)
				{
					canDash = true;
				}
			}
		}
	}

	/// <summary>
	/// Makes the player jump directly upwards with an animation
	/// </summary>
	public void Jump()
	{
		VerticalFacing = "up";
		// For tutorial
		if (!HasJumped)
		{
			HasJumped = true;
		}
		if (IsOnFloor() || numJumps < 2)
		{
			jumpSound.Play();
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
			DecreasePower(1);
		}
	}

	/// <Summary>
	/// Makes player die by playing animation
	/// </Summary>
	public void Die()
	{
		alive = false;
		isDashing = false;
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
		if (!invincible)
		{
			if(!powerSurgeActive)
			animation.Animation = "hurt";
			hurtSound.Play();
			// Camera shake
			camera.Shake(shakeAmount, shakeDuration);
			DecreasePower(damage);
		}
	}

	/// <Summary>
	/// The player "dashes" left or right depending on the direction they're facing
	/// </Summary>
	public void Dash()
	{
		fallTime = 0;
		invincible = true;
		// For tutorial
		if (!HasDashed)
		{
			HasDashed = true;
		}
		if (!isDashing && alive && canDash)
		{
			canDash = false;
			dashSound.Play();
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
		camera.Shake(2, 0.2f);
		if (power > 100 && !PowerSurgeEnabled)
		{
			power = 100;
		}
	}


	/// <summary>
	/// Use selected attack
	/// </summary>
	private void Attack()
	{
		// For tutorial
		if (!HasAttacked)
		{
			HasAttacked = true;
		}
		// WEAK PULSE
		if (attackSelected == "weak pulse")
		{
			weakPulseSound.Play();
			Node attackInstance = weakPulse.Instantiate();
			((WeakPulse)attackInstance).GlobalPosition = GlobalPosition + new Vector2(0,5);
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
			jumpSound.Play();
			strongBlastSound.Play();
			Node attackInstance = strongBlast.Instantiate();
			((StrongBlast)attackInstance).GlobalPosition = GlobalPosition + new Vector2(0, -2);
			GetTree().Root.AddChild(attackInstance);
			if (attackInstance is StrongBlast b)
			{
				b.Activate(facing);
				DecreasePower(10);
			}
		}
	}

	/// <summary>
	/// Switches between attacks
	/// </summary>
	/// <param name="direction"></param>
	private void CycleAttack(string direction)
	{
		// For tutorial
		if (!HasCycled)
		{
			HasCycled = true;
		}
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

	/// <summary>
	/// Called when any player animation finishes
	/// </summary>
	public void OnAnimationFinished()
	{
		if (alive)
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
			if (animation.Animation == "dash")
			{
				invincible = false;
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
		if (animation.Animation == "death")
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
				if (!TutorialMode)
				{
					// Reload scene
					var scenePath = GetTree().CurrentScene.SceneFilePath;
					GetTree().ChangeSceneToFile(scenePath);
				}
				else
				{
					animation.Animation = "idle";
					alive = true;
					power = 100;
					animation.Visible = true;
					animation.Play();
				}
			};

			timer.Start();
		}

	}

	/// <summary>
	/// Called when the player collects a fragment
	/// </summary>
	public void AddFragment()
	{
		var camera = GetParent().GetNode<Camera>("Camera");
		fragmentSound.Play();
		fragmentSlots[fragmentCount].Texture = (Texture2D)GD.Load("res://Assets/Objects/Fragment - Filled Slot.png");
		fragmentCount++;
		camera.Shake(2, 0.2f);
		if (fragmentCount == 3 && power < 100)
		{
			power = 100;
			camera.Shake(6, 0.2f);
		}
	}
	/// <summary>
	/// Start the power surge timer
	/// </summary>
	public void StartPowerSurgeTimer()
	{
		backgroundMusic.Stop();
		animation.Visible = false;
		animation = GetNode<AnimatedSprite2D>("Animations - Power Surge");
		animation.Visible = true;
		Scale = new Vector2(1.3f, 1.3f);
		Speed = 300f;
		JumpStrength -= 50;
		powerSurgeMusic.Play();
		powerSurgeTime = 15f;
		powerSurgeActive = true;
		powerSurgeTimer.Visible = true;
	}
	/// <summary>
	/// Stop the power surge timer
	/// </summary>
	public void StopPowerSurgeTimer()
	{
		if (isDashing)
		{
			isDashing = false;
		}
		backgroundMusic.Play();
		animation.Visible = false;
		animation = GetNode<AnimatedSprite2D>("Animations");
		animation.Visible = true;
		Scale = new Vector2(1f, 1f);
		Speed = 200f;
		JumpStrength += 50;
		powerSurgeMusic.Stop();
		powerSurgeActive = false;
		powerSurgeTimer.Visible = false;
	}

	/// <summary>
	/// Disable any number of input mappings
	/// </summary>
	/// <param name="inputs">Names of mappings to be disabled</param>
	public void DisableInputs(params string[] inputs)
	{
		foreach (var input in inputs)
		{
			if (!disabledInputs.Contains(input))
			{
				disabledInputs.Add(input);
			}
		}
	}

	/// <summary>
	/// Enable any number of input mappings
	/// </summary>
	/// <param name="input">Names of mappings to be enabled</param>
	public void EnableInputs(params string[] inputs)
	{
		foreach (var input in inputs)
		{
			disabledInputs.Remove(input);
		}

	}

	/// <summary>
	/// Get total distance moved left/right
	/// </summary>
	/// <returns>mileage</returns>
	public float GetMileage()
	{
		return mileage;
	}

	public bool IsAlive()
	{
		return alive;
	}

	public int GetFragmentCount()
	{
		return fragmentCount;
	}

	public float GetPower()
	{
		return power;
	}

	/// <summary>
	/// Use the power surge attack
	/// </summary>
	public void PowerSurgeAttack()
	{
			powerSurgeAttackSound.Play();
			Node attackInstance = powerSurgeBlast.Instantiate();
			((PowerSurgeBlast)attackInstance).GlobalPosition = GlobalPosition + new Vector2(0,5);
			AddChild(attackInstance);
			if (attackInstance is PowerSurgeBlast b)
			{
				b.Activate(facing);
				DecreasePower(5);
			}
	}

	public string GetDirection()
	{
		return facing;
	}
	
	public void UpdateVolume()
	{
		foreach (Node node in GetNode<Node2D>("Sounds").GetChildren())
		{
			if (node is AudioStreamPlayer2D sound)
			{
				if (node.Name != "Power Surge")
				{
					sound.VolumeDb = GameSettings.Instance.GetFinalSfx();
				}
				else
				{
					sound.VolumeDb = GameSettings.Instance.GetFinalMusic();
				}
			}
		}
	}
}

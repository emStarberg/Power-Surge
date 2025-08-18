using Godot;
/**
	Contains methods for all player controls and animations
*/
public partial class PlayerMove : CharacterBody2D
{
	[Export] public float Speed = 200f; // Movement speed          
	[Export] public float JumpStrength = -300f; // Jump velocity
	[Export] public float Gravity = 1000f; // Gravity force      
	[Export] public float MaxFallSpeed = 1000f; // Terminal velocity
	[Export] public Node2D _animFolder;
	private Vector2 velocity;
	private AnimatedSprite2D IdleAnim, JumpAnim, DeathAnim, HurtAnim;
	private PackedScene JumpAnimation = GD.Load<PackedScene>("Scenes/jump_animation.tscn");
	private int NumJumps = 0; // For deciding whether a mid air jump is allowed, resets when ground is hit

	public override void _Ready()
	{
		//JumpAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Jump");
		IdleAnim = _animFolder.GetNode<AnimatedSprite2D>("Anim_Idle");
		IdleAnim.Play();
	}


	public override void _PhysicsProcess(double delta)
	{
		// Apply gravity to Y velocity.
		velocity.Y += Gravity * (float)delta;

		// Clamp vertical velocity to terminal velocity.
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Get input direction
		float direction = 0.0f;
		if (Input.IsActionPressed("input_left"))
			direction -= 1.0f;
		if (Input.IsActionPressed("input_right"))
			direction += 1.0f;

		// Handle horizontal movement
		velocity.X = direction * Speed;

		// Handle jump
		if (Input.IsActionJustPressed("input_jump"))
		{
			Jump(); // Jump
		}

		// Update velocity
		Velocity = velocity;
		// Move
		MoveAndSlide();
	}

	
	/// <Summary>
	/// Makes the player jump directly upwards with an animation
	/// </Summary>
	public void Jump()
	{
		if (IsOnFloor() || NumJumps < 2)
		{
			// Reset the number of jumps if on floor
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
}

using Godot;

public partial class PlayerMove : CharacterBody2D
{
	[Export] public float Speed = 200f;          // Horizontal movement speed
	[Export] public float JumpStrength = -300f;  // Upward velocity when jumping
	[Export] public float Gravity = 1000f;       // Gravity force
	[Export] public float MaxFallSpeed = 1000f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Apply gravity
		// Apply gravity to Y velocity.
		velocity.Y += Gravity * (float)delta;

		// Clamp vertical velocity to terminal velocity.
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Get input direction
		float direction = 0.0f;
		if (Input.IsActionPressed("ui_left"))
			direction -= 1.0f;
		if (Input.IsActionPressed("ui_right"))
			direction += 1.0f;

		// Handle horizontal movement
		velocity.X = direction * Speed;

		// Handle jump
		if (Input.IsActionJustPressed("ui_up"))
		{
			if (IsOnFloor())
			{
				GD.Print("here");
				velocity.Y = JumpStrength; // Set upward velocity for jump.
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}

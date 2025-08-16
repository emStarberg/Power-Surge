using Godot;

public partial class PlayerMove : CharacterBody2D
{
	[Export] public float Speed = 200f; // Movement speed          
	[Export] public float JumpStrength = -300f; // Jump velocity
	[Export] public float Gravity = 1000f; // Gravity force      
	[Export] public float MaxFallSpeed = 1000f; // Terminal velocity

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

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
			if (IsOnFloor())
			{
				GD.Print("here");
				// Jump
				velocity.Y = JumpStrength; 
			}
		}
		// Update velocity
		Velocity = velocity;
		// Move
		MoveAndSlide();
	}
}

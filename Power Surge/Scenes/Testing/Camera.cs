using Godot;

public partial class Camera : Camera2D
{
	[Export]
	public NodePath PlayerPath; // Path to the player node
	private Node2D _player; // Reference to the player node

	public override void _Ready()
	{
		Zoom = new Vector2(2.5f,2.5f);
		_player = GetParent().GetNode<Node2D>("Player");
		Offset = Offset = new Vector2(0, -25);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player != null)
		{
			// Move the camera to follow the player's position
			Position = _player.Position;
		}
	}
}

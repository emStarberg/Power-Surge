using Godot;

public partial class Background : Node2D
{
	[Export] public NodePath CameraPath;
	private Camera2D _camera;

	public override void _Ready()
	{
		if (CameraPath != null)
		{
			_camera = GetNode<Camera2D>(CameraPath);
		}
	}

	public override void _Process(double delta)
	{
		if (_camera != null)
		{
			// Match background position to camera position
			GlobalPosition = _camera.GlobalPosition +_camera.Offset;
		}
	}
}

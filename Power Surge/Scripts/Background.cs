using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Makes the background follow the camera
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Background : Node2D
{
	[Export] public NodePath cameraPath;
	private Camera2D _camera;

	public override void _Ready()
	{
		if (cameraPath != null)
		{
			_camera = GetNode<Camera2D>(cameraPath);
		}
	}

	public override void _Process(double delta)
	{
		if (_camera != null)
		{
			GlobalPosition = _camera.GlobalPosition + _camera.Offset;
		}
	}
}

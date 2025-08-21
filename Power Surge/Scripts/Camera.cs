using System;
using Godot;

public partial class Camera : Camera2D
{
	[Export]
	public NodePath PlayerPath; // Path to the player node
	private Node2D _player; // Reference to the player node
	private float ShakeAmount = 0f;
	private float ShakeTime = 0f;
	private Random random = new();
	private Vector2 BaseOffset = new Vector2(0, -25);

	public override void _Ready()
	{
		Zoom = new Vector2(3f, 3f);
		_player = GetParent().GetNode<Node2D>("Player");
		Offset = BaseOffset;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player != null)
		{
			// Move the camera to follow the player's position
			Position = _player.Position;
		}
		if (ShakeTime > 0)
		{
			ShakeTime -= (float)delta;
			var shakeOffset = new Vector2(
				(float)(random.NextDouble() * 2 - 1) * ShakeAmount,
				(float)(random.NextDouble() * 2 - 1) * ShakeAmount
			);
			Offset = BaseOffset + shakeOffset;
			if (ShakeTime <= 0)
				Offset = BaseOffset;
		}
		else
		{
			Offset = BaseOffset;
		}
	}
	
	public void Shake(float amount = 10f, float duration = 0.2f)
	{
		ShakeAmount = amount;
		ShakeTime = duration;
	}
}

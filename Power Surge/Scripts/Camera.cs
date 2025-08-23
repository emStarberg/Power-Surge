using System;
using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Camera class for game. Follows the player and contains a method to create a shake effect
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Camera : Camera2D
{
	[Export]
	public NodePath playerPath; // Path to player node
	private Node2D _player; // Reference to player node
	private float shakeAmount, shakeTime = 0f; // Parameters for camera shake effect
	private Random random = new(); // Random number for generating shake effect
	private Vector2 baseOffset = new Vector2(0, -15); // Camera offset from player pos

	public override void _Ready()
	{
		Zoom = new Vector2(3f, 3f);
		_player = GetParent().GetNode<Node2D>("Player");
		Offset = baseOffset;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player != null)
		{
			// Move the camera to follow the player's position
			Position = _player.Position;
		}
		// Shake the camera when required
		if (shakeTime > 0)
		{
			shakeTime -= (float)delta;
			var shakeOffset = new Vector2(
				(float)(random.NextDouble() * 2 - 1) * shakeAmount,
				(float)(random.NextDouble() * 2 - 1) * shakeAmount
			);
			Offset = baseOffset + shakeOffset;
			if (shakeTime <= 0)
				Offset = baseOffset;
		}
		else
		{
			Offset = baseOffset;
		}
	}
	/// <summary>
	/// Creates a camera shake effect
	/// </summary>
	/// <param name="amount">Camera shake amount</param>
	/// <param name="duration">Camera shake duration</param>
	public void Shake(float amount = 10f, float duration = 0.2f)
	{
		shakeAmount = amount;
		shakeTime = duration;
	}
}

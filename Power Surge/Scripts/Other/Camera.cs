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
	private Player _player; // Reference to player node
	private float shakeAmount, shakeTime = 0f; // Parameters for camera shake effect
	private Random random = new(); // Random number for generating shake effect
	private Vector2 baseOffset = new Vector2(0, 0); // Camera offset from player pos
	private Vector2 targetOffset = Vector2.Zero;
	private float offsetLerpSpeed = 2.5f; // Higher = snappier, lower = smoother

	public override void _Ready()
	{
		Zoom = new Vector2(2.4f, 2.4f);
		_player = GetParent().GetNode<Player>("Player");
		Offset = baseOffset;
		MakeCurrent();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player != null)
		{
			float halfVisibleWidth = GetViewportRect().Size.X * 0.5f / Zoom.X;
			float halfVisibleHeight = GetViewportRect().Size.Y * 0.5f / Zoom.Y;

			// X axis: clamp as before
			float newX = Position.X;
			if (_player.Position.X > LimitLeft + halfVisibleWidth && _player.Position.X < LimitRight - halfVisibleWidth)
				newX = _player.Position.X;

			Position = new Vector2(newX, _player.Position.Y);
			
			string facing = "right";
			facing = _player.GetDirection();

			if (facing == "right")
				targetOffset = new Vector2(25, 0);
			else if (facing == "left")
				targetOffset = new Vector2(-25, 0);
			else
				targetOffset = Vector2.Zero;

			// Smoothly interpolate baseOffset towards targetOffset
			baseOffset = baseOffset.Lerp(targetOffset, offsetLerpSpeed * (float)delta);
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

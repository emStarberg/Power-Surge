using Godot;
using System;

public partial class UICamera : Camera2D
{
	private float shakeAmount, shakeTime = 0f; // Parameters for camera shake effect
	private Random random = new(); // Random number for generating shake effect
	private Vector2 baseOffset = new Vector2(0, -15); // Camera offset
	public override void _PhysicsProcess(double delta)
	{

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

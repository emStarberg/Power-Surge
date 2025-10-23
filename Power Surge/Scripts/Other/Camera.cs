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
	public string Mode = "horizontal"; // horizontal, vertical, still
	private float shakeAmount, shakeTime = 0f; // Parameters for camera shake effect
	private Random random = new(); // Random number for generating shake effect
	private Vector2 baseOffset = new Vector2(0, 0); // Camera offset from player pos
	private Vector2 targetOffset = Vector2.Zero;
	private float offsetLerpSpeed = 2.5f;
	private string facingVertical = "down";
	private float centerY = 200;
	// Pan state
	private bool isPanning = false;
	private Vector2 panTarget = Vector2.Zero;
	private float panLerpSpeed = 3.5f;
	private string savedMode = null;
	private bool transitioningToCentered = false;
	

	public override void _Ready()
	{
		Zoom = new Vector2(2.1f, 2.1f);
		_player = GetParent().GetNode<Player>("Player");
		Offset = baseOffset;
		MakeCurrent();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player != null)
		{
			if (isPanning)
			{
				if (transitioningToCentered)
				{
					float newX = _player.Position.X;
					float newY = Mathf.Lerp(Position.Y, panTarget.Y, panLerpSpeed * (float)delta);
					Position = new Vector2(newX, newY);

					if (Mathf.Abs(Position.Y - panTarget.Y) < 1.0f)
					{
						Position = new Vector2(newX, panTarget.Y);
						isPanning = false;
						Mode = "centered";
						transitioningToCentered = false;
					}
				}
				else
				{
					Position = Position.Lerp(panTarget, panLerpSpeed * (float)delta);
					if (Position.DistanceTo(panTarget) < 1.0f)
					{
						Position = panTarget;
						isPanning = false;
					}
				}
			}
			else
			{
				switch (Mode)
				{
					case "horizontal":
						HorizontalMode(delta);
						break;
					
					case "vertical":
						VerticalMode(delta);
						break;

					case "centered":
						CenteredMode(delta);
						break;

					default:
						break;
				}
			}
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

	/// <summary>
	/// For when the player is in a horizontal camera area
	/// </summary>
	/// <param name="delta">_Process</param>
	public void HorizontalMode(double delta)
	{
		offsetLerpSpeed = 2.5f;
		float halfVisibleWidth = GetViewportRect().Size.X * 0.5f / Zoom.X;

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

	/// <summary>
	/// For when the player is in a vertical camera area
	/// </summary>
	/// <param name="delta">_Process</param>
	public void VerticalMode(double delta)
	{
		offsetLerpSpeed = 1.0f;
		float halfVisibleWidth = GetViewportRect().Size.X * 0.5f / Zoom.X;

		float newX = Position.X;
		if (_player.Position.X > LimitLeft + halfVisibleWidth && _player.Position.X < LimitRight - halfVisibleWidth)
			newX = _player.Position.X;

		Position = new Vector2(newX, _player.Position.Y);

		if (_player.VerticalFacing == "down")
			targetOffset = new Vector2(0, 60);
		else if (_player.VerticalFacing == "up")
			targetOffset = new Vector2(0, -60);
		else
			targetOffset = Vector2.Zero;

		// Smoothly interpolate baseOffset towards targetOffset
		baseOffset = baseOffset.Lerp(targetOffset, offsetLerpSpeed * (float)delta);
	}

	public void CenteredMode(double delta)
	{
		float ySmoothSpeed = 3.5f;
		float targetX = _player.Position.X;
		float targetY = centerY;
		float newY = Mathf.Lerp(Position.Y, targetY, ySmoothSpeed * (float)delta);
		Position = new Vector2(targetX, newY);
	}


	/// <summary>
	/// Change to centered mode by first panning smoothly to the centered position, then letting CenteredMode take over.
	/// </summary>
	public void ChangeToCentered(float lerpSpeed = 4.5f, bool immediate = false)
	{
		if (_player == null)
			return;

		if (!isPanning)
			savedMode = Mode;

		transitioningToCentered = true;
		panLerpSpeed = lerpSpeed;
		panTarget = new Vector2(_player.Position.X, centerY);

		if (immediate)
		{
			Position = panTarget;
			Mode = "centered";
			transitioningToCentered = false;
		}
	}
	
	/// <summary>
	/// Pan the camera smoothly to the given world-space focus position.
	/// Call CancelPan() to return to the previous follow behaviour.
	/// </summary>
	public void Pan(Vector2 focus, float lerpSpeed = 3.5f, bool immediate = false)
	{
		if (!isPanning)
			savedMode = Mode;
		isPanning = true;
		panTarget = focus;
		panLerpSpeed = lerpSpeed;
		if (immediate)
			Position = panTarget;
	}

	/// <summary>
	/// Cancel pan and resume previous behaviour.
	/// </summary>
	public void CancelPan()
	{
		isPanning = false;
		if (!string.IsNullOrEmpty(savedMode))
			Mode = savedMode;
		savedMode = null;
	}

	/// <summary>
	/// Return whether the pan has completed or not
	/// </summary>
	public bool PanCompleted()
	{
		return Position == panTarget;
	}

	public bool IsPanning()
	{
		return isPanning;
	}

	public void SetCenterY(float num)
	{
		centerY = num;
	}

	public float GetCenterY()
	{
		return centerY;
	}
	
}

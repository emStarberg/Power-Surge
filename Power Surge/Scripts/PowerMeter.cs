using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Power meter displaying how much power the player has left. Has a spark animation that gets more frequent the more power there is
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class PowerMeter : TextureProgressBar
{
	[Export] public Camera Camera;
	// All possible animation positions
	private Vector2 pos1 = new(2, 10);
	private Vector2 pos2 = new(10, 10);
	private Vector2 pos3 = new(18, 10);
	private Vector2 pos4 = new(26, 10);
	private Vector2 pos5 = new(34, 10);
	private Vector2 pos6 = new(42, 10);
	private PackedScene sparkAnimation = GD.Load<PackedScene>("Scenes/ui_spark.tscn");// For spawning animations along the power meter
	private float loopWaitTime;
	private List<Vector2> sparkPositions;
	private List<float> sparkThresholds;
	private int sparkIndex = 0;
	private Timer sparkTimer;
	private Timer loopTimer;
	private AnimatedSprite2D powerSurgeAnim;
	private bool powerSurgeMode = false;

	public override void _Ready()
	{
		powerSurgeAnim = GetNode<AnimatedSprite2D>("Power Surge");
		sparkPositions = new List<Vector2> { pos1, pos2, pos3, pos4, pos5, pos6 };
		sparkThresholds = new List<float> { 0f, 16.6f, 33.3f, 50f, 66.6f, 83.3f };

		sparkTimer = new Timer();
		sparkTimer.WaitTime = 0.2f;
		sparkTimer.OneShot = false;
		AddChild(sparkTimer);
		sparkTimer.Timeout += OnSparkTimerTimeout;

		loopTimer = new Timer();
		loopTimer.WaitTime = 3f;
		loopTimer.OneShot = true;
		AddChild(loopTimer);
		loopTimer.Timeout += OnLoopTimerTimeout;
	}

	public override void _Process(double delta)
	{
		UpdateLoopWaitTime();

		// Stop timers and animations if powerSurgeMode is true
		if (powerSurgeMode)
		{
			if (sparkTimer != null && !sparkTimer.IsStopped())
				sparkTimer.Stop();
			if (loopTimer != null && !loopTimer.IsStopped())
				loopTimer.Stop();
		}
		else
		{
			if (sparkTimer != null && loopTimer != null && sparkTimer.IsStopped() && loopTimer.IsStopped())
			{
				StartSparkSequence();
			}
		}
	}

	/// <summary>
	/// Change time between animations depending on power level
	/// </summary>
	private void UpdateLoopWaitTime()
	{
		if (loopTimer == null)
			return;

		if (Value > 83.3)
		{
			loopWaitTime = 0.8f;
		}
		else if (Value > 66.6f)
		{
			loopWaitTime = 1.5f;
		}
		else if (Value > 33.3f)
		{
			loopWaitTime = 2f;
		}
		else
		{
			loopWaitTime = 3.5f;
		}

		loopTimer.WaitTime = loopWaitTime;
	}
	/// <summary>
	/// Start displaying the spark animations
	/// </summary>
	private void StartSparkSequence()
	{
		sparkIndex = 0;
		sparkTimer.Start();
	}
	/// <summary>
	/// Spawn animation with correct timing
	/// </summary>
	private void OnSparkTimerTimeout()
	{
		if (sparkIndex < sparkPositions.Count)
		{
			if (Value >= sparkThresholds[sparkIndex])
			{
				SpawnSpark(sparkPositions[sparkIndex]);
			}
			sparkIndex++;
		}
		else
		{
			sparkTimer.Stop();
			loopTimer.Start();
		}
	}
	/// <summary>
	/// Start animation spawns with corect timing
	/// </summary>
	private void OnLoopTimerTimeout()
	{
		StartSparkSequence();
	}
	/// <summary>
	/// Spawn spark animation
	/// </summary>
	/// <param name="pos">Position to spawn in</param>
	private void SpawnSpark(Vector2 pos)
	{
		Node sparkInstance = sparkAnimation.Instantiate();
		((Node2D)sparkInstance).Position = pos;
		((Node2D)sparkInstance).Scale = new Vector2(0.65f, 0.65f);
		CallDeferred("add_child", sparkInstance);
	}

	public void SetPowerSurgeMode(bool b)
	{
		powerSurgeMode = b;
		if (b)
		{
			powerSurgeAnim.Visible = true;
			powerSurgeAnim.Play();
			Camera.Shake(10, 0.2f);
		}
		else
		{
			powerSurgeAnim.Visible = false;
			powerSurgeAnim.Stop();
		}
	}

	public bool GetPowerSurgeMode()
	{
		return powerSurgeMode;
	}
}

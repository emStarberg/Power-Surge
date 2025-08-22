using Godot;
using System;
using System.Collections.Generic;

public partial class PowerMeter : TextureProgressBar
{
	private Vector2 pos1 = new(2, 10);
	private Vector2 pos2 = new(10, 10);
	private Vector2 pos3 = new(18, 10);
	private Vector2 pos4 = new(26, 10);
	private Vector2 pos5 = new(34, 10);
	private Vector2 pos6 = new(42, 10);
	private PackedScene sparkAnimation = GD.Load<PackedScene>("Scenes/ui_spark.tscn");
	private float loopWaitTime;

	private List<Vector2> sparkPositions;
	private List<float> sparkThresholds;
	private int sparkIndex = 0;
	private Timer sparkTimer;
	private Timer loopTimer;

	public override void _Ready()
	{
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

		StartSparkSequence();
	}

	public override void _Process(double delta)
	{
		UpdateLoopWaitTime();
	}


	private void UpdateLoopWaitTime()
	{
		if (Value > 83.3)
		{
			loopWaitTime = 0.1f;
		}
		else if (Value > 66.6f)
		{
			loopWaitTime = 0.4f;
		}
		else if (Value > 50f)
		{
			loopWaitTime = 1.0f;
		}
		else if (Value > 33.3f)
		{
			loopWaitTime = 1.7f;
		}
		else if (Value > 16.6f)
		{
			loopWaitTime = 2.2f;
		}
		else
		{
			loopWaitTime = 2.8f;
		}

		loopTimer.WaitTime = loopWaitTime;
	}

	private void StartSparkSequence()
	{
		sparkIndex = 0;
		sparkTimer.Start();
	}

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

	private void OnLoopTimerTimeout()
	{
		StartSparkSequence();
	}

	private void SpawnSpark(Vector2 pos)
	{
		Node sparkInstance = sparkAnimation.Instantiate();
		((Node2D)sparkInstance).Position = pos;
		((Node2D)sparkInstance).Scale = new Vector2(0.65f, 0.65f);
		CallDeferred("add_child", sparkInstance);
	}
}

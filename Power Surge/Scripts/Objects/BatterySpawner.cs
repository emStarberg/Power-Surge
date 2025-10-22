using Godot;
using System;

public partial class BatterySpawner : Area2D, IWorldObject
{
	private PackedScene battery = GD.Load<PackedScene>("Scenes/battery_pack.tscn");
	private float timer = 0;
	[Export] private float spawnTime = 20f; // Seconds between spawns
	private bool timerStarted = false;

	public override void _Ready()
	{
		// Spawn initial battery only if none present
		if (!HasBatteryPresent())
		{
			var node = battery.Instantiate();
			if (node is BatteryPack bp)
			{
				bp.GlobalPosition = GlobalPosition;
				bp.Visible = true;
			} 
			GetParent().CallDeferred("add_child", node);
			
		}
	}

	public override void _Process(double delta)
	{
		if (timerStarted)
			timer += (float)delta;

		if (timer >= spawnTime)
		{
			timer = 0;
			timerStarted = false;

			// Only spawn if a battery isn't already present
			if (!HasBatteryPresent())
			{
				var node = battery.Instantiate();
				if (node is BatteryPack bp)
				{
					bp.GlobalPosition = GlobalPosition;
					bp.Visible = true;
				}
				GetParent().CallDeferred("add_child", node);
				
			}
		}
	}

	/// <summary>
	/// Start timer when battery pack is picked up
	/// </summary>
	/// <param name="area"></param>
	public void OnAreaExited(Area2D area)
	{
		if (area is BatteryPack)
		{
			timer = 0;
			timerStarted = true;
		}
	}

	/// <summary>
	/// Stop the timer if a battery is placed back on the spawner
	/// (connect Area2D.area_entered to this in the editor or via code)
	/// </summary>
	public void OnAreaEntered(Area2D area)
	{
		if (area is BatteryPack)
		{
			timer = 0;
			timerStarted = false;
		}
	}

	// Defensive check for an existing BatteryPack at/near the spawner.
	private bool HasBatteryPresent()
	{
		// Fast check: overlapping areas/bodies (requires Area2D.Monitoring = true)
		try
		{
			foreach (var a in GetOverlappingAreas())
				if (a is BatteryPack) return true;
		}
		catch { /* ignore if monitoring not enabled */ }

		try
		{
			foreach (var b in GetOverlappingBodies())
				if (b is BatteryPack) return true;
		}
		catch { /* ignore */ }

		// Fallback: scan root for nearby BatteryPack instances
		foreach (Node n in GetTree().Root.GetChildren())
		{
			if (n is BatteryPack bp)
			{
				if (bp.GlobalPosition.DistanceTo(GlobalPosition) < 16f)
					return true;
			}
		}

		return false;
	}
}

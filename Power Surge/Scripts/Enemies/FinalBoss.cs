using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;
//------------------------------------------------------------------------------
// <summary>
//   Methods for the final boss in level 4-2
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class FinalBoss : Node2D
{
	private PackedScene circuitBug = GD.Load<PackedScene>("Scenes/circuit_bug.tscn");
	private PackedScene voltageSentinel = GD.Load<PackedScene>("Scenes/voltage_sentinel.tscn");
	private bool spawning = false, isAlive = true;
	private float spawnTimer = 0;
	private string spawnSide = "right";
	private int spawnIndex = 0;
	private string[] enemyArray = new[] { "sentinel", "bug", "bug", "sentinel", "bug" };
	public List<BossHammer> Hammers = new List<BossHammer>();
	public override void _Ready()
	{
		foreach(Node n in GetChildren())
		{
			// Add hammers to list
			if(n is BossHammer b)
			{
				Hammers.Add(b);
			}
		}
	}


	public override void _Process(double delta)
	{
		if (spawning)
		{
			spawnTimer += (float)delta;
			if (spawnTimer >= 0.3f)
			{
				spawnTimer = 0;
				if (spawnIndex < enemyArray.Length)
				{
					SpawnEnemy(enemyArray[spawnIndex]);
					spawnIndex++;
				}
				else
				{
					spawning = false;
				}
			}
		}
	}


	/// <summary>
	/// Spawn a set of enemies
	/// </summary>
	/// <param name="side">Side of screen to spawn in ("left" or "right")</param>
	public void SpawnEnemies()
	{
		spawning = true;
		if(spawnSide == "left")
		{
			spawnSide = "right";
		}
		else
		{
			spawnSide = "left";
		}
		spawnIndex = 0;
		spawnTimer = 0;
	}

	/// <summary>
	/// Spawn a specific enemy
	/// </summary>
	private void SpawnEnemy(string enemy)
	{
		Node enemyInstance;
		if (enemy == "sentinel")
		{
			enemyInstance = voltageSentinel.Instantiate();

		}
		else if (enemy == "bug")
		{
			enemyInstance = circuitBug.Instantiate();
		}
		else
		{
			return;
		}

		if (enemyInstance is Enemy instance)
		{
			if (spawnSide == "left")
			{
				instance.GlobalPosition = new Godot.Vector2(-150, -150);
			}
			else
			{
				instance.GlobalPosition = new Godot.Vector2(150, -150);
			}
			if (GetParent() is Level4_2 parentLevel)
				instance.TreeExited += parentLevel.OnEnemyTreeExited;

			GetParent().GetNode<Node2D>("Spawned Enemies").AddChild(instance);
		}
	}

	public void UseHammer()
	{
		Random rng = new();
		Hammers[rng.Next(0, Hammers.Count)].Attack();
	}
	
}

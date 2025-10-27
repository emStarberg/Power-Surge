using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Level4_2 : GameLevel
{
	private DialogueBox dialogueBox;
	private bool dialogueStarted = false, popupShown = false, timerRunning = true, resumedAfterBoss = false;
	private List<int> lineNumbers = new List<int> { 15 }; // Line numbers to pause dialogue at
	private TileMapLayer fakeGround; // Ground to be removed
	private AnimationPlayer animationPlayer;
	private float timer = 0;
	private string bossPhase;
	private FinalBoss finalBoss;
	private float spawnTimer = 0;
	private int enemyCount = 0;

	public override void _Ready()
	{
		
		StartLevel();
		GameData.Instance.GlowEnabled = false;
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-4-2.txt");
		camera.LimitLeft = -1175;
		camera.LimitRight = 3850;

		camera.Mode = "fixed";
		camera.Position = new Vector2(0, 0);
		camera.Zoom = new Vector2(2, 2);

		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");
		//backgroundMusic.Play();

		animationPlayer = GetNode<AnimationPlayer>("Animation Player");

		fakeGround = GetNode<TileMapLayer>("Fake Ground");

		finalBoss = GetNode<FinalBoss>("Final Boss");

		expectedTime = 200;

		dialogueBox.Pause();

		UpdateVolume();

		// Set up checkpoints
		foreach (Node node in GetNode<Node2D>("Checkpoints").GetChildren())
		{
			if (node is Area2D checkpoint)
			{
				// Connect a signal to each checkpoint
				checkpoint.BodyEntered += (Node2D body) => OnCheckPointEntered(body, checkpoint);
			}
		}

		// Set up camera changes
		foreach (Node node in GetNode<Node2D>("Checkpoints").GetNode<Node2D>("Camera Changes").GetChildren())
		{
			if (node is CameraChange change)
			{
				// Connect a signal to each checkpoint
				change.BodyExited += (Node2D body) => OnCameraChangeExited(body, change);
			}
		}



	}

	public override void _Process(double delta)
	{
		levelTimer += (float)delta;
		checkOptionsMenu();
		player.Paused = !dialogueBox.IsPaused();

		if (timerRunning)
		{
			timer += (float)delta;
		}

		if (timer > 1f && !dialogueStarted)
		{
			dialogueStarted = true;
			dialogueBox.Start();
			timerRunning = false;
			timer = 0;
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (popup.Visible)
			{
				popup.Visible = false;
			}
			if (lineNumbers.Contains(dialogueBox.GetLineNumber()) && !dialogueBox.IsTyping() && !dialogueBox.IsPaused())
			{
				dialogueBox.Pause();
				if(dialogueBox.GetLineNumber() == 15)
				{
					camera.Shake(1f, 5);
					animationPlayer.CurrentAnimation = "Ground Breaking";
					animationPlayer.Play();
				}
			}
		}


		if (bossPhase == "spawn")
		{
			if(enemyCount < 1)
			{
				// Continue spawning enemies
				spawnTimer += (float)delta;
				if (spawnTimer >= 8)
				{
					finalBoss.SpawnEnemies();
					spawnTimer = 0;
				}
			}
			else
			{
				// Remove remaining enemies
				camera.Shake(3, 3);
				foreach (Node n in GetNode<Node2D>("Spawned Enemies").GetChildren())
				{
					n.QueueFree();
				}
				animationPlayer.CurrentAnimation = "Platforms";
				animationPlayer.Play();
				bossPhase = "platforms";
			}

		}

	}
	
	/// <summary>
	/// Called when an enemy calls QueueFree() on itself (dies)
	/// </summary>
	public void OnEnemyTreeExited()
	{
		enemyCount++;
	}

	/// <summary>
	/// When a checkpoint is passed by the player
	/// </summary>
	/// <param name="body"></param>
	/// <param name="checkpoint"></param>
	public void OnCheckPointEntered(Node2D body, Area2D checkpoint)
	{
		if (body is Player player)
		{
			string name = checkpoint.Name;
			dialogueBox.Resume();
			if (name == "Hammer Phase")
			{

				camera.ChangeToFixed(new Vector2(2178, -950));
				camera.Mode = "fixed";
			}

			checkpoint.QueueFree();
		}
	}

	/// <summary>
	/// When a camera change point is passed by the player
	/// </summary>
	/// <param name="body"></param>
	/// <param name="checkpoint"></param>
	protected override void OnCameraChangeExited(Node2D body, CameraChange change)
	{
		if (body is Player player)
		{
			if (player.GetDirection() == change.DirectionEnteredFrom)
			{
				switch (change.Name)
				{
					case "1":
						if (change.DirectionEnteredFrom == "right")
						{
							camera.Mode = "centered";
							camera.SetCenterY(-350f);
							camera.ChangeToCentered();
						}
						else
						{
							camera.Mode = "centered";
							camera.SetCenterY(-470f);
							camera.ChangeToCentered();
						}
						break;
					case "2":
						camera.Mode = "fixed";
						camera.Position = new Vector2(0, -15);
						camera.Zoom = new Vector2(2, 2);

						player.DisableAllInputs();
						break;
				}
			}
		}
	}

	/// <summary>
	/// Called by Animation Player when ground animaiton has finished
	/// </summary>
	public void OnGroundAnimFinished()
	{
		fakeGround.QueueFree();
		bossPhase = "spawn";
		spawnTimer = 6;
	}

	/// <summary>
	/// Called by Animation Player when platform animation has finished
	/// </summary>
	public void OnPlatformAnimFinished()
	{
		GD.Print("method called");
		// Move to platform phase
		camera.Mode = "horizontal";
		player.Position = new Vector2(1550, -1050); // FOR TESTING
	}

	
}

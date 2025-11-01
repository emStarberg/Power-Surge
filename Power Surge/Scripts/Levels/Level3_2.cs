using Godot;
using System;
using System.Collections.Generic;

public partial class Level3_2 : GameLevel
{
	[Export] public Gate EndGate;
	private DialogueBox dialogueBox;
	private bool dialogueStarted = false, popupShown = false, resumedAfterPan = false, timerRunning = true, resumedAfterBoss = false;
	private List<int> lineNumbers = new List<int> {1, 3, 10}; // Line numbers to pause dialogue at
	private float timer = 0;
	private AnimationPlayer bossStartAnimation;
	public override void _Ready()
	{
		
		StartLevel();
		GameData.Instance.GlowEnabled = false;
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-3-2.txt");
		camera.LimitLeft = -1175;
		camera.LimitRight = 3850;
		camera.Mode = "centered";
		camera.SetCenterY(-470f);

		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");
		
		bossStartAnimation = GetNode<AnimationPlayer>("Boss Start Animation");

		bossStartAnimation.CurrentAnimation = "Start";
		bossStartAnimation.Stop();
		backgroundMusic.Play();

		expectedTime = 200;

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

		if(timer > 2f && !resumedAfterBoss)
		{
			resumedAfterBoss = true;
			dialogueBox.Resume();
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
			}
		}
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
			checkpoint.QueueFree();
			dialogueBox.Resume();
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
			if (player.GetDirection() == change.DirectionEnteredFrom && change.Name == "1")
			{									
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
			}else if (change.Name == "2")
			{
				camera.Mode = "fixed";
				camera.Position = new Vector2(0, -15);
				camera.Zoom = new Vector2(2, 2);

				player.DisableAllInputs();
			}
		}
	}

	/// <summary>
	/// Starts the boss fight once the player is in position
	/// </summary>
	public void OnBossTriggerEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Paused = true;
			camera.Shake(1, 7.5f);
			bossStartAnimation.CurrentAnimation = "Start";
			bossStartAnimation.Play();
			backgroundMusic.Stop();
			backgroundMusic.Stream = GD.Load<AudioStream>("res://Assets/Audio/Lab Boss.ogg");
			GetNode<Area2D>("Boss Start Trigger").QueueFree();
		}
	}
	/// <summary>
	/// Called by Lab Boss when boss dies
	/// </summary>
	public void OnBossDeathStart()
	{
		//Remove power surge mode if active
		if (player.PowerSurgeEnabled)
		{
			player.SetPower(100);
			player.StopPowerSurgeTimer();
		}
		// Stop music
		backgroundMusic.Stop();
		

	}

	/// <summary>
	/// Called by Lab Boss when boss death animation finishes
	/// </summary>
	public void OnBossDeathFinish()
	{
		// Open gate
		EndGate.UpdateState(true);
		// Resume default level music
		backgroundMusic.Stream = GD.Load<AudioStream>("res://Assets/Audio/Cave.mp3");
		backgroundMusic.Play();
		// Resume dialogue after timer
		timer = 0;
		timerRunning = true;

	}
	
	/// <summary>
	/// Start moving elevators and platforms when boss fight starts
	/// </summary>
	public void StartPlatforms()
	{
		foreach(Node n in GetNode<Node2D>("Objects").GetChildren())
		{
			if(n is SwitchOperatedObject s)
			{
				if(s is not Gate)
				{
					s.UpdateState(true);
				}
			}
		}
	}
	
}

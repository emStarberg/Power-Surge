using Godot;
using System;
using System.Collections.Generic;

public partial class Level2_2 : GameLevel
{
	private DialogueBox dialogueBox, wrongWayDialogue;
	private bool dialogueStarted = false, popupShown = false, resumedAfterPan = false, timerRunning = true;
	private List<int> lineNumbers = new List<int> { 1, 2, 12, 17 }; // Line numbers to pause dialogue at
	private float timer = 0;

	public override void _Ready()
	{
		StartLevel();
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-2-2.txt");
		dialogueBox.Pause();
		wrongWayDialogue = GetNode<DialogueBox>("UI/Wrong Way Dialogue");
		wrongWayDialogue.AddLinesFromFile("res://Assets/Dialogue Files/2-2-wrong-way.txt");
		wrongWayDialogue.Pause();
		camera.LimitLeft = -450;
		camera.LimitRight = 1750;
		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");

		expectedTime = 150;

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
				if (dialogueBox.GetLineNumber() == 2)
				{
					camera.Pan(new Vector2(1300, 0), 1.2f);
					timer = 0;
					timerRunning = true;
				}
				if (dialogueBox.GetLineNumber() == 12)
				{
					camera.CancelPan();
					GetNode<StaticBody2D>("Objects/Barrier").QueueFree();
					GetNode<Area2D>("Checkpoints/Wrong Way").QueueFree();
				}
			}
		}
		if(timer >= 1.8f && !resumedAfterPan)
		{
			dialogueBox.Resume();
			resumedAfterPan = true;
			timerRunning = false;
			timer = 0;
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
			if(name == "Wrong Way")
			{
				wrongWayDialogue.Start();
			}
			else
			{
				dialogueBox.Resume();
				checkpoint.QueueFree();
			}

		}
	}
}

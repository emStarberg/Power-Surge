using Godot;
using System;
using System.Collections.Generic;

public partial class Level3_1 : GameLevel
{
	private DialogueBox dialogueBox;
	private bool dialogueStarted = false, popupShown = false, resumedAfterPan = false, timerRunning = true;
	private List<int> lineNumbers = new List<int> { 1 }; // Line numbers to pause dialogue at
	private float timer = 0;

	public override void _Ready()
	{
		StartLevel();
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		//dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-2-2.txt");
		dialogueBox.Pause();
		camera.LimitLeft = -450;
		camera.LimitRight = 1750;
		camera.Mode = "centered";
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

		/*if (timer > 1f && !dialogueStarted)
		{
			dialogueStarted = true;
			dialogueBox.Start();
			timerRunning = false;
		}*/

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
		}
	}
}

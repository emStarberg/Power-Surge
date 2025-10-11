using Godot;
using System;
using System.Collections.Generic;

public partial class Level1_2 : GameLevel
{
	private DialogueBox dialogueBox;
	private bool dialogueStarted = false, popupShown = false;
	private List<int> lineNumbers = new List<int> { 2, 7, 9 }; // Line numbers to pause dialogue at
	private float timer;

	public override void _Ready()
	{
		StartLevel();
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-1-2.txt");

		camera.LimitLeft = -400;
		camera.LimitRight = 4500;

		expectedTime = 70;


		// Set up checkpoints
		foreach (Node node in GetNode<Node2D>("Checkpoints").GetChildren())
		{
			if (node is Area2D checkpoint)
			{
				// Connect a signal to each checkpoint
				checkpoint.BodyEntered += (Node2D body) => OnCheckPointEntered(body, checkpoint);
			}
		}

		
	}

	public override void _Process(double delta)
	{
		levelTimer += (float)delta;
		checkOptionsMenu();
		player.Paused = !dialogueBox.IsPaused();

		timer += (float)delta;
		if (timer > 1f && !dialogueStarted)
		{
			dialogueStarted = true;
			dialogueBox.Start();
		}
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (popup.Visible)
			{
				popup.Visible = false;
			}
			if (lineNumbers.Contains(dialogueBox.GetLineNumber()) && !dialogueBox.IsTyping())
			{

				if (dialogueBox.GetLineNumber() == 7 && !popupShown && dialogueStarted)
				{
					popup.Visible = true;
					popupShown = true;
				}
				dialogueBox.Pause();
			}
		}
	}
	
	public void OnCheckPointEntered(Node2D body, Area2D checkpoint)
	{
		if (body is Player player)
		{
			string name = checkpoint.Name;
			dialogueBox.Resume();
			checkpoint.QueueFree();
		}
	}

	

}

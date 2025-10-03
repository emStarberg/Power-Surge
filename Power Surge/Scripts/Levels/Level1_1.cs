using Godot;
using System;
using System.Collections.Generic;

public partial class Level1_1 : GameLevel
{
	private DialogueBox dialogueBox;
	private float timer = 0f;
	// Checkpoints are: #1 Reaching floating platforms, #2 Reaching fragment, #3 Collecting fragment, #4 Reaching enemy, #5 Destroying enemy, #6 Reaching battery
	private bool dialogueStarted = false, collectedFragment = false, killedEnemy = false , collectedBattery = false, popupShown = false;
	private List<int> lineNumbers = new List<int> { 3, 7, 9, 11, 14, 16, 17 }; // Line numbers to pause dialogue at
	private Enemy enemy;
	private BatteryPack battery;
	private Control popup;
	private Camera camera;
	public override void _Ready()
	{
		player = GetNode<Player>("Player");
		enemy = GetNode<Enemy>("Enemies/First Enemy");
		battery = GetNode<BatteryPack>("Objects/First Battery");
		popup = GetNode<Control>("UI/Pop-up");
		popup.Visible = false;
		// Set up dialogue
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/level-1-1.txt");

		camera = GetNode<Camera>("Camera");
		camera.LimitLeft = -400;
		camera.LimitRight = 4500;

		expectedTime = 80;

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
				// Show popup after collecting fragment
				if (dialogueBox.GetLineNumber() == 11 && !popupShown)
				{
					GD.Print("here");
					popup.Visible = true;
					popupShown = true;
				}
				dialogueBox.Pause();


			}

			
		}
		// #3 Collecting fragment
		if (player.GetFragmentCount() == 1 && !collectedFragment)
		{
			collectedFragment = true;
			dialogueBox.Resume();
		}

		// #5 Destroying enemy
		if (!IsInstanceValid(enemy) && !killedEnemy)
		{
			killedEnemy = true;
			dialogueBox.Resume();
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

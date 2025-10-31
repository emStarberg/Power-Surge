using Godot;
using System;
using System.Collections.Generic;

public partial class Tutorial : GameLevel
{
	private DialogueBox dialogueBox, deathDialogue;
	private bool hasJumped = false, hasDashed = false, hasMoved = false, hasAttacked = false, hasCycled = false;
	private bool dialogueWasPlaying = false, deathDialogueStarted = false, dialogueStarted = false;
	private Control tutorials;
	private float timer = 0;
	public override void _Ready()

	{
		dialogueBox = GetNode<DialogueBox>("UI/DialogueBox");
		deathDialogue = GetNode<DialogueBox>("UI/DeathDialogue");
		player = GetNode<Player>("Player");
		tutorials = GetNode<Control>("UI/Tutorials");
		backgroundMusic = GetNode<AudioStreamPlayer2D>("Camera/Background Music");

		// Disable all inputs to begin with, these are unlocked as the tutorial progresses
		DisableAllInputs();

		// Set up dialogue
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/tutorial.txt");
		deathDialogue.AddLinesFromFile("res://Assets/Dialogue Files/tutorialdeath.txt");

		GameData.Instance.GlowEnabled = GlowEnabled;
	}

	public override void _Process(double delta)
	{
		checkOptionsMenu();

		timer += (float)delta;
		if (timer >= 2f && !dialogueStarted)
		{
			dialogueStarted = true;
			dialogueBox.Start();
		}

		// Continue dialogue
		if (Input.IsActionJustPressed("ui_accept"))
		{
			// Movement
			if (dialogueBox.GetLineNumber() == 3 && !dialogueBox.IsTyping() && !hasMoved)
			{
				dialogueBox.Pause();
				ShowTutorial("Move");
				EnableAllInputs();
				player.EnableInputs("input_left", "input_right");

			}
			// Jump
			if (dialogueBox.GetLineNumber() == 4 && !dialogueBox.IsTyping() && !hasJumped)
			{
				dialogueBox.Pause();
				ShowTutorial("Jump");
				EnableAllInputs();
				player.EnableInputs("input_jump");
			}
			// Dash
			if (dialogueBox.GetLineNumber() == 13 && !dialogueBox.IsTyping() && !hasDashed)
			{
				dialogueBox.Pause();
				ShowTutorial("Dash");
				EnableAllInputs();
				player.EnableInputs("input_dash");
			}
			// Attack
			if (dialogueBox.GetLineNumber() == 22 && !dialogueBox.IsTyping() && !hasAttacked)
			{
				dialogueBox.Pause();
				ShowTutorial("Attack");
				EnableAllInputs();
				player.EnableInputs("input_cycle_backward", "input_cycle_forward", "input_attack");
			}
			// Death
			if (deathDialogue.GetLineNumber() == 2 && !deathDialogue.IsTyping())
			{
				deathDialogue.Pause();
				if (dialogueWasPlaying)
				{
					DisableAllInputs();
					dialogueBox.Resume();
				}
				else
				{
					EnableAllInputs();
				}
				dialogueWasPlaying = false;
			}
			// End of dialogue
			if (dialogueBox.GetLineNumber() == 28 && !dialogueBox.IsTyping())
			{
				dialogueBox.Pause();
				EnableAllInputs();
			}

			if (!dialogueBox.IsPaused())
			{
				DisableAllInputs();
				HideTutorials();
			}
		}
		// Move
		if (player.GetMileage() >= 100 && !hasMoved)
		{
			hasMoved = true;
			dialogueBox.Resume();
			DisableAllInputs();
			HideTutorials();
		}
		// Jump
		if (player.HasJumped && !hasJumped)
		{
			hasJumped = true;
			dialogueBox.Resume();
			DisableAllInputs();
			HideTutorials();
		}
		// Dash
		if (player.HasDashed && !hasDashed)
		{
			hasDashed = true;
			dialogueBox.Resume();
			DisableAllInputs();
			HideTutorials();
		}
		// Attack
		if (player.HasAttacked && !hasAttacked)
		{
			hasAttacked = true;
			dialogueBox.Resume();
			DisableAllInputs();
			HideTutorials();
		}

		// Play special dialogue if player runs out of power
		if (!player.IsAlive() && !deathDialogueStarted)
		{
			DisableAllInputs();
			deathDialogueStarted = true;
			dialogueWasPlaying = !dialogueBox.IsPaused();
			dialogueBox.Pause();
			deathDialogue.Start();
		}
	}

	/// <summary>
	/// Disables all input maps
	/// </summary>
	private void DisableAllInputs()
	{
		player.DisableInputs("input_left", "input_right", "input_jump", "input_dash", "input_cycle_forward", "input_cycle_backward", "input_cycle_forward", "input_shield", "input_attack");
	}

	/// <summary>
	/// Enables all input maps that should be already available
	/// </summary>
	private void EnableAllInputs()
	{
		List<String> enabledInputs = new List<string>();
		if (hasMoved)
		{
			enabledInputs.Add("input_left");
			enabledInputs.Add("input_right");
		}
		if (hasJumped)
		{
			enabledInputs.Add("input_jump");
		}
		if (hasDashed)
		{
			enabledInputs.Add("input_dash");
		}
		if (hasAttacked)
		{
			enabledInputs.Add("input_attack");
			enabledInputs.Add("input_cycle_backward");
			enabledInputs.Add("input_cycle_forward");
		}

		foreach (String input in enabledInputs)
		{
			player.EnableInputs(input);
		}
	}

	/// <summary>
	/// Make the specified tutorial text visible
	/// </summary>
	/// <param name="name">Name of tutorial</param>
	private void ShowTutorial(String name)
	{
		tutorials.GetNode<Control>(name).Visible = true;
	}

	/// <summary>
	/// Make all tutorial texts invisible
	/// </summary>
	private void HideTutorials()
	{
		foreach (Node n in tutorials.GetChildren())
		{
			if (n is Control control)
			{
				control.Visible = false;
			}
		}
	}
	
	protected override void UpdateVolume()
	{
		backgroundMusic.VolumeDb = GameSettings.Instance.GetFinalMusic();
	}

}

using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Handles the name selector, where the player chooses their name at the start of the game
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class NameSelector : ColorRect
{
	private List<Control> buttons = new List<Control>();
	private LineEdit lineEdit;
	private bool onButtons = false;
	Control currentButton;
	int selected = 0;
	Texture2D buttonOn, buttonOff;
	AudioStreamPlayer2D menuSound;

	public override void _Ready()
	{
		menuSound = GetNode<AudioStreamPlayer2D>("Menu Sound");

		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		// Add buttons to list
		foreach (Node node in GetNode<Control>("Buttons").GetChildren())
		{
			if (node is Control button)
			{
				buttons.Add(button);
			}
		}

		lineEdit = GetNode<LineEdit>("LineEdit");
		lineEdit.GrabFocus();
	}

	public override void _Process(double delta)
	{
		if (onButtons)
		{
			// Switch between buttons
			if (Input.IsActionJustPressed("ui_left"))
			{
				DeselectButton(selected);
				if (selected > 0)
				{
					selected--;
				}
				else
				{
					selected = buttons.Count - 1;
				}
				SelectButton(selected);
			}
			if (Input.IsActionJustPressed("ui_right"))
			{
				DeselectButton(selected);
				if (selected < buttons.Count - 1)
				{
					selected++;
				}
				else
				{
					selected = 0;
				}
				SelectButton(selected);
			}
		}

		if (Input.IsActionJustPressed("ui_up") && onButtons)
		{
			for (int i = 0; i < buttons.Count; i++)
				DeselectButton(i);

			onButtons = false;
			lineEdit.GrabFocus();
		}

		if (Input.IsActionJustPressed("ui_down") && !onButtons)
		{
			SelectButton(0);
			selected = 0;
			onButtons = true;
			lineEdit.ReleaseFocus();
		}

		if (Input.IsActionJustPressed("input_accept") && onButtons)
		{
			if (currentButton.Name == "OK")
			{
				if (lineEdit.Text != "")

				{
					// Set/reset settings
					GameSettings.Instance.PlayerName = lineEdit.Text;
					GameSettings.Instance.HasStarted = true;
					GameSettings.Instance.UnlockedLevels = new string[9];
					GameSettings.Instance.LevelFragments = [0,0,0,0,0,0,0,0,0];
					GameSettings.Instance.TutorialComplete = false;

				}
				GetTree().ChangeSceneToFile("res://Scenes/Cutscenes/opening_sequence.tscn");
			}
			else if (currentButton.Name == "CANCEL")
			{
				QueueFree();
			}
		}
	}

	/// <summary>
	/// Select a button by highlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void SelectButton(int index)
	{
		menuSound.Play();
		currentButton = buttons[index];
		currentButton.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
	}

	/// <summary>
	/// Deselect a button by unhiglighting it
	/// </summary>
	/// <param name="index">Index of control node to deselect</param>
	private void DeselectButton(int index)
	{
		Control button = buttons[index];
		button.GetNode<Sprite2D>("Sprite").Texture = buttonOff;
	}
}

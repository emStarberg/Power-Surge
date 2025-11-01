using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Handles the play options menu
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class PlayOptions : Node2D
{
	// For key mapping
	private bool waitingForKey = false;
	private string actionToRebind = " ";

	private List<Control> menuItems = new List<Control>();
	private int selected = 0;
	private Texture2D buttonOn, buttonOff;
	private Control effects, currentItem;
	private AudioStreamPlayer2D menuSound;

	public override void _Ready()
	{
		// Don't pause title screen
		//if (GetTree().CurrentScene.Name != "TitleScreen")
			//GetTree().Paused = true;

		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");
		effects = GetNode<Control>("Menus/Main/Buttons/Effects");
		menuSound = GetNode<AudioStreamPlayer2D>("Menu Sound");


		GameSettings.Instance.VolumeChanged += OnVolumeChanged;
		OnVolumeChanged(); // Set initial volume

		// Add buttons to list
		foreach (Node node in GetNode<Control>("Menus/Main/Buttons").GetChildren())
		{
			if (node is Control button)
			{
				// Don't add effects to list
				if (button.Name != "Effects")
				{
					menuItems.Add(button);
				}
			}
		}
		SelectButton(selected);

		
	}

	public override void _Process(double delta)
	{
				// Switch between buttons
		if (Input.IsActionJustPressed("input_up"))
		{
			DeselectButton(selected);
			if (selected > 0)
			{
				selected--;
			}
			else
			{
				selected = menuItems.Count - 1;
			}
			SelectButton(selected);
		}
		if (Input.IsActionJustPressed("input_down"))
		{
			DeselectButton(selected);
			if (selected < menuItems.Count - 1)
			{
				selected++;
			}
			else
			{
				selected = 0;
			}
			SelectButton(selected);
		}

		// Enter pressed
		if (Input.IsActionJustPressed("input_accept"))
		{
			String name = currentItem.Name;
			
		}
	}

	private void SelectButton(int index)
	{
		menuSound.Play();
		currentItem = menuItems[index];
		currentItem.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
		effects.Position = currentItem.Position;
	}

	private void DeselectButton(int index)
	{
		Control button = menuItems[index];
		button.GetNode<Sprite2D>("Sprite").Texture = buttonOff;
	}

	
	private void OnVolumeChanged()
	{
		if (menuSound == null || !menuSound.IsInsideTree())
			return;

			menuSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
	}


}

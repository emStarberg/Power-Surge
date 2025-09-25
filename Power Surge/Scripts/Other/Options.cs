using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Handles the options menu
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Options : Node2D
{
	private bool listeningForKey = false;
	private string actionToRebind = "input_jump";

	private List<Control> menuItems = new List<Control>();
	private int selected = 0;
	private Texture2D buttonOn, buttonOff;
	private UICamera camera;
	private Control effects, currentItem, currentMenu;
	private AudioStreamPlayer2D menuSound;


	public override void _Ready()
	{
		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		camera = GetParent().GetNode<UICamera>("UI Camera");
		effects = GetNode<Control>("Menus/Main/Buttons/Effects");
		menuSound = GetNode<AudioStreamPlayer2D>("Menu Sound");

		currentMenu = GetNode<Control>("Menus/Main"); // Set main as default

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
		// Main
		if (currentMenu.Name == "Main")
		{
			MainProcess();
		}
		else if (currentMenu.Name == "Audio")
		{
			AudioProcess();
		}
		else if (currentMenu.Name == "Controls")
		{
			ControlsProcess();
		}


	}




	public void OnRebindButtonPressed(string action)
	{
		listeningForKey = true;
		actionToRebind = action;
		// Update UI to show "Press any key..."
	}

	public override void _Input(InputEvent @event)
	{
		if (listeningForKey && @event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			// Remove old bindings
			InputMap.ActionEraseEvents(actionToRebind);

			// Add new binding
			InputMap.ActionAddEvent(actionToRebind, keyEvent);

			//listeningForKey = false;
			//actionToRebind = "";
			// Update UI to show new key
			GetNode<Label>("Control/Options/Input Mapping/Jump/Key").Text = keyEvent.AsText();
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

	private void SelectSlider(int index)
	{
		menuSound.Play();
		currentItem = menuItems[index];

		Label label = currentItem.GetNode<Label>("Label");
		label.AddThemeColorOverride("font_color", new Color("79e4ff")); // Blue
		label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f)); // Semi-transparent black
		label.AddThemeConstantOverride("shadow_offset_x", 4); // Horizontal offset
		label.AddThemeConstantOverride("shadow_offset_y", 4); // Vertical offset

		HSlider slider = currentItem.GetNode<HSlider>("HSlider");
		slider.GrabFocus();
	}

	private void DeselectSlider(int index)
	{
		currentItem = menuItems[index];
		currentItem.GetNode<Label>("Label").AddThemeColorOverride("font_color", new Color(255, 255, 255)); // White
	}

	private void SelectBox(int index)
	{
		menuSound.Play();
		currentItem = menuItems[index];
		Label key = currentItem.GetNode<Label>("Key");
		Label label = currentItem.GetNode<Label>("Label");
		label.AddThemeColorOverride("font_color", new Color("79e4ff")); // Blue
		key.AddThemeColorOverride("font_color", new Color("79e4ff")); // Blue
		label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f)); // Semi-transparent black
		label.AddThemeConstantOverride("shadow_offset_x", 4); // Horizontal offset
		label.AddThemeConstantOverride("shadow_offset_y", 4); // Vertical offset
	}

	private void DeselectBox(int index)
	{
		Control currentItem = menuItems[index];
		Label key = currentItem.GetNode<Label>("Key");
		Label label = currentItem.GetNode<Label>("Label");
		label.AddThemeColorOverride("font_color", new Color(255, 255, 255)); // White
		key.AddThemeColorOverride("font_color", new Color(255,255,255)); // White
	}

	private void MainProcess()
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
			if (name == "AUDIO")
			{
				DeselectButton(selected);
				SwitchMenu(GetNode<Control>("Menus/Audio"));
			}
			else if (name == "CONTROLS")
			{
				DeselectButton(selected);
				SwitchMenu(GetNode<Control>("Menus/Controls"));
			}
			else if (name == "BACK")
			{
				QueueFree();
			}
		}
	}

	private void AudioProcess()
	{
		// Switch between buttons
		if (Input.IsActionJustPressed("input_up"))
		{
			DeselectSlider(selected);
			if (selected > 0)
			{
				selected--;
			}
			else
			{
				selected = menuItems.Count - 1;
			}
			SelectSlider(selected);
		}
		if (Input.IsActionJustPressed("input_down"))
		{
			DeselectSlider(selected);
			if (selected < menuItems.Count - 1)
			{
				selected++;
			}
			else
			{
				selected = 0;
			}
			SelectSlider(selected);
		}

		if (Input.IsActionJustPressed("input_accept"))
		{
			if (currentItem.Name == "BACK")
			{
				DeselectSlider(selected);
				SwitchMenu(GetNode<Control>("Menus/Main"));
			}
		}
	}
	

	private void ControlsProcess()
	{
		// Switch between buttons
		if (Input.IsActionJustPressed("input_up"))
		{
			DeselectBox(selected);
			if (selected > 0)
			{
				selected--;
			}
			else
			{
				selected = menuItems.Count - 1;
			}
			SelectBox(selected);
		}
		if (Input.IsActionJustPressed("input_down"))
		{
			DeselectBox(selected);
			if (selected < menuItems.Count - 1)
			{
				selected++;
			}
			else
			{
				selected = 0;
			}
			SelectBox(selected);
		}

		if (Input.IsActionJustPressed("input_accept"))
		{
			if (currentItem.Name == "BACK")
			{
				DeselectBox(selected);
				SwitchMenu(GetNode<Control>("Menus/Main"));
			}
		}
	}

	public void VolumeChanged(double value)
	{
		GameSettings.Instance.Volume = (float)value;
	}

	public void MusicVolumeChanged(double value)
	{
		GameSettings.Instance.MusicVolume = (float)value;
	}

	public void SfxVolumeChanged(double value)
	{
		GameSettings.Instance.SfxVolume = (float)value;
	}


	private void SwitchMenu(Control to)
	{
		currentMenu.Visible = false;
		currentMenu = to;
		currentMenu.Visible = true;
		selected = 0;
		menuItems.Clear();
		string name = to.Name;
		if (name == "Audio")
		{
			// Add sliders to list
			foreach (Node node in GetNode<Control>("Menus/Audio/Sliders").GetChildren())
			{
				if (node is Control slider)
				{
					// Don't add effects to list
					if (slider.Name != "Effects")
					{
						menuItems.Add(slider);
						HSlider s = slider.GetNode<HSlider>("HSlider");
						if (slider.Name == "MASTER")
						{

							s.Value = GameSettings.Instance.Volume;
						}
						else if (slider.Name == "MUSIC")
						{
							s.Value = GameSettings.Instance.MusicVolume;
						}
						else if (slider.Name == "SFX")
						{
							s.Value = GameSettings.Instance.SfxVolume;
						}
					}
				}
			}
			SelectSlider(selected);


		}
		else if (name == "Main")
		{
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
		else if (name == "Controls")
		{
			// Add boxes to list
			foreach (Node node in GetNode<Control>("Menus/Controls/Boxes").GetChildren())
			{
				if (node is Control button)
				{
					menuItems.Add(button);
				}
			}
			SelectBox(selected);
		}
	}
	
	private void OnVolumeChanged()
	{
		menuSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
	}
}

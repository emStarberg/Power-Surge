using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Handles the title screen
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class TitleScreen : Node2D
{
	private List<Control> buttons = new List<Control>();
	private int selected = 0;
	private Texture2D buttonOn, buttonOff;
	private UICamera camera;
	private Control effects, currentButton;
	private AudioStreamPlayer2D zapSound, backgroundMusic;
	private bool optionsOpen = false, selectorOpen = false;
	public override void _Ready()
	{
		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		camera = GetNode<UICamera>("UI Camera");
		effects = GetNode<Control>("Control/Buttons/Effects");
		zapSound = GetNode<AudioStreamPlayer2D>("Zap Sound");
		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");

		GameSettings.Instance.VolumeChanged += OnVolumeChanged;
		OnVolumeChanged(); // Set initial volume

		// Add buttons to list
		foreach (Node node in GetNode<Control>("Control/Buttons").GetChildren())
		{
			if (node is Control button)
			{
				// Don't add effects to list
				if (button.Name != "Effects")
				{
					buttons.Add(button);
				}
			}
		}
		SelectButton(selected);
	}

	public override void _Process(double delta)
	{
		// Switch between buttons
		if (Input.IsActionJustPressed("input_up") && !optionsOpen && !selectorOpen)
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
		if (Input.IsActionJustPressed("input_down") && !optionsOpen && !selectorOpen)
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

		// Enter pressed
		if (Input.IsActionJustPressed("input_accept") && !optionsOpen && !selectorOpen)
		{
			String name = currentButton.Name;
			if (name == "START")
			{
				GetNode<Control>("Control/Buttons").Visible = false;
				selectorOpen = true;
				var selectorScene = GD.Load<PackedScene>("res://Scenes/Screens/name_selector.tscn");
				var selectorInstance = selectorScene.Instantiate();
				GetTree().CurrentScene.AddChild(selectorInstance);
				selectorInstance.TreeExited += OnNameSelectorClosed;
			}
			else if (name == "OPTIONS")
			{
				GetNode<Control>("Control/Buttons").Visible = false;
				optionsOpen = true;
				var optionsScene = GD.Load<PackedScene>("res://Scenes/Screens/options_screen.tscn");
				var optionsInstance = optionsScene.Instantiate();
				GetTree().CurrentScene.AddChild(optionsInstance);
				optionsInstance.TreeExited += OnOptionsClosed;
			}
			else if (name == "EXIT")
			{
				GetTree().Quit();
			}
		}
	}

	private void OnOptionsClosed()
	{
		optionsOpen = false;
		GetNode<Control>("Control/Buttons").Visible = true;
	}

	private void OnNameSelectorClosed()
	{
		selectorOpen = false;
		GetNode<Control>("Control/Buttons").Visible = true;
	}

	/// <summary>
	/// Select a button by higlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void SelectButton(int index)
	{
		zapSound.Play();
		camera.Shake(7, 0.1f);
		currentButton = buttons[index];
		currentButton.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
		effects.Position = currentButton.Position;
		foreach (Node node in effects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
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

	private void OnVolumeChanged()
	{
		backgroundMusic.VolumeDb = GameSettings.Instance.GetFinalMusic();
		zapSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
	}

}

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

	private bool menuOpen = false;

	public override void _Ready()
	{
		var tree = GetTree();
		// Don't pause title screen (guard against nulls)
		if (tree != null && tree.CurrentScene != null && tree.CurrentScene.Name != "TitleScreen")
			tree.Paused = true;

		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		effects = GetNodeOrNull<Control>("Menus/Main/Buttons/Effects");
		menuSound = GetNodeOrNull<AudioStreamPlayer2D>("Menu Sound");

		// Subscribe to settings volume changes if available
		if (GameSettings.Instance != null)
		{
			GameSettings.Instance.VolumeChanged += OnVolumeChanged;
			OnVolumeChanged(); // Set initial volume
		}

		// Add buttons to list
		var buttonsRoot = GetNodeOrNull<Control>("Menus/Main/Buttons");
		if (buttonsRoot != null)
		{
			foreach (Node node in buttonsRoot.GetChildren())
			{
				if (node is Control button && button.Name != "Effects")
				{
					menuItems.Add(button);
				}
			}
		}

		// Ensure selected is valid
		if (menuItems.Count == 0)
		{
			selected = -1;
			currentItem = null;
		}
		else
		{
			selected = Mathf.Clamp(selected, 0, menuItems.Count - 1);
			SelectButton(selected);
		}
	}

	public override void _Process(double delta)
	{
		// No menu items to operate on
		if (menuItems.Count == 0 || menuOpen)
			return;

		// Switch between buttons
		if (Input.IsActionJustPressed("input_up"))
		{
			DeselectButton(selected);
			if (selected > 0)
				selected--;
			else
				selected = menuItems.Count - 1;
			SelectButton(selected);
		}
		else if (Input.IsActionJustPressed("input_down"))
		{
			DeselectButton(selected);
			if (selected < menuItems.Count - 1)
				selected++;
			else
				selected = 0;
			SelectButton(selected);
		}

		// Enter pressed
		if (Input.IsActionJustPressed("input_accept"))
		{
			if (currentItem == null)
				return;

			string name = currentItem.Name;

			if (name == "CONTINUE" && GameSettings.Instance != null && GameSettings.Instance.HasStarted)
			{
				string[] unlockedLevels = GameSettings.Instance.UnlockedLevels ?? new string[0];
				// find last non-empty entry
				int last = -1;
				for (int i = 0; i < unlockedLevels.Length; i++)
				{
					if (!string.IsNullOrEmpty(unlockedLevels[i]))
						last = i;
				}

				if (last >= 0)
				{
					string level = unlockedLevels[last];
					if (level == "tutorial")
						GetTree()?.ChangeSceneToFile("res://Scenes/Levels/tutorial.tscn");
					else
						GetTree()?.ChangeSceneToFile("res://Scenes/Levels/level_" + level + ".tscn");
				}
				// else nothing unlocked yet - ignore or show message
			}
			else if (name == "NEW GAME")
			{
				Visible = false;
				menuOpen = true;

				var selectorScene = GD.Load<PackedScene>("res://Scenes/Screens/name_selector.tscn");
				var selectorInstance = selectorScene.Instantiate();

				var t = GetTree();
				if (t != null)
				{
					Node parent = t.CurrentScene ?? t.Root;
					parent.AddChild(selectorInstance);
					selectorInstance.TreeExited += OnNameSelectorClosed;
				}
				else
				{
					GD.PrintErr("PlayOptions: unable to get SceneTree to open name selector.");
				}
			}
			else if (name == "LEVELS" && GameSettings.Instance != null && GameSettings.Instance.HasStarted)
			{
				GetTree()?.ChangeSceneToFile("res://Scenes/Screens/level_selector.tscn");
			}
			else if (name == "BACK")
			{
				var t = GetTree();
				if (t != null)
					t.Paused = false;
				QueueFree();
			}
		}
	}

	private void SelectButton(int index)
	{
		if (menuItems == null || menuItems.Count == 0)
			return;
		if (index < 0 || index >= menuItems.Count)
			return;

		if (menuSound != null && menuSound.IsInsideTree())
			menuSound.Play();

		currentItem = menuItems[index];

		var sprite = currentItem.GetNodeOrNull<Sprite2D>("Sprite");
		if (sprite != null && buttonOn != null)
			sprite.Texture = buttonOn;

		if (effects != null)
			effects.Position = currentItem.Position;
	}

	private void DeselectButton(int index)
	{
		if (menuItems == null || menuItems.Count == 0)
			return;
		if (index < 0 || index >= menuItems.Count)
			return;

		Control button = menuItems[index];
		var sprite = button.GetNodeOrNull<Sprite2D>("Sprite");
		if (sprite != null && buttonOff != null)
			sprite.Texture = buttonOff;
	}

	private void OnVolumeChanged()
	{
		if (menuSound == null || !menuSound.IsInsideTree())
			return;

		// Guard GameSettings.Instance in case it's null
		if (GameSettings.Instance != null)
			menuSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
	}

	public void OnNameSelectorClosed()
	{
		Visible = true;
		menuOpen = false;
	}

	public override void _ExitTree()
	{
		// Unsubscribe to avoid callbacks after this node is freed
		if (GameSettings.Instance != null)
			GameSettings.Instance.VolumeChanged -= OnVolumeChanged;

		base._ExitTree();
	}
}

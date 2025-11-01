using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
//------------------------------------------------------------------------------
// <summary>
//   Handles the level select screen
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class LevelSelector : Control
{
	private List<Control> levels = new List<Control>();
	private Texture2D selectedFrame, deselectedFrame, buttonOn, buttonOff;
	private int selected = 0; // Index of selected level
	private AudioStreamPlayer2D zapSound, backgroundMusic;
	private UICamera camera;
	private Control effects, currentLevel, backButton;
	private bool buttonSelected = true;
	private Control fragmentCountDisplay;

	public override void _Ready()
	{
		// Get frame textures
		selectedFrame = GD.Load<Texture2D>("res://Assets/Thumbnails/Thumbnail Frame - Selected.png");
		deselectedFrame = GD.Load<Texture2D>("res://Assets/Thumbnails/Thumbnail Frame.png");

		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		effects = GetNode<Control>("Levels/Effects");
		camera = GetNode<UICamera>("UI Camera");
		backButton = GetNode<Control>("BACK");
		fragmentCountDisplay = GetNode<Control>("Fragment Count Display");

		int displayCount = GameSettings.Instance.GetTotalFragments();
		if(displayCount < 18)
		{
			fragmentCountDisplay.GetNode<Label>("Fragment Count").Text = displayCount + "/18";

		}
		else
		{
			fragmentCountDisplay.Visible = false;
			if (GameSettings.Instance.CoreComplete)
			{
				GameSettings.Instance.UnlockedLevels[8] = "4-2";
			}
		}

		// Get audio
		zapSound = GetNode<AudioStreamPlayer2D>("Zap Sound");
		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");

		GameSettings.Instance.VolumeChanged += OnVolumeChanged;
		OnVolumeChanged(); // Set initial volume
		// Add levels to list
		foreach (Node n in GetNode<Control>("Levels").GetChildren())
		{
			if (n is Control c)
			{
				if (c.Name != "Effects")
				{
					c.GetNode<Control>("Fragments").Position = new Vector2(0, -33);

					if (GameSettings.Instance.UnlockedLevels.Contains<string>(c.Name))
					{
			
						levels.Add(c);
						c.GetNode<Control>("Locked").Visible = false;

						int fragments = GetFragmentCount(c.Name);

						int count = 0;
						foreach(Node node in c.GetNode<Control>("Fragments").GetChildren())
						{
							if(node is TextureRect t && count < fragments)
							{
								count++;
								t.Texture = GD.Load<Texture2D>("res://Assets/Objects/Fragment - Filled Slot.png");
							}
						}
					}
	
				}
				if (c.Name == "tutorial")
					c.GetNode<Control>("Fragments").Visible = false;

			}
		}

		SelectButton();
	}


	public override void _Process(double delta)
	{
		// Switch between levels
		if (Input.IsActionJustPressed("input_left") && !buttonSelected)
		{
			DeselectLevel(selected);
			if (selected > 0)
			{
				selected--;
			}
			else
			{
				selected = levels.Count - 1;
			}
			SelectLevel(selected);
		}
		if (Input.IsActionJustPressed("input_right") && !buttonSelected)
		{
			DeselectLevel(selected);
			if (selected < levels.Count - 1)
			{
				selected++;
			}
			else
			{
				selected = 0;
			}
			SelectLevel(selected);
		}
		if (!buttonSelected)
		{
			if (Input.IsActionJustPressed("input_down") && currentLevel.Name == "tutorial")
				SelectButton();
		}
		if (Input.IsActionJustPressed("input_up") && buttonSelected && GameSettings.Instance.UnlockedLevels.Contains<string>("tutorial"))
		{
			DeselectButton();
			SelectLevel(0);
		}


		// Enter pressed
		if (Input.IsActionJustPressed("input_accept"))
		{
			if (!buttonSelected)
			{
				if (currentLevel.Name == "tutorial")
				{
					GetTree().ChangeSceneToFile("res://Scenes/Levels/tutorial.tscn");
				}
				else
				{
					GetTree().ChangeSceneToFile("res://Scenes/Levels/level_" + currentLevel.Name + ".tscn");
				}
			}
			else
			{
				GameSettings.Instance.SaveGame();
				GetTree().ChangeSceneToFile("res://Scenes/Screens/title_screen.tscn");
			}
		}

	}

	/// <summary>
	/// Select a level by highlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void SelectLevel(int index)
	{
		zapSound.Play();
		camera.Shake(7, 0.1f);
		currentLevel = levels[index];
		currentLevel.GetNode<TextureRect>("Frame").Texture = selectedFrame;
		currentLevel.GetNode<Label>("Label").Visible = true;
		currentLevel.GetNode<Control>("Fragments").Position = new Vector2(0, 0);
		effects.Position = currentLevel.Position;
		
		foreach (Node node in effects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
	}
	/// <summary>
	/// Deselect a level by unhighlighting it
	/// </summary>
	/// <param name="index">Index of control node to deselect</param>
	private void DeselectLevel(int index)
	{
		Control level = levels[index];
		level.GetNode<TextureRect>("Frame").Texture = deselectedFrame;
		level.GetNode<Label>("Label").Visible = false;
		level.GetNode<Control>("Fragments").Position = new Vector2(0, -33);
	}

	/// <summary>
	/// Select a button by highlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void SelectButton()
	{
		buttonSelected = true;
		zapSound.Play();
		camera.Shake(7, 0.1f);
		backButton.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
		if (GameSettings.Instance.UnlockedLevels.Contains<string>("tutorial"))
		DeselectLevel(0);	
	}

	/// <summary>
	/// Select a button by highlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void DeselectButton()
	{
		buttonSelected = false;
		zapSound.Play();
		camera.Shake(7, 0.1f);
		backButton.GetNode<Sprite2D>("Sprite").Texture = buttonOff;
		effects.Position = levels[0].Position;
		foreach (Node node in effects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
		currentLevel = levels[0];
	}


	private int GetFragmentCount(string name)
	{
		var gameSettings = GameSettings.Instance;
		int index = 0;

		for (int i = 0; i < gameSettings.UnlockedLevels.Length; i++)
		{
			if (gameSettings.UnlockedLevels[i] == name)
			{
				index = i;
			}
		}
		
		return gameSettings.LevelFragments[index];
	}
	
	private void OnVolumeChanged()
	{
		backgroundMusic.VolumeDb = GameSettings.Instance.GetFinalMusic();
		zapSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
		
	}
}

using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Screen that shows after a level is complete. Calculates a rank based on player stats throughout the level.
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class EndScreen : Node2D
{
	private List<Control> buttons = new List<Control>();
	private List<Label> labels = new List<Label>();
	private int selected = 0;
	private Texture2D buttonOn, buttonOff;
	private UICamera camera;
	private Control effects, currentButton, textEffects, alert;
	private AudioStreamPlayer2D zapSound, backgroundMusic, lightningSound;
	private List<string> levels = new List<string> { "1-1", "1-2", "2-1", "2-2"};
	private float timer = 0, enemiesKilled = 0;
	private bool shownFragments = false, shownPower = false, shownTime = false, shownRank = false, shownEnemies = false, glowing = true;
	private Label rank;
	

	public override void _Ready()
	{
		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		camera = GetNode<UICamera>("UI Camera");
		effects = GetNode<Control>("Control/Buttons/Effects");
		textEffects = GetNode<Control>("Control/Stats/Effects");
		zapSound = GetNode<AudioStreamPlayer2D>("Zap Sound");
		lightningSound = GetNode<AudioStreamPlayer2D>("Lightning Sound");
		rank = GetNode<Label>("Control/Rank");
		alert = GetNode<Control>("Control/Alert"); // For MVP

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

		// Add labels to list
		foreach (Node node in GetNode<Control>("Control/Stats").GetChildren())
		{
			if (node is Label stat)
			{
				labels.Add(stat.GetNode<Label>("Label"));
			}
		}

		labels[0].Text = GameData.Instance.LevelFragments.ToString() + "/3";
		labels[1].Text = GameData.Instance.LevelPower.ToString() + "%";
		float totalSeconds = GameData.Instance.LevelTime;
		int minutes = (int)(totalSeconds / 60);
		int seconds = (int)(totalSeconds % 60);
		// Format as "MM:SS"
		labels[2].Text = $"{minutes:D2}:{seconds:D2}";
		enemiesKilled = GameData.Instance.LevelEnemyCount - GameData.Instance.LevelEnemyCountFinal;
		labels[3].Text = enemiesKilled + "/" + GameData.Instance.LevelEnemyCount;

		// Show alert if last available level
		alert.Visible = levels.IndexOf(GameData.Instance.CurrentLevel) == levels.Count - 1;

		rank.Text = CalculateRank();
	}

	public override void _Process(double delta)
	{
		timer += (float)delta;
		if (timer >= 2f && !shownFragments)
		{
			shownFragments = true;
			ShowStat(labels[0]);
		}
		if (timer >= 3f && !shownPower)
		{
			shownPower = true;
			ShowStat(labels[1]);
		}
		if (timer >= 4f && !shownTime)
		{
			shownTime = true;
			ShowStat(labels[2]);
		}
		if(timer >= 5f && !shownEnemies)
		{
			shownEnemies = true;
			ShowStat(labels[3]);
		}
		if (timer >= 7f && !shownRank)
		{
			shownRank = true;
			ShowRank();
		}
		
		// Switch between buttons
		if (Input.IsActionJustPressed("input_left"))
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
		if (Input.IsActionJustPressed("input_right"))
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
		// Button pressed
		if (Input.IsActionJustPressed("input_accept"))
		{
			switch (currentButton.Name)
			{
				case "RETRY":
					// Restart current level
					GetTree().ChangeSceneToFile("res://Scenes/Levels/level_" + GameData.Instance.CurrentLevel + ".tscn");
					break;
				case "EXIT":
					// Exit to title screen
					GetTree().ChangeSceneToFile("res://Scenes/Screens/title_screen.tscn");
					break;
				case "NEXT":
					// Go to next level
					int index = levels.IndexOf(GameData.Instance.CurrentLevel);
					string next = levels[index + 1];
					if (next == "1-1" || next == "1-2")
					{
						glowing = false;
					}
					if (!alert.Visible)
					{
						LevelLoader.Instance.ChangeLevel("res://Scenes/Levels/level_" + next + ".tscn", glowing);
					}					
					break;
				default:
					break;
			}
		}
	}

	/// <summary>
	/// Select a button by highlighting it
	/// </summary>
	/// <param name="index">Index of control node to select</param>
	private void SelectButton(int index)
	{
		currentButton = buttons[index];
		currentButton.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
		if (shownFragments)
		{
			zapSound.Play();
			camera.Shake(7, 0.1f);
			effects.Position = currentButton.Position;
			foreach (Node node in effects.GetChildren())
			{
				if (node is AnimatedSprite2D spark)
				{
					spark.Play();
				}
			}
		}
	}

	/// <summary>
	/// Deselect a button by unhighlighting it
	/// </summary>
	/// <param name="index">Index of control node to deselect</param>
	private void DeselectButton(int index)
	{
		Control button = buttons[index];
		button.GetNode<Sprite2D>("Sprite").Texture = buttonOff;
	}

	/// <summary>
	/// Show a stat label with dramatic effect
	/// </summary>
	/// <param name="label">Label to be shown</param>
	private void ShowStat(Label label)
	{
		lightningSound.Play();
		camera.Shake(8, 0.3f);
		label.Visible = true;
		textEffects.Position = ((Label)label.GetParent()).Position + new Vector2(200, 0);
		foreach (Node node in textEffects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
	}

	/// <summary>
	/// Show the player's rank with dramatic effect
	/// </summary>
	private void ShowRank()
	{
		Control rankEffects = rank.GetNode<Control>("Effects");
		lightningSound.Play();
		camera.Shake(10, 0.6f);
		rank.Visible = true;
		foreach (Node node in rankEffects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
	}

	/// <summary>
	/// Calculate final rank using fragments collected, power remaining, time, and enemies killed
	/// </summary>
	/// <returns>Final letter grade</returns>
	private string CalculateRank()
	{
		GD.Print("RESULTS:");
		GD.Print("-------------------------------");
		string finalRank = "F";
		int points = 0;

		points += GameData.Instance.LevelFragments; // Max 3
		GD.Print("+ " + points + " for " + GameData.Instance.LevelFragments + " fragments collected");
		float power = GameData.Instance.LevelPower; // Max 2
		if (power >= 70)
		{
			points++;
			GD.Print("+ 1 for power >= 80");
		}
		if (power >= 110)
		{
			points++;
			GD.Print("+ 1 for power >= 110");
		}
		
		float levelTime = GameData.Instance.LevelTime;
		float expectedTime = GameData.Instance.LevelExpectedTime;

		if (levelTime <= expectedTime) // Max 2
		{
			if ((expectedTime - levelTime) > 15)
			{
				points++;
				GD.Print("+ 1 for 15s faster than expected time");
			}
			if ((expectedTime - levelTime) > 5)
			{
				points++;
				GD.Print("+ 1 for 5s faster than expected time");
			}
		}
		float enemyPercentage = (enemiesKilled / GameData.Instance.LevelEnemyCount) * 100;
		// Max 2
		if (enemyPercentage >= 50) { 
			points++;
			GD.Print("+1 for 70% of enemies defeated");
		}
		if (enemyPercentage >= 100) {
			points++;
			GD.Print("+1 for 100% of enemies defeated");
		}

		List<string> ranks = new List<string> { "F", "E", "D", "C", "C+", "B", "B+", "A", "A+", "S" }; // The maximum 9 points is needed for 'S' rank

		finalRank = ranks[points];

		GD.Print("-------------------------------");
		GD.Print("Time: " + levelTime);
		GD.Print("Fragments: " + GameData.Instance.LevelFragments);
		GD.Print("Power: " + power);
		GD.Print("Enemies Defeated: " + enemiesKilled);

		GD.Print("-------------------------------");
		GD.Print("Final Rank: " + finalRank);
		GD.Print("Points: " + points);
		GD.Print("-------------------------------");
		GD.Print("EnemyCount: " + GameData.Instance.LevelEnemyCount);
		GD.Print("EnemyCountFinal: " + GameData.Instance.LevelEnemyCountFinal);

		return finalRank;


		
	}
}

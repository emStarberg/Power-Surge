using Godot;
using System;
using System.Collections.Generic;

public partial class TitleScreen : Node2D
{
	private List<Control> buttons = new List<Control>();
	private int selected = 0;
	private Texture2D buttonOn, buttonOff;
	private UICamera camera;
	private Control effects;
	public override void _Ready()
	{
		buttonOn = GD.Load<Texture2D>("res://Assets/UI/Button - Highlighted.png");
		buttonOff = GD.Load<Texture2D>("res://Assets/UI/Button.png");

		camera = GetNode<UICamera>("UI Camera");
		effects = GetNode<Control>("Control/Buttons/Effects");

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
		if (Input.IsActionJustPressed("input_down"))
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
		if (Input.IsActionJustPressed("input_up"))
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


	private void SelectButton(int index)
	{
		camera.Shake(7, 0.1f);
		Control button = buttons[index];
		button.GetNode<Sprite2D>("Sprite").Texture = buttonOn;
		effects.Position = button.Position;
		foreach (Node node in effects.GetChildren())
		{
			if (node is AnimatedSprite2D spark)
			{
				spark.Play();
			}
		}
	}

	private void DeselectButton(int index)
	{
		Control button = buttons[index];
		button.GetNode<Sprite2D>("Sprite").Texture = buttonOff;
	}

}

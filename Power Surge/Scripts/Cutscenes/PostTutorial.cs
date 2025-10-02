using Godot;
using System;

//------------------------------------------------------------------------------
// <summary>
//   Plays the post tutorial cutscene for the game
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class PostTutorial : Node2D
{
	private DialogueBox dialogueBox;
	private float timer = 0;
	private bool dialogueStarted = false, fadingIn = true, fadingOut = false;
	private TextureRect fadeImage;
	private float fadeTime = 6.0f, fadeTimer = 0; // seconds

	public override void _Ready()
	{
		fadeImage = GetNode<TextureRect>("Control/BackgroundImage");
		fadeImage.Modulate = new Color(1, 1, 1, 0); // Start fully transparent
		dialogueBox = GetNode<DialogueBox>("DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/post-tutorial.txt");

		FadeImageIn();
	}

	public override void _Process(double delta)
	{
		timer += (float)delta;

		// Fade image in
		if (fadingIn)
		{
			fadeTimer += (float)delta;
			float alpha = Mathf.Clamp(fadeTimer / fadeTime, 0, 1);
			fadeImage.Modulate = new Color(1, 1, 1, alpha);

			if (alpha >= 1)
			{
				fadingIn = false; // Fade complete
				dialogueBox.Start();
				dialogueStarted = true;
			}
		}

		// Fade image out
		if (fadingOut && fadeImage.Visible)
		{
			fadeTimer += (float)delta;
			float alpha = Mathf.Clamp(1 - (fadeTimer / fadeTime), 0, 1);
			fadeImage.Modulate = new Color(1, 1, 1, alpha);

			if (alpha <= 0)
			{
				fadeImage.Visible = false; // Hide when fully faded out
				GetTree().ChangeSceneToFile("res://Scenes/Levels/level_1-1.tscn");
			}
		}

		if (dialogueBox.GetLineNumber() == 19 && !dialogueBox.IsTyping() && Input.IsActionJustPressed("ui_accept") && dialogueStarted)
		{
			FadeImageOut();
		}
	}


	/// <summary>
	/// Fade in an image
	/// </summary>
	/// <param name="duration">Time to fade in for</param>
	public void FadeImageIn(float duration = 6f)
	{
		fadeImage.Visible = true;
		GD.Print("Fading image in");
		fadeTime = duration;
		fadeTimer = 0f;
		fadingIn = true;
		fadeImage.Modulate = new Color(1, 1, 1, 0);
	}

	/// <summary>
	/// Fade out an image
	/// </summary>
	/// <param name="duration">Time to fade out for</param>
	public void FadeImageOut(float duration = 6f)
	{
		fadeImage.Visible = true;
		GD.Print("Fading image out");
		fadeTime = duration;
		fadeTimer = 0f;
		fadingOut = true;
		fadeImage.Modulate = new Color(1, 1, 1, 1); // Start fully opaque
	}

}

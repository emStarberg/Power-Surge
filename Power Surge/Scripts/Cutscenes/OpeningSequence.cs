using Godot;
using System;
using System.Runtime.CompilerServices;

//------------------------------------------------------------------------------
// <summary>
//   Plays the opening cutscene for the game
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class OpeningSequence : Node2D
{
	private VideoStreamPlayer videoPlayer;
	private AudioStreamPlayer2D buzzerSound, explosionSoundFar, explosionSoundNear, runningSound, doorOpenSound, doorCloseSound;
	private string part1 = "res://Assets/Videos/Opening Pt. 1.ogv", part2 = "res://Assets/Videos/Opening Pt. 2.ogv", loop = "res://Assets/Videos/Opening Alarm Loop.ogv";
	private string currentVideo = "part 1";
	private float videoTimer = 0f;
	private DialogueBox dialogueBox;
	private Camera2D camera;
	private int explosionNearIndex = 0, explosionFarIndex = 0;
	private float[] explosionNearTimes = [7.2f], explosionFarTimes = [4.0f, 12.0f];
	private bool runSoundPlayed = false, openSoundPlayed = false, closeSoundPlayed = false;
	private bool dialogueStarted = false; // To be used when starting/resuming dialogue
	private TextureRect fadeImage;
	private float fadeTime = 6.0f, fadeTimer = 0; // seconds
	private bool fadingIn = false, fadingOut = false;

	private float zoomTime = 2f, zoomTimer = 0f;
	private bool zoomingIn = false;

	private Vector2 startZoom = new Vector2(0.9f, 0.9f), endZoom = new Vector2(2, 2);

	private bool buzzerFadingOut = false;
	private float buzzerFadeSpeed = 10f; // Decibels per second


	private Vector2 panStart, panEnd;
	private bool panningUp = false;
	private float panTime = 2f, panTimer = 0f;

	private AnimatedSprite2D orb1, orb2;


	public override void _Ready()
	{
		GD.Print(GameSettings.Instance.PlayerName);
		dialogueBox = GetNode<DialogueBox>("DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/openingsequence.txt");

		buzzerSound = GetNode<AudioStreamPlayer2D>("Control/Buzzer");
		explosionSoundNear = GetNode<AudioStreamPlayer2D>("Control/Explosion Near");
		explosionSoundFar = GetNode<AudioStreamPlayer2D>("Control/Explosion Far");
		runningSound = GetNode<AudioStreamPlayer2D>("Control/Running");
		doorOpenSound = GetNode<AudioStreamPlayer2D>("Control/Door Open");
		doorCloseSound = GetNode<AudioStreamPlayer2D>("Control/Door Close");

		videoPlayer = GetNode<VideoStreamPlayer>("Control/Video");

		fadeImage = GetNode<TextureRect>("Control/BackgroundImage");
		fadeImage.Modulate = new Color(1, 1, 1, 0); // Start fully transparent

		camera = GetNode<Camera2D>("Camera");

		orb1 = GetNode<AnimatedSprite2D>("Control/Orb1");
		orb2 = GetNode<AnimatedSprite2D>("Control/Orb2");
		orb1.Visible = false;
		orb2.Visible = false;

		UpdateVolume(); // Set initial volume

		// Begin playing part 1
		videoPlayer.Play();
		currentVideo = "part 1";
		buzzerSound.VolumeDb -= 10;
		buzzerSound.Play();
	}

	public override void _Process(double delta)
	{
		videoTimer += (float)delta;

		if (currentVideo == "part 1")
		{
			// Play explosion sound at each specified time
			if (explosionNearIndex < explosionNearTimes.Length && videoTimer >= explosionNearTimes[explosionNearIndex])
			{
				explosionSoundNear.Play();
				explosionNearIndex++;
			}

			// Play explosion sound at each specified time
			if (explosionFarIndex < explosionFarTimes.Length && videoTimer >= explosionFarTimes[explosionFarIndex])
			{
				explosionSoundFar.Play();
				explosionFarIndex++;
			}
		}
		else if (currentVideo == "alarm loop")
		{
			if (!dialogueStarted && videoTimer >= 4.0f)
			{
				dialogueBox.Start();
				dialogueStarted = true;
			}
			if (!buzzerFadingOut)
			{
				// Play far explosion every 8 seconds
				if (Math.Round(videoTimer % 8) == 0)
				{
					explosionSoundFar.Play();
				}
				// Play far explosion every 20 seconds
				if (Math.Round(videoTimer % 20) == 0)
				{
					explosionSoundNear.Play();
				}
			}
			// When dialogue is paused
			if (dialogueBox.IsPaused())
			{
				if (currentVideo == "alarm loop")
				{
					// Play run sound once at 1 second after dialogue box pauses (video timer has just reset)
					if (videoTimer >= 1 && !runSoundPlayed)
					{
						runningSound.Play();
						runSoundPlayed = true;
					}
					if (videoTimer >= 5 && !openSoundPlayed)
					{
						runningSound.Stop();
						doorOpenSound.Play();
						openSoundPlayed = true;
					}
				}

			}

			// Fade out alarm buzzer sound
			if (buzzerFadingOut)
			{
				buzzerSound.VolumeDb -= buzzerFadeSpeed * (float)delta;
				if (buzzerSound.VolumeDb <= -80)
				{
					// Switch to pt.2 when faded out
					videoPlayer.Loop = false;
					buzzerSound.Stop();
					videoPlayer.Stream = ResourceLoader.Load<VideoStream>(part2);
					videoPlayer.Play();
					currentVideo = "part 2";
					buzzerFadingOut = false;
				}
			}

		}
		else if (currentVideo == "computer" && !dialogueStarted)
		{ // Start dialogue again
			if (videoTimer >= 2)
			{
				dialogueBox.Resume();
				dialogueStarted = true;
			}
		}
		// Fade image in
		if (fadingIn)
		{
			fadeTimer += (float)delta;
			float alpha = Mathf.Clamp(fadeTimer / fadeTime, 0, 1);
			fadeImage.Modulate = new Color(1, 1, 1, alpha);

			if (alpha >= 1)
			{
				fadingIn = false; // Fade complete
				dialogueBox.Resume();
				orb1.Visible = true;
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
				GetTree().ChangeSceneToFile("res://Scenes/Levels/tutorial.tscn");
			}
		}
		// Zoom camera in when dialogue has finished
		if (zoomingIn)
		{
			orb2.Visible = false;
			zoomTimer += (float)delta;
			float t = Mathf.Clamp(zoomTimer / zoomTime, 0, 1);

			camera.Zoom = startZoom.Lerp(endZoom, t);
			camera.Position = panStart.Lerp(panEnd, t);

			if (t >= 1)
			{
				zoomingIn = false; // Zoom and pan complete
				FadeImageOut();
			}
		}

		// Move to next scene if needed
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (!dialogueBox.IsPaused() && dialogueStarted)
			{
				if (dialogueBox.GetLineNumber() == 2 && !dialogueBox.IsTyping() && videoPlayer.IsPlaying() && currentVideo == "alarm loop")
				{
					// Pause dialogue and reset video timer
					dialogueBox.Pause();
					videoTimer = 0;
				}
				else if (dialogueBox.GetLineNumber() == 45 && !dialogueBox.IsTyping())
				{
					orb1.Visible = false;
					orb2.Visible = true;
					// Show close up of computer
					dialogueBox.Pause();
					dialogueStarted = false;
					currentVideo = "computer";
					videoTimer = 0;
					fadeImage.Texture = GD.Load<Texture2D>("res://Assets/UI/Lab Computer.png");
				}
				else if (dialogueBox.GetLineNumber() == 48 && !dialogueBox.IsTyping())
				{
					// Zoom in on computer before fading out
					ZoomIn();
				}
			}
		}
	}

	/// <summary>
	/// Called when the current video is finished playing
	/// </summary>
	public void OnVideoFinished()
	{
		videoTimer = 0;
		if (currentVideo == "part 1")
		{
			buzzerSound.VolumeLinear ++;
			videoPlayer.Stream = ResourceLoader.Load<VideoStream>(loop);
			currentVideo = "alarm loop";
			videoPlayer.Loop = true;
			videoPlayer.Play();
		}
		else if (currentVideo == "part 2")
		{
			currentVideo = "lab";
			videoPlayer.Stop();
			fadeImage.Visible = true;
			FadeImageIn();
		}
	}

	/// <summary>
	/// Loop buzzer sound when audio finishes
	/// </summary>
	/// 
	public void OnBuzzerFinished()
	{
		// Loop
		buzzerSound.Play();
	}

	/// <summary>
	/// Fade in an image
	/// </summary>
	/// <param name="duration">Time to fade in for</param>
	public void FadeImageIn(float duration = 4.5f)
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
	public void FadeImageOut(float duration = 3.5f)
	{
		fadeImage.Visible = true;
		GD.Print("Fading image out");
		fadeTime = duration;
		fadeTimer = 0f;
		fadingOut = true;
		fadeImage.Modulate = new Color(1, 1, 1, 1); // Start fully opaque
	}
	/// <summary>
	/// Zoom and pan camera
	/// </summary>
	/// <param name="duration">Time to zoom in/pan for</param>
	public void ZoomIn(float duration = 2f, Vector2? targetZoom = null)
	{
		zoomTime = duration;
		zoomTimer = 0f;
		zoomingIn = true;
		startZoom = camera.Zoom;
		endZoom = targetZoom ?? new Vector2(2.2f, 2.2f);
		panStart = camera.Position;
		panEnd = panStart + new Vector2(0, -120);
	}
	/// <summary>
	/// Called when door opening sound finishes
	/// </summary>
	public void OnOpenSoundFinished()
	{
		doorCloseSound.Play();
	}
	/// <summary>
	/// Called when door closing sound finishes
	/// </summary>
	public void OnClosedSoundFinished()
	{
		buzzerFadingOut = true;
	}

	private void UpdateVolume()
	{
		explosionSoundFar.VolumeDb = GameSettings.Instance.GetFinalSfx() - 30;
		explosionSoundNear.VolumeDb = GameSettings.Instance.GetFinalSfx();
		doorOpenSound.VolumeDb = GameSettings.Instance.GetFinalSfx() + 5;
		doorCloseSound.VolumeDb = GameSettings.Instance.GetFinalSfx() + 5;
		runningSound.VolumeDb = GameSettings.Instance.GetFinalSfx() + 5;

		buzzerSound.VolumeDb = GameSettings.Instance.GetFinalMusic()-10;
	}
}

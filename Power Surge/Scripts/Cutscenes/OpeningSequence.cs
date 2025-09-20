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
	private string part1 = "res://Assets/Videos/Opening Pt. 1.ogv";
	private string part2 = "res://Assets/Videos/Opening Pt. 2.ogv";
	private string loop = "res://Assets/Videos/Opening Alarm Loop.ogv";
	private string currentVideo = "part 1";
	private float videoTimer = 0f;
	private DialogueBox dialogueBox;
	private int explosionNearIndex = 0;
	private float[] explosionNearTimes = [7.2f];
	private int explosionFarIndex = 0;
	private float[] explosionFarTimes = [4.0f, 12.0f];
	private bool runSoundPlayed = false;
	private bool openSoundPlayed = false;
	private bool closeSoundPlayed = false;

	private bool dialogueStarted = false; // To be used when starting/resuming dialogue

	private TextureRect fadeImage;
	private float fadeTime = 6.0f; // seconds
	private float fadeTimer = 0f;
	private bool fadingIn = false;

	private bool buzzerFadingOut = false;
	private float buzzerFadeSpeed = 10f; // Decibels per second


	public override void _Ready()
	{
		dialogueBox = GetNode<DialogueBox>("DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/openingsequence.txt");

		buzzerSound = GetNode<AudioStreamPlayer2D>("Control/Buzzer");
		explosionSoundNear = GetNode<AudioStreamPlayer2D>("Control/Explosion Near");
		explosionSoundFar = GetNode<AudioStreamPlayer2D>("Control/Explosion Far");
		runningSound = GetNode<AudioStreamPlayer2D>("Control/Running");
		doorOpenSound = GetNode<AudioStreamPlayer2D>("Control/Door Open");
		doorCloseSound = GetNode<AudioStreamPlayer2D>("Control/Door Close");
		;
		videoPlayer = GetNode<VideoStreamPlayer>("Control/Video");

		fadeImage = GetNode<TextureRect>("Control/BackgroundImage");
		fadeImage.Modulate = new Color(1, 1, 1, 0); // Start fully transparent

		// Begin playing part 1
		videoPlayer.Play();
		currentVideo = "part 1";
		buzzerSound.VolumeDb = -30;
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
			
		}else if (currentVideo == "computer" && !dialogueStarted) 
		{ // Start dialogue again
				if (videoTimer >= 2)
				{
					dialogueBox.Resume();
					dialogueStarted = true;
				}
		}
		
		if (fadingIn)
		{
			GD.Print(fadeImage.Modulate);
			fadeTimer += (float)delta;
			float alpha = Mathf.Clamp(fadeTimer / fadeTime, 0, 1);
			fadeImage.Modulate = new Color(1, 1, 1, alpha);

			if (alpha >= 1)
			{
				fadingIn = false; // Fade complete
				dialogueBox.Resume();
			}

		}

		// Move to next scene if needed
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (dialogueBox.GetLineNumber() == 2 && !dialogueBox.IsTyping() && videoPlayer.IsPlaying() && currentVideo == "alarm loop")
			{
				// Pause dialogue and reset video timer
				dialogueBox.Pause();
				videoTimer = 0;
			}
			else if (dialogueBox.GetLineNumber() == 54 && !dialogueBox.IsTyping())
			{
				dialogueBox.Pause();
				dialogueStarted = false;
				currentVideo = "computer";
				videoTimer = 0;
				fadeImage.Texture = GD.Load<Texture2D>("res://Assets/UI/Lab Computer.png");
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
		buzzerSound.VolumeDb = -20;
	}

	public void FadeImageIn(float duration = 6.0f)
	{
		fadeImage.Visible = true;
		GD.Print("Fading image in");
		fadeTime = duration;
		fadeTimer = 0f;
		fadingIn = true;
		fadeImage.Modulate = new Color(1, 1, 1, 0);
	}

	public void OnOpenSoundFinished()
	{
		doorCloseSound.Play();
	}

	public void OnClosedSoundFinished()
	{
		buzzerFadingOut = true;
	}
}

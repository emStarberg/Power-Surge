using Godot;
using System;

public partial class OpeningSequence : Node2D
{
	private VideoStreamPlayer videoPlayer;
	private AudioStreamPlayer2D buzzerSound, explosionSoundFar, explosionSoundNear;
	private string part1 = "res://Assets/Videos/Opening Pt. 1.ogv";
	private string loop = "res://Assets/Videos/Opening Alarm Loop.ogv";
	private string currentVideo = "part 1";
	private float videoTimer = 0f;
	private DialogueBox dialogueBox;
	
	private int explosionNearIndex = 0;
	private float[] explosionNearTimes = [7.2f];

	private int explosionFarIndex = 0;
	private float[] explosionFarTimes = [4.0f, 12.0f];

	private bool dialogueStarted = false;


	public override void _Ready()
	{
		dialogueBox = GetNode<DialogueBox>("DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/openingsequence.txt");

		buzzerSound = GetNode<AudioStreamPlayer2D>("Control/Buzzer");
		explosionSoundNear = GetNode<AudioStreamPlayer2D>("Control/Explosion Near");
		explosionSoundFar = GetNode<AudioStreamPlayer2D>("Control/Explosion Far");

		videoPlayer = GetNode<VideoStreamPlayer>("Control/Video");

		// Begin playing part 1
		videoPlayer.Play();
		currentVideo = "part 1";
		buzzerSound.VolumeDb = -10;
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
	

	if (Input.IsActionJustPressed("ui_accept"))
			{
			if (dialogueBox.GetLineNumber() == 2 && !dialogueBox.GetIsTyping() && videoPlayer.IsPlaying() && currentVideo == "alarm loop")
			{
				GD.Print("Stopped loop");
				videoPlayer.Stop();
				buzzerSound.Stop();
				
				}
			}
}

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
	}

	public void OnBuzzerFinished()
	{
		// Loop
		buzzerSound.Play();
		buzzerSound.VolumeDb = 0;
	}
}

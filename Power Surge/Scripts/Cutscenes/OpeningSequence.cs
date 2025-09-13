using Godot;
using System;

public partial class OpeningSequence : Node2D
{
	private VideoStreamPlayer videoPlayer;
	private string part1 = "res://Assets/Videos/Opening Pt. 1.ogv";
	private string loop = "res://Assets/Videos/Opening Alarm Loop.ogv";
	private string currentVideo = "part 1";

	private DialogueBox dialogueBox;

	public override void _Ready()
	{
		dialogueBox = GetNode<DialogueBox>("DialogueBox");
		dialogueBox.AddLinesFromFile("res://Assets/Dialogue Files/openingsequence.txt");

		
		videoPlayer = GetNode<VideoStreamPlayer>("Control/Video");

		// Begin playing part 1
		videoPlayer.Play();
		currentVideo = "part 1";
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (dialogueBox.GetLineNumber() == 2 && !dialogueBox.GetIsTyping() && videoPlayer.IsPlaying() && currentVideo == "alarm loop")
			{
				GD.Print("Stopped loop");
				videoPlayer.Stop();
			}

			if (Input.IsActionJustPressed("ui_accept"))
			{
				GD.Print(dialogueBox.GetLineNumber());
			}
		}
	}


	public void OnVideoFinished()
	{
		if (currentVideo == "part 1")
		{
			videoPlayer.Stream = ResourceLoader.Load<VideoStream>(loop);
			currentVideo = "alarm loop";
			videoPlayer.Loop = true;
			videoPlayer.Play();
			dialogueBox.Start();
		}
	}
}

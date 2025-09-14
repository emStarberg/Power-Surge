using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueBox : Control
{
	[Export] private Label speakerLabel;
	[Export] private Label dialogueLabel;
	[Export] private TextureRect portrait;

	private readonly Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
	private bool isTyping = false;
	private float typingSpeed = 0.03f; // seconds between letters
	private bool isFinished = false;
	private readonly List<DialogueLine> dialogueList = new List<DialogueLine>();
	private bool paused;
	private AudioStreamPlayer2D startupSound;
	
	private AudioStreamPlayer2D continueSound;




	public override void _Ready()
	{
		Visible = false;
		startupSound = GetNode<AudioStreamPlayer2D>("Startup Sound");
		continueSound = GetNode<AudioStreamPlayer2D>("Continue Sound");
	}
	public override void _Process(double delta)
	{
		if (!paused)
		{
			// If accept is pressed
			if (Input.IsActionJustPressed("ui_accept"))
			{
				if (isTyping)
				{
					// Skip typing effect and show full line
					dialogueLabel.Text = currentLine.Text;
					isTyping = false;
				}
				else
				{
					// Show next line
					ShowNextLine();
				}
			}
		}

	}

	// Add this field to store the current line:
	private DialogueLine currentLine;

	// Update ShowNextLine to set currentLine:
	public void ShowNextLine()
	{
		if (dialogueQueue.Count == 0)
		{
			Hide();
			return;
		}
		continueSound.Play();
		currentLine = dialogueQueue.Dequeue();
		speakerLabel.Text = currentLine.SpeakerName;
		portrait.Texture = currentLine.Portrait;
		dialogueLabel.Text = "";

		Show();
		_ = TypeText(currentLine.Text);
	}

	public void StartDialogue(List<DialogueLine> lines)
	{
		dialogueQueue.Clear();
		foreach (var line in lines)
			dialogueQueue.Enqueue(line);

		ShowNextLine();
	}


	private async System.Threading.Tasks.Task TypeText(string text)
	{
		isTyping = true;
		dialogueLabel.Text = "";

		foreach (char letter in text)
		{
			dialogueLabel.Text += letter;
			await ToSignal(GetTree().CreateTimer(typingSpeed), "timeout");
			if (!isTyping) break;
		}

		isTyping = false;
	}

	public void AddLine(string speakerName, string text)
	{
		dialogueList.Add(new DialogueLine(speakerName, text));
	}

	public void Start()
	{
		StartDialogue(dialogueList);
		startupSound.Play();
		paused = false;
		Visible = true;
	}

	public void Pause()
	{
		paused = true;
	}

	public void Resume()
	{
		paused = false;
	}

	/// <summary>
	/// Reads dialogue lines from a file and adds them to the dialogue list.
	/// Expected file format: speakerName|text (one line per dialogue)
	/// </summary>
	public void AddLinesFromFile(string filename)
	{
		dialogueList.Clear();

		var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Could not open file: {filename}");
			return;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line)) continue;

			var parts = line.Split('|');
			if (parts.Length < 2)
			{
				GD.PrintErr($"Malformed line: {line}");
				continue;
			}

			string speaker = parts[0].Trim();
			string text = parts[1].Trim();


			dialogueList.Add(new DialogueLine(speaker, text));
		}

		file.Close();
	}

	public int GetLineNumber()
	{
		return dialogueList.Count - dialogueQueue.Count;
	}

	public bool GetIsTyping()
	{
		return isTyping;
	}

}






public class DialogueLine
{
	public string SpeakerName { get; set; }
	public string Text { get; set; }
	public Texture2D Portrait;
	public DialogueLine(string speaker, string text)
	{
		SpeakerName = speaker;
		Text = text;
		string path = "res://Assets/UI/Icons/"+ speaker + ".png";
		

		Portrait = GD.Load<Texture2D>(path);
	}
}
	

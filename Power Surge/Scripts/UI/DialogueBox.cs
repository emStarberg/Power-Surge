using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Handles displaying a dialogue box for communicating with the player
// 	 Contains DialogueLine class
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class DialogueBox : Control
{
	[Export] private Label speakerLabel, dialogueLabel;
	[Export] private TextureRect portrait;

	private readonly Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
	private bool typing = false;
	private float typingSpeed = 0.03f; // seconds between letters
	private bool isFinished = false;
	private readonly List<DialogueLine> dialogueList = new List<DialogueLine>();
	private bool paused;
	private AudioStreamPlayer2D startupSound, continueSound;
	private DialogueLine currentLine;
	private String playerName = "George";

	public override void _Ready()
	{
		Visible = false;
		startupSound = GetNode<AudioStreamPlayer2D>("Startup Sound");
		continueSound = GetNode<AudioStreamPlayer2D>("Continue Sound");

		GameSettings.Instance.VolumeChanged += OnVolumeChanged;
		OnVolumeChanged(); // Set initial volume
	}

	public override void _Process(double delta)
	{
		if (!paused)
		{
			// If accept is pressed
			if (Input.IsActionJustPressed("ui_accept"))
			{
				if (typing)
				{
					// Skip typing effect and show full line
					dialogueLabel.Text = FormatTextWithLineBreaks(currentLine.Text);
					typing = false;
				}
				else
				{
					Texture2D prev = portrait.Texture; // Get current texture for later comparison
													   // Show next line
					ShowNextLine();
					Texture2D current = portrait.Texture; // Get new texture
														  // Play startup sound if speaker has changed
					if (current != prev)
					{
						startupSound.Play();
					}
					else if (dialogueQueue.Count != 0)
					{
						continueSound.Play();
					}
				}
			}
		}

	}

	/// <summary>
	/// Display the next line of dialogue
	/// </summary>
	public void ShowNextLine()
	{
		// Stop if there are no more lines left
		if (dialogueQueue.Count == 0)
		{
			Hide();
			return;
		}
		// Remove current line from queue
		currentLine = dialogueQueue.Dequeue();
		// Set up new line
		speakerLabel.Text = currentLine.SpeakerName;
		portrait.Texture = currentLine.Portrait;
		dialogueLabel.Text = "";

		Show();
		// Start typing effect
		_ = TypeText(currentLine.Text);
	}


	/// <summary>
	/// Produces a typing effect on the dialogue
	/// </summary>
	/// <param name="text">Text to be typed</param>
	/// <returns></returns>
	private async System.Threading.Tasks.Task TypeText(string text)
	{
		typing = true;
		dialogueLabel.Text = "";

		int charCount = 0;
		foreach (char letter in text)
		{
			dialogueLabel.Text += letter;
			charCount++;

			// Insert a new line if current line exceeds 48 characters
			if (charCount >= 48 && letter == ' ')
			{
				dialogueLabel.Text += "\n";
				charCount = 0;
			}

			await ToSignal(GetTree().CreateTimer(typingSpeed), "timeout");
			if (!typing) break;
		}

		typing = false;
	}

	/// <summary>
	/// Format the given text to fit within dialogue box
	/// </summary>
	/// <param name="text">Text to format</param>
	/// <param name="maxCharsPerLine">Maximun number of characters per line</param>
	/// <returns></returns>
	private string FormatTextWithLineBreaks(string text, int maxCharsPerLine = 50)
	{
		int charCount = 0;
		string result = "";
		foreach (char letter in text)
		{
			result += letter;
			charCount++;
			if (charCount >= maxCharsPerLine && letter == ' ')
			{
				result += "\n";
				charCount = 0;
			}
		}
		return result;
	}

	/// <summary>
	/// Add a new dialogue line to the queue
	/// </summary>
	/// <param name="speakerName">Name of person speaking</param>
	/// <param name="text">Dialogue text</param>
	public void AddLine(string speakerName, string text)
	{
		dialogueList.Add(new DialogueLine(speakerName, text));
	}

	/// <summary>
	/// Start dialogue from the beginning
	/// </summary>
	public void Start()
	{
		dialogueQueue.Clear();
		foreach (var line in dialogueList)
			dialogueQueue.Enqueue(line);

		ShowNextLine();
		startupSound.Play();
		paused = false;
		Visible = true;
	}

	/// <summary>
	/// Pause the dialogue
	/// </summary>
	public void Pause()
	{
		paused = true;
		Visible = false;
	}

	/// <summary>
	/// Resume the dialogue after a pause
	/// </summary>
	public void Resume()
	{
		paused = false;
		ShowNextLine();
		startupSound.Play();
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
		ReplaceNameInDialogue(playerName);
	}

	/// <summary>
	/// Get the number of the line currently being displayed
	/// </summary>
	/// <returns></returns>
	public int GetLineNumber()
	{
		return dialogueList.Count - dialogueQueue.Count;
	}

	/// <summary>
	/// Whether dialogue is being typed
	/// </summary>
	/// <returns>isTyping</returns>
	public bool IsTyping()
	{
		return typing;
	}

	/// <summary>
	/// Whether the dialogue is paused
	/// </summary>
	/// <returns></returns>
	public bool IsPaused()
	{
		return paused;
	}

	/// <summary>
	/// Replaces any appearences of "[name]" in the dialogue with the player's chosen name
	/// </summary>
	/// <param name="playerName">Player's chosen name</param>
	public void ReplaceNameInDialogue(string playerName)
	{
		foreach (var line in dialogueList)
		{
			if (line.Text.Contains("[name]"))
				line.Text = line.Text.Replace("[name]", playerName);
		}
	}

	private void OnVolumeChanged()
	{
		startupSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
		continueSound.VolumeDb = GameSettings.Instance.GetFinalSfx();
	}

}




/// <summary>
/// Class for creating lines of dialogue
/// </summary>
public class DialogueLine
{
	public string SpeakerName { get; set; }
	public string Text { get; set; }
	public Texture2D Portrait;

	/// <summary>
	/// Constructor for DialogueLine
	/// </summary>
	/// <param name="speaker">Person speaking</param>
	/// <param name="text">Dialogue text</param>
	public DialogueLine(string speaker, string text)
	{
		SpeakerName = speaker;
		Text = text;

		// Set portrait based on speaker name
		string path = "res://Assets/UI/Icons/" + speaker + ".png";
		Portrait = GD.Load<Texture2D>(path);
	}
}
	

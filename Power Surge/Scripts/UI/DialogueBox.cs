using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueBox : Control
{
	[Export] private Label speakerLabel;
	[Export] private Label dialogueLabel;
	[Export] private TextureRect portrait;

	private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
	private bool isTyping = false;
	private float typingSpeed = 0.03f; // seconds between letters
	public override void _Ready(){
		List<DialogueLine> introDialogue = new List<DialogueLine>
		{
			new DialogueLine("LILAH", "Try using that battery pack there to recharge!", GD.Load<Texture2D>("res:///Assets/Default/icon.svg")),
			new DialogueLine("ARCHIE", "Don't listen to her, Felix!", GD.Load<Texture2D>("res:///Assets/Default/icon.svg")),
			new DialogueLine("ARCHIE", "The guardians are protecting something important!",  GD.Load<Texture2D>("res:///Assets/Default/icon.svg"))
		};
		StartDialogue(introDialogue);
	}
	public override void _Process(double delta)
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
}

public class DialogueLine
{
	public string SpeakerName { get; set; }
	public string Text { get; set; }
	public Texture2D Portrait { get; set; }

	public DialogueLine(string speaker, string text, Texture2D portrait)
	{
		SpeakerName = speaker;
		Text = text;
		Portrait = portrait;
	}
}

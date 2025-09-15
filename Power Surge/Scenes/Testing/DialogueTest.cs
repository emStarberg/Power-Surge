using Godot;
using System;

public partial class DialogueTest : Node2D
{
	private DialogueBox db;
	public override void _Ready()
	{
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);


		db = GetNode<DialogueBox>("DialogueBox");
		db.AddLinesFromFile("res://Assets/Dialogue Files/test.txt");
		db.Start();
	}
}

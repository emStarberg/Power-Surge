using Godot;
using System;

public partial class ScreenResolution : Node
{
	public override void _Ready(){
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
	}
	
	public override void _Process(double delta){
		if(Input.IsActionJustPressed("input_exit")){
			GetTree().Quit();
		}
	}
}

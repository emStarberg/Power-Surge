using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Sets the screen resolution for the scene it's in
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class ScreenResolution : Node
{
	public override void _Ready()
	{
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
	}
}

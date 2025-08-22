using Godot;
using System;

public partial class JumpAnimation : AnimatedSprite2D
{
	public override void _Ready(){
		Play();
	}
	
	/// <Summary>
	/// Destroys itself once the animation has finished
	/// </Summary>
	public void _on_animation_finished()
	{
		QueueFree();
	}
}

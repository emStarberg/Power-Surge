using Godot;
using System;

public partial class JumpAnimation : AnimatedSprite2D
{
	public override void _Ready(){
		Play();
	}

	public void _on_animation_finished(){
		// Destroy self
		QueueFree();
	}
}

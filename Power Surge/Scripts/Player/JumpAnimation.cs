using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Jump animation that spawns when the player jumps
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------

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

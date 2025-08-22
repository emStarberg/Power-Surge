using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Dash animation that spawns when player dashes
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class DashAnimation : AnimatedSprite2D
{
	public override void _Ready()
	{
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
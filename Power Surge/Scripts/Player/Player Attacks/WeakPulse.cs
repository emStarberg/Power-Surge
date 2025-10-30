using Godot;
using System;

/// <summary>
///   Represents the player's weak pulse attack.
///   Handles activation, animation, and cleanup for the attack.
/// </summary>
/// <author>Emily Braithwaite</author>
public partial class WeakPulse : Node2D, IPlayerAttack
{
	public float damage = 20;
	private string direction;
	private AnimatedSprite2D animatedSprite;
	private Vector2 offset;
	private bool hurtCalled = false;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// Initializes the animated sprite reference.
	/// </summary>
	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("Anim_WeakPulse");
	}

	/// <summary>
	/// Activates the attack, sets direction, and starts the animation.
	/// Positions the pulse and flips the sprite if needed.
	/// </summary>
	public void Activate(string dir)
	{
		animatedSprite.Visible = true;
		animatedSprite.Play();

		direction = dir;

		if (direction == "right")
		{
			offset = new Vector2(22, 0);
			animatedSprite.FlipH = true;
		}
		else
		{
			offset = new Vector2(-20, 0);
		}

		GlobalPosition = ((Node2D)GetParent()).GlobalPosition + offset;
	}

	/// <summary>
	/// Cleans up the attack node when the animation finishes.
	/// </summary>
	public void OnAnimationFinished()
	{
		QueueFree();
	}


	/// <summary>
	/// Handles collision with other bodies, applies damage to enemies.
	/// Calls Hurt on enemy if applicable.
	/// </summary>
	public void OnAreaEntered(Node2D area)
	{
		if (area.Name != "Player" && !hurtCalled)
		{
			if (area.GetParent() is Enemy enemy)
			{
				GD.Print("hurt called");
				enemy.Hurt(damage);
				hurtCalled = true;
			}
		}
	}	
}

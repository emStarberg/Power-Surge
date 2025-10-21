using Godot;
using System;

//------------------------------------------------------------------------------
// <summary>
//   Represents the player's strong blast attack.
//   Handles movement, animation, and collision logic for the attack.
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------

public partial class ProjectileLabBoss : Area2D
{
	private string direction;
	private AnimatedSprite2D animatedSprite;
	private bool doMove = false;
	private float speed = 350f;
	private int damage = 20;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// Initializes the animated sprite reference.
	/// </summary>
	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("Animations");
	}

	/// <summary>
	/// Handles movement logic each frame if the attack is active.
	/// Moves the blast left or right depending on direction.
	/// </summary>
	public override void _Process(double delta)
	{
		if (doMove)
		{
			if (direction == "left")
				Position += new Vector2(-speed * (float)delta, 0);
			else if (direction == "right")
				Position += new Vector2(speed * (float)delta, 0);
		}
	}

	/// <summary>
	/// Activates the attack, sets direction, and starts the animation.
	/// Positions the blast and flips the sprite if needed.
	/// </summary>
	public void Activate(string dir)
	{
		if (!GameData.Instance.GlowEnabled)
		{
			GetNode<PointLight2D>("Light").Visible = false;
		}

		doMove = true;
		direction = dir;
		if (dir == "right")
		{
			animatedSprite.FlipH = true;
			animatedSprite.Position += new Vector2(-20, 0);
		}
		animatedSprite.Animation = "start";
		animatedSprite.Play();
	}

	/// <summary>
	/// Handles animation transitions and cleanup when the animation finishes.
	/// Switches to moving animation or frees the node if contact animation finishes.
	/// </summary>
	public void OnAnimFinished()
	{
		if (animatedSprite.Animation == "start")
		{
			animatedSprite.Animation = "moving"; // "moving" plays on a loop
			animatedSprite.Play();
		}
		else // Is using contact animation
		{
			QueueFree(); // Destroy self
		}
	}

	/// <summary>
	/// Stops movement and plays the contact animation.
	/// Adjusts sprite position based on direction.
	/// </summary>
	public void Stop()
	{
		doMove = false;
		animatedSprite.Animation = "contact";
		if (direction == "left")
		{
			animatedSprite.Position += new Vector2(-10, 0);
		}
		else
		{
			animatedSprite.Position += new Vector2(10, 0);
		}
		animatedSprite.Play();
	}

	/// <summary>
	/// Handles collision with other bodies, applies damage to Player.
	/// </summary>
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			Stop();
			player.Hurt(damage, 0.2f, 0.2f);
		}
	}

	/// <summary>
	/// Handles collision with other areas.
	/// </summary>
	public void OnAreaEntered(Area2D area)
	{
		Stop();
	}	
}

using Godot;
using System;

public partial class Hammer : Node2D, IWorldObject
{
	private AnimatedSprite2D animation;
	private CollisionShape2D collider;
	private Node2D statics;

	public override void _Ready()
	{
		animation = GetNode<AnimatedSprite2D>("Animation");
		collider = GetNode<CollisionShape2D>("Area/Collider");
		collider.Disabled = true;
		statics = GetNode<Node2D>("Statics");
		DisableStatics();
	}


	public void OnAnimationFrameChanged()
	{
		if (animation.Frame == 2)
		{
			collider.Disabled = false;
			EnableStatics();
		}
		else if (animation.Frame == 6)
		{
			collider.Disabled = true;
			DisableStatics();
		}
	}

	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Hurt(35, 0.2f, 0.2f);
		}
	}

	private void DisableStatics()
	{
		foreach (Node2D node in statics.GetChildren())
		{
			if (node is StaticBody2D body)
			{
				body.GetNode<CollisionShape2D>("Collider").Disabled = true;
			}
		}
	}

	private void EnableStatics()
	{
		foreach (Node2D node in statics.GetChildren())
		{
			if (node is StaticBody2D body)
			{
				body.GetNode<CollisionShape2D>("Collider").Disabled = false;
			}
		}
	}
	
	public void UpdateVolume()
	{

	}
}

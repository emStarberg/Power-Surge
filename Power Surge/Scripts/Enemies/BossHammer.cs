using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//     Hammer used by final boss
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class BossHammer : Enemy
{
	private AnimationPlayer animationPlayer;
	private bool attacking = false;
	private Camera camera;

	public override void _Ready()
	{
		player = GetParent().GetParent().GetNode<Player>("Player");
		animationPlayer = GetNode<AnimationPlayer>("Animation Player");
		animationPlayer.CurrentAnimation = "Hammer";
		animation = GetNode<AnimatedSprite2D>("Animation");
		camera = GetParent().GetParent().GetNode<Camera>("Camera");
		hurtSound = GetNode<AudioStreamPlayer2D>("Hurt Sound");
		health = 100;
		healAmount = 0;
	}


	/// <summary>
	/// Play the hammer animation
	/// </summary>
	public void Attack()
	{
		animationPlayer.Play();
		attacking = true;
	}

	/// <summary>
	/// Called when a body enters the damage area (bottom) of the hammer
	/// </summary>
	/// <param name="body"></param>
	public void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Hurt(20, 0.5f, 0.5f);
		}
	}

	/// <summary>
	/// Called by animation player when hammer hits the ground
	/// </summary>
	public void OnGroundHit()
	{
		camera.Shake(1, 1);
	}

	public override void Die()
	{
		isAlive = false;
		canBeHurt = false;
		animationPlayer.Stop();
		animationPlayer.CurrentAnimation = "Death";
		animationPlayer.Play();
		if (player.GetPower() <= 100)
		{
			player.IncreasePower(healAmount);
		}
	}
	
	/// <summary>
	/// Called by animation player when death anim has finished
	/// </summary>
	public void OnDeathFinished()
	{
		if(GetParent() is FinalBoss fb)
		{
			fb.Hammers.Remove(this);
		}
	}
}

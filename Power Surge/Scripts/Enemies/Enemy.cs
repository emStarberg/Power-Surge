using Godot;
using System;
//------------------------------------------------------------------------------
// <summary>
//   Interface to be inherited by all enemies
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public abstract partial class Enemy : CharacterBody2D
{
	protected bool isAlive = true;
	protected float health;
	protected AnimatedSprite2D animation;
	protected bool canBeHurt = true;
	protected Timer hurtCooldownTimer;

	/// <summary>
	/// Called when enemy is hit by an attack
	/// </summary>
	/// <param name="amount">Amount of health to subtract</param>
	public void Hurt(float amount)
	{
		if (!canBeHurt)
			return;

		health -= amount;
		if (health <= 0)
		{
			Die();
		}
		else
		{
			FlashHurtEffect();
			canBeHurt = false;
		}
	}

	/// <summary>
	/// Called when health = 0
	/// </summary>
	public virtual void Die()
	{
		isAlive = false;
		animation.Animation = "death";

	}

	/// <summary>
	/// Get remaining health of enemy
	/// </summary>
	/// <returns>Remaining health</returns>
	public float GetHealth()
	{
		return health;
	}

	/// <summary>
	/// Flash with red overlay when hurt
	/// </summary>
	protected async void FlashHurtEffect()
	{
		if (this.isAlive)
		{
			for (int i = 0; i < 3; i++)
			{
				animation.Modulate = new Color(1, 0, 0);
				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
				animation.Modulate = new Color(1, 1, 1);
				await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
			}
			canBeHurt = true;
		}

	}

}

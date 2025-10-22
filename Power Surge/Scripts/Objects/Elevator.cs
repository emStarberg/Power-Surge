using Godot;
using System;
using System.Runtime.CompilerServices;
//------------------------------------------------------------------------------
// <summary>
//   Can be stood on by the player, moves up and down when activated by a switch
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Elevator : SwitchOperatedObject
{
	[Export] float MaxHeight = 100; // Maximum height elevator can rise relative to starting position (Should start positioned at minimum height/floor)
	[Export] float Speed = 100;
	private float maxHeight, minHeight;
	private string direction = "up";
	private bool returning = false;
	private RayCast2D downRay;

	public override void _Ready()
	{
		minHeight = Position.Y;
		maxHeight = Position.Y - MaxHeight;
		downRay = GetNode<RayCast2D>("Down Ray");
	}

	public override void _Process(double delta)
	{
		if (IsOn)
		{
			downRay.ForceRaycastUpdate();
			if (downRay.IsColliding())
			{
				var collider = downRay.GetCollider();
				if(collider is Enemy || collider is Player)
				{
					direction = "up";
				}
			}
			if (direction == "up")
			{
				// Move up until maxHeight, then switch direction
				if (Position.Y <= maxHeight)
				{
					Position = new Vector2(Position.X, maxHeight);
					direction = "down";
				}
				else
				{
					Position += Vector2.Up * Speed * (float)delta;
				}
			}
			else // down
			{
				// Move down until minHeight, then switch direction
				if (Position.Y >= minHeight)
				{
					Position = new Vector2(Position.X, minHeight);
					direction = "up";                    
				}
				else
				{
					Position += Vector2.Down * Speed * (float)delta;
				}
			}
		}
		else if (returning)
		{
			// Move down until minHeight, then stay there
			if (Position.Y >= minHeight)
			{
				Position = new Vector2(Position.X, minHeight);
				returning = false;
			}
			else
			{
				Position += Vector2.Down * Speed * (float)delta;
			}
		}
		
	}
	
	protected override void OnStateSwitched()
	{
		if (!IsOn)
		{
			// Return to floor
			direction = "down";
			returning = true;
		}
	}

}

using Godot;
using System;
using System.Collections.Generic;
//------------------------------------------------------------------------------
// <summary>
//   Switches one or many SwitchOperatedObjects on/off
//   Can have a paired switch that is always in the opposite state (on/off)
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Switch : Area2D
{
	[Export] public SwitchOperatedObject[] SwitchObjects; // Array of objects that the switch operates
	[Export] public Switch PairedSwitch = null; // Switch that it is paired with (can be null if standalone)
	[Export] public bool IsOn = false;

	private Label label;
	private Texture2D onTexture, offTexture;
	private Sprite2D sprite;
	private bool canBeHit = true;

	public override void _Ready()
	{
		label = GetNode<Label>("Label");
		onTexture = GD.Load<Texture2D>("res://Assets/Objects/Switch - On.png");
		offTexture = GD.Load<Texture2D>("res://Assets/Objects/Switch - Off.png");
		sprite = GetNode<Sprite2D>("Sprite");
		if (IsOn)
		{
			sprite.Texture = onTexture;
		}
		else
		{
			sprite.Texture = offTexture;
		}
				foreach(SwitchOperatedObject o in SwitchObjects)
		{
			o.UpdateState(IsOn);
		}
	}

	/// <summary>
	/// Switch between states on/off
	/// </summary>
	public void ChangeState(bool fromSelf = true)
	{
		// Change own state
		IsOn = !IsOn;

		// Change object states
		foreach(SwitchOperatedObject o in SwitchObjects)
		{
			o.UpdateState(IsOn);
		}

		if (fromSelf)
		{
			// Change state of PairedSwitch if not null
			PairedSwitch?.ChangeState(false);
		}

		// Change texture
		if (IsOn)
		{
			sprite.Texture = onTexture;
		}
		else
		{
			sprite.Texture = offTexture;
		}
	}

	/// <summary>
	/// Called when a Node2D enters Area2D collider
	/// If area is a PlayerAttack, change states
	/// </summary>
	public void OnAreaEntered(Node2D area)
	{
		if (area is IPlayerAttack && canBeHit)
		{
			ChangeState();
			canBeHit = false;
		}
	}

	/// <summary>
	/// Called when a Node2D exits Area2D collider
	/// If area is a PlayerAttack, canBeHit = true;
	/// </summary>
	public void OnAreaExited(Node2D area)
	{
		if (area is IPlayerAttack)
			canBeHit = true;
	}

}

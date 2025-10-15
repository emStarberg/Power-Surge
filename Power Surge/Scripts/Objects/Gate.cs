using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Gate that can open and close, operated by a Switch
//   On = Open. Off = Closed
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public partial class Gate : SwitchOperatedObject
{
	private bool open;
	private AnimatedSprite2D animation;
	private StaticBody2D body;
	private bool ready = false;

	public override void _Ready()
	{
		open = IsOn;
		animation = GetNode<AnimatedSprite2D>("Animation");
		body = GetNode<StaticBody2D>("Body");

		body.GetNode<CollisionShape2D>("Collider").Disabled = open;

		// Show beginning state
		if (open)
		{
			animation.Frame = 5;

		}	
		else
		{
			animation.Frame = 0;
		}
		ready = true;
	}

	/// <summary>
	/// Called when the off/on state is switched
	/// </summary>
	protected override void OnStateSwitched()
	{
		if (ready)
		{
			open = IsOn;
			CallDeferred("SetCollider");
			// Open or close
			if (open)
				Open();
			if (!open)
				Close();
		}
	}

	private void Open()
	{
		animation.Frame = 0;
		animation.Play();
	}

	private void Close()
	{
		animation.Frame = 5;
		animation.PlayBackwards();
	}

	private void SetCollider()
	{
		body.GetNode<CollisionShape2D>("Collider").Disabled = open;
	}
}

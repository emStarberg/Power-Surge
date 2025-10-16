using Godot;
//------------------------------------------------------------------------------
// <summary>
//   Objects inheriting this can be turned off/on by a Switch
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public abstract partial class SwitchOperatedObject: Node2D, IWorldObject
{
	[Export] public bool IsOn = false;

	/// <summary>
	/// Switch to the given off/on state
	/// </summary>
	/// <param name="state"></param>
	public void UpdateState(bool state)
	{
		if(IsOn != state)
		{
			IsOn = state;
			OnStateSwitched();
		}
	}
	
	/// <summary>
	/// Called when the off/on state is switched
	/// To be implemented by sub classes
	/// </summary>
	protected virtual void OnStateSwitched()
	{
	}
}

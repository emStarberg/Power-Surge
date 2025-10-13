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
}

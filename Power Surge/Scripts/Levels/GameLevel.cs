using Godot;

//------------------------------------------------------------------------------
// <summary>
//   Abstract class to be inherited by all game levels
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public abstract partial class GameLevel : Node2D
{
	protected bool optionsOpen = false;
	protected Player player;
	protected float levelTimer = 0;
	protected float expectedTime;
	protected Camera camera;

	protected void checkOptionsMenu()
	{
		// esc pressed
		if (Input.IsActionJustPressed("input_menu") && !optionsOpen)
		{
			optionsOpen = true;
			var optionsScene = GD.Load<PackedScene>("res://Scenes/Screens/options_screen.tscn");
			var optionsInstance = optionsScene.Instantiate();
			GetTree().CurrentScene.GetNode<Control>("UI/Control").AddChild(optionsInstance);
			optionsInstance.TreeExited += OnOptionsClosed;
		}
	}
	/// <summary>
	/// Called when the options menu closes
	/// </summary>
	protected void OnOptionsClosed()
	{
		player = GetNode<Player>("Player");
		optionsOpen = false;
		player.UpdateVolume();
		UpdateVolume();
		foreach (Node node in GetNode<Node2D>("Enemies").GetChildren())
		{
			if (node is Enemy enemy)
			{
				enemy.UpdateVolume();
			}
		}
	}
	/// <summary>
	/// For updating volume levels after options menu has closed
	/// </summary>
	protected virtual void UpdateVolume()
	{

	}
	/// <summary>
	/// Get the time taken to complete the level
	/// </summary>
	/// <returns>levelTimer</returns>
	public float GetLevelTimer()
	{
		return levelTimer;
	}
	/// <summary>
	/// Get the level's expected completion time in seconds, for calculating rank
	/// </summary>
	/// <returns></returns>
	public float GetExpectedTime()
	{
		return expectedTime;
	}
}

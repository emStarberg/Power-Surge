using Godot;

//------------------------------------------------------------------------------
// <summary>
//   Abstract class to be inherited by all game levels
// </summary>
// <author>Emily Braithwaite</author>
//------------------------------------------------------------------------------
public abstract partial class GameLevel : Node2D
{
	[Export] public bool GlowEnabled = false;
	protected bool optionsOpen = false;
	protected Player player;
	protected float levelTimer = 0;
	protected float expectedTime;
	protected AudioStreamPlayer2D backgroundMusic;
	protected Camera camera;
	protected Control popup;

	protected void StartLevel()
	{
		player = GetNode<Player>("Player");
		popup = GetNode<Control>("UI/Pop-up");
		popup.Visible = false;
		camera = GetNode<Camera>("Camera");
		backgroundMusic = GetNode<AudioStreamPlayer2D>("Background Music");
		GameData.Instance.LevelEnemyCount = GetEnemiesRemaining();
		GameData.Instance.GlowEnabled = GlowEnabled;
	}
	
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

		if(Input.IsActionJustPressed("input_reload")){
			GetTree().ReloadCurrentScene();
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
		foreach (Node node in GetNode<Node2D>("Enemies").GetChildren())
		{
			if (node is IWorldObject obj)
			{
				obj.UpdateVolume();
			}
		}
	}
	/// <summary>
	/// For updating volume levels after options menu has closed
	/// </summary>
	protected virtual void UpdateVolume()
	{
		backgroundMusic.VolumeDb = GameSettings.Instance.GetFinalMusic();
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

	/// <summary>
	/// Get number of enemies remaining in the level
	/// </summary>
	/// <returns>Enemies remaining</returns>
	public int GetEnemiesRemaining()
	{
		int count = 0;
		foreach (Node node in GetNode<Node2D>("Enemies").GetChildren())
		{
			if (node is Enemy enemy)
			{
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// When a camera change point is passed by the player
	/// </summary>
	/// <param name="body"></param>
	/// <param name="checkpoint"></param>
	protected virtual void OnCameraChangeExited(Node2D body, CameraChange change)
	{
		if (body is Player player)
		{
			if (player.GetDirection() == change.DirectionEnteredFrom)
			{
				if (camera.Mode == "horizontal")
				{
					camera.Mode = "vertical";
				}
				else if (camera.Mode == "vertical")
				{
					camera.Mode = "horizontal";
				}
			}
		}
	}
}

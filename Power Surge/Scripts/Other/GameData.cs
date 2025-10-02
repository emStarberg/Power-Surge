using System;
using Godot;

public partial class GameData : Node
{

    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameData();
            return _instance;
        }
    }
    // Game data
    public int TotalFragments;
    public string CurrentLevel;

    // Current level data
    public int LevelFragments = 0;
    public float LevelTime = 0;
    public float LevelPower = 0;

}
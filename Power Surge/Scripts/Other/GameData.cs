using Godot;
using System;

public partial class GameData : Node
{
    private static GameData _instance;
    public static GameData Instance => _instance;

    // global settings / pending changes
    public bool GlowEnabled = true;
    public bool? PendingGlow = null;

    // game data
    public int TotalFragments;
    public string CurrentLevel;

    // current level data
    public int LevelFragments = 0;
    public float LevelTime = 0f;
    public float LevelPower = 0f;
    public float LevelExpectedTime = 0f;

    public override void _Ready()
    {
        if (_instance != null && _instance != this)
        {
            GD.PrintErr("GameData: extra instance detected, freeing duplicate.");
            QueueFree();
            return;
        }
        _instance = this;

        // If LevelLoader stored a pending glow preference before GameData existed, consume it now.
        if (LevelLoader.Instance != null && LevelLoader.Instance.PendingGlow.HasValue)
        {
            PendingGlow = LevelLoader.Instance.PendingGlow;
        }

        // Apply PendingGlow immediately so new scenes/readies see the correct value
        if (PendingGlow.HasValue)
        {
            GlowEnabled = PendingGlow.Value;
            PendingGlow = null;
        }
    }

    public override void _ExitTree()
    {
        if (_instance == this)
            _instance = null;
    }

    // convenience: apply pending settings (callable from other code if needed)
    public void ApplyPending()
    {
        if (PendingGlow.HasValue)
        {
            GlowEnabled = PendingGlow.Value;
            PendingGlow = null;
        }
    }
}
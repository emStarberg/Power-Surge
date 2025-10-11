using Godot;
using System;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }
    public bool? PendingGlow { get; private set; } = null;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void _Ready()
    {
        // If GameData already exists, transfer any pending value now
        ApplyPendingToGameData();
    }

    /// <summary>
    /// Load a new level from a scene path
    /// </summary>
    /// <param name="scenePath">Path to scene</param>
    /// <param name="glowEnabled">Choose if glowing is enabled (null = no change)</param>
    public void ChangeLevel(string scenePath, bool? glowEnabled = null)
    {
        // if caller provided a preference, apply it immediately to the global GameData
        if (glowEnabled.HasValue && GameData.Instance != null)
            GameData.Instance.GlowEnabled = glowEnabled.Value;

        GetTree().ChangeSceneToFile(scenePath);
    }

    public void ApplyPendingToGameData()
    {
        if (PendingGlow.HasValue && GameData.Instance != null)
        {
            GameData.Instance.PendingGlow = PendingGlow;
            PendingGlow = null;
        }
    }
}
using Godot;
using System;

public partial class LevelLoader : Node
{
    public static LevelLoader Instance { get; private set; }

    // store a desired glow value if GameData isn't available yet
    public bool? PendingGlow { get; private set; } = null;

    // Set the singleton as early as possible to avoid other nodes hitting null in their _Ready/_Process
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
    // ...existing code...
    public void ChangeLevel(string scenePath, bool? glowEnabled = null)
    {
        // if caller provided a preference, apply it immediately to the global GameData
        if (glowEnabled.HasValue && GameData.Instance != null)
            GameData.Instance.GlowEnabled = glowEnabled.Value;

        GetTree().ChangeSceneToFile(scenePath);
    }
// ...existing code...

    // Called by GameData._Ready() (or here in _Ready) to transfer any pending value into the GameData singleton.
    public void ApplyPendingToGameData()
    {
        if (PendingGlow.HasValue && GameData.Instance != null)
        {
            GameData.Instance.PendingGlow = PendingGlow;
            PendingGlow = null;
        }
    }
}
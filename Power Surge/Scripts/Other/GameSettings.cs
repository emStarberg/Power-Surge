using System;
using System.Collections.Generic;
using Godot;
/// <summary>
/// Stores game settings such as volume and key bindings
/// Make changes to settings and stats here, then SaveGame() to save to the config file
/// </summary>
public partial class GameSettings : Node
{
    // Broardcast to all audio players
    [Signal] public delegate void VolumeChangedEventHandler();

    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameSettings();
            return _instance;
        }
    }

    public int[] LevelFragments = { 0, 0, 0, 0, 0, 0, 0, 0 }; // In order of levels, how many fragments have been collected
    public bool HasStarted; // Whether the game has been started on this save file
    public bool TutorialComplete; // Whether the tutorial has been completed
    public string[] UnlockedLevels = new string[9];
    private float volume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private float finalMusicVolume = 1f;
    private float finalSfxVolume = 1f;

    public string currentLevel;
    public string nextLevel;

    public float Volume
    {
        get => volume;
        set
        {
            volume = value;
            UpdateAllAudioVolumes();
        }
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            UpdateAllAudioVolumes();
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = value;
            UpdateAllAudioVolumes();
        }
    }

    // For updating audio players
    public float GetFinalSfx()
    {
        return finalSfxVolume;
    }

    public float GetFinalMusic()
    {
        return finalMusicVolume;
    }

    public int GetTotalFragments()
    {
        // Add up fragments across levels
        int totalFragments = 0;
        foreach (int i in LevelFragments)
        {
            totalFragments += i;
        }
        return totalFragments;
    }
    




    public string PlayerName { get; set; } = "Felix";

    private void UpdateAllAudioVolumes()
    {
        EmitSignal(nameof(VolumeChanged));
        finalMusicVolume = Mathf.LinearToDb(volume * musicVolume);
        finalSfxVolume = Mathf.LinearToDb(volume * sfxVolume);
    }

    public void SaveGame()
    {
        var config = ConfigManager.Instance;

        config.SaveData("PlayerName", PlayerName);

        config.SaveData("SfxVolume", finalSfxVolume);
        config.SaveData("MusicVolume", finalMusicVolume);
        config.SaveData("MasterVolume", volume);

        config.SaveData("LevelFragments", LevelFragments);
        config.SaveData("HasStarted", HasStarted);
        config.SaveData("UnlockedLevels", UnlockedLevels);

        // Save key bindings
        var bindings = new Godot.Collections.Dictionary<string, Godot.Collections.Array>();
        foreach (var action in InputMap.GetActions())
        {
            var events = new Godot.Collections.Array();
            foreach (var ev in InputMap.ActionGetEvents(action))
                events.Add(ev.AsText()); // store key names as strings

            bindings[action] = events;
        }
        config.SaveData("InputBindings", bindings);
    }
    

    public void LoadGame()
    {
        var config = ConfigManager.Instance;

        PlayerName = (string)config.LoadData("PlayerName", "Felix");

        finalSfxVolume = (float)config.LoadData("SfxVolume", 0);
        finalMusicVolume = (float)config.LoadData("MusicVolume", 0);
        volume = (float)config.LoadData("MasterVolume", 0);

        LevelFragments = (int[])config.LoadData("LevelFragments", new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0});
        HasStarted = (bool)config.LoadData("HasStarted", false);
        TutorialComplete = (bool)config.LoadData("TutorialComplete", false);
        UnlockedLevels = (string[])config.LoadData("UnlockedLevels", new string[8]);

        var defaultBindings = new Godot.Collections.Dictionary<string, Godot.Collections.Array>();
        object savedBindingsObj = ConfigManager.Instance.LoadData("InputBindings", defaultBindings);
        Godot.Collections.Dictionary<string, Godot.Collections.Array> savedBindings = null;

        // Try direct cast first (works if LoadData returned the expected dictionary)
        if (savedBindingsObj is Godot.Collections.Dictionary<string, Godot.Collections.Array> sb)
        {
            savedBindings = sb;
        }
        else
        {
            // If the data was boxed differently (Variant or a non-generic IDictionary), attempt to convert it
            if (savedBindingsObj is System.Collections.IDictionary idict)
            {
                savedBindings = new Godot.Collections.Dictionary<string, Godot.Collections.Array>();
                foreach (var key in idict.Keys)
                {
                    if (key is string k && idict[key] is Godot.Collections.Array v)
                        savedBindings[k] = v;
                }
            }
        }
            if (savedBindings != null)
            {
                foreach (string action in savedBindings.Keys)
                {
                    InputMap.ActionEraseEvents(action); // clear existing
                    foreach (object evObj in savedBindings[action])
                    {
                        // If the saved binding is already an InputEvent object, add it directly.
                        if (evObj is InputEvent inputEvent)
                        {
                            InputMap.ActionAddEvent(action, inputEvent);
                            continue;
                        }
                    }
                }
            }
        }

}
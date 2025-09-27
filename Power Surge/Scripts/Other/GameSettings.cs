using System;
using Godot;

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

    private float volume = 0.5f;
    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;

    private float finalMusicVolume = 0.5f;
    private float finalSfxVolume = 0.5f;

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

    public string PlayerName { get; set; } = "George";

    private void UpdateAllAudioVolumes()
    {
        EmitSignal(nameof(VolumeChanged));
        finalMusicVolume = Mathf.LinearToDb(volume * musicVolume);
        finalSfxVolume = Mathf.LinearToDb(volume * sfxVolume);
    }


}
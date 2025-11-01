// File: Scripts/GameConfigManager.cs
using Godot;
using System;
using System.Collections.Generic;

public partial class ConfigManager : Node
{
    private const string ConfigPath = "user://config.cfg";
    private ConfigFile _config = new ConfigFile();
    private Dictionary<string, Variant> _cache = new();

    public static ConfigManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        LoadFromDisk();
    }

    public void SaveData(string key, Variant value)
    {
        _cache[key] = value;
        //GD.Print($"Saved {key} = {value}");
        SaveToDisk();
    }

    public Variant LoadData(string key, Variant defaultValue = default)
    {
        if (_cache.ContainsKey(key))
            return _cache[key];

        var err = _config.Load(ConfigPath);
        if (err == Error.Ok && _config.HasSectionKey("data", key))
        {
            var value = _config.GetValue("data", key, defaultValue);
            _cache[key] = value;
            return value;
        }

        return defaultValue;
    }

    private void SaveToDisk()
    {
        foreach (var kv in _cache)
            _config.SetValue("data", kv.Key, kv.Value);

        var err = _config.Save(ConfigPath);
        if (err != Error.Ok)
            GD.PrintErr($"Failed to save config file: {err}");
    }

    private void LoadFromDisk()
    {
        var err = _config.Load(ConfigPath);
        if (err != Error.Ok)
        {
            GD.Print("No config file found, creating new one.");
            return;
        }

        foreach (string key in _config.GetSectionKeys("data"))
            _cache[key] = _config.GetValue("data", key);

        GD.Print("Config loaded successfully.");
    }

    
}

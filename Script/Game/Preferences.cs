using Godot;
using System;

public partial class Preferences : Node
{
    private readonly string savePath = "user://preferences.tres";

    public PreferencesResource Data;

    public event Action PreferencesUpdated;

    public override void _Ready()
    {
        if (ResourceLoader.Exists(savePath))
        {
            Data = (PreferencesResource)ResourceLoader.Load(savePath);
        }
        else
        {
            Data = new();
            GD.Print("Preferences not found, using defaults");
        }

        UpdateAudioPreferences();
        PreferencesUpdated?.Invoke();
        GD.Print("Preferences loaded");
    }

    public void SavePreferences()
    {
        Error result = ResourceSaver.Save(Data, savePath);

        if (result != Error.Ok)
        {
            GD.PrintErr("Failed to save preferences to location: " + savePath);
        }
        else
        {
            GD.Print("Saved player preferences to: " + savePath);
        }
    }

    private void UpdateAudioPreferences()
    {
        if (Data.EnableAudio && AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")))
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), false);
        }
        else if (!Data.EnableAudio && !AudioServer.IsBusMute(AudioServer.GetBusIndex("Master")))
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
        }

        if (Data.EnableMusic && AudioServer.IsBusMute(AudioServer.GetBusIndex("Music")))
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), false);
        }
        else if (!Data.EnableMusic && !AudioServer.IsBusMute(AudioServer.GetBusIndex("Music")))
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), true);
        }

        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Data.MasterVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Ambient"), Data.AmbientVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Environment"), Data.EnvironmentVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Data.MusicVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Data.SFXVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("UI"), Data.UIVolume);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Voice"), Data.VoiceVolume);

        GD.Print("Audio settings restored from preferences");
    }

}


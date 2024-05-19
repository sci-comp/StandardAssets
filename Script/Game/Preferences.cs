using Godot;
using System;

public partial class Preferences : Node
{
    private readonly string savePath = "user://preferences.tres";

    public PreferencesResource Data { get; set; } = new();

    public event Action PreferencesUpdated;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (ResourceLoader.Exists(savePath))
        {
            Data = (PreferencesResource)ResourceLoader.Load(savePath);
            UpdateAudioPreferences();
            PreferencesUpdated?.Invoke();
            GD.Print("Preferences loaded");
        }
        else
        {
            GD.Print("Preferences not found, using defaults");
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("exit_program"))
        {
            GetTree().Quit();
        }
    }

    public void SavePreferences()
    {
        ResourceSaver.Save(Data, savePath);
        UpdateAudioPreferences();
        PreferencesUpdated.Invoke();
        GD.Print("Saved player preferences to: " + savePath);
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
    }

}


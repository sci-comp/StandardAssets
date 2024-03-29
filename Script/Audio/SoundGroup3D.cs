using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SoundGroup3D : Node
{
    [Export] public int MaxVoices = 3;
    [Export] public Vector2 VaryPitch = new(0.97f, 1.03f);
    [Export] public Vector2 VaryVolume = new(0.95f, 1.0f);
    
    private RandomNumberGenerator rnd = new();
    private SFXPlayer3D sfxPlayer3D;

    public List<AudioStreamPlayer3D> AvailableSources = new();
    public List<AudioStreamPlayer3D> ActiveSources = new();

    public int TotalVariations => ActiveSources.Count + AvailableSources.Count;

    public override void _Ready()
    {
        sfxPlayer3D = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");

        foreach (AudioStreamPlayer3D audioStreamPlayer3D in GetChildren().Cast<AudioStreamPlayer3D>())
        {
            audioStreamPlayer3D.Finished += () => OnAudioFinished(audioStreamPlayer3D);
            AvailableSources.Add(audioStreamPlayer3D);
        }

        if (MaxVoices > AvailableSources.Count)
        {
            GD.Print("More voices are allowed than sources exist for sound group: " 
                + Name 
                + ". Setting max voices to: " + AvailableSources.Count);

            MaxVoices = AvailableSources.Count;
        }
    }

    public void OnAudioFinished(AudioStreamPlayer3D src)
    {
        GD.Print("On audio finished playing.");
        ActiveSources.Remove(src);
        AvailableSources.Append(src);
        sfxPlayer3D.UpdateSoundGroupDisplay(this);
    }

    public void Stop(AudioStreamPlayer3D src)
    {
        src.Stop();
        ActiveSources.Remove(src);
        AvailableSources.Append(src);
        sfxPlayer3D.UpdateSoundGroupDisplay(this);
    }

    public (AudioStreamPlayer3D, SoundGroup3D) GetAvailableSource()
    {
        AudioStreamPlayer3D src;

        // Stop an active source if necessary
        if ((AvailableSources.Count > 0 && ActiveSources.Count >= MaxVoices)
            || AvailableSources.Count == 0)
        {
            src = ActiveSources[0];
            Stop(src);
        }

        src = AvailableSources[rnd.RandiRange(0, AvailableSources.Count)];
        src.PitchScale = (float)GD.RandRange(VaryPitch.X, VaryPitch.Y);
        src.VolumeDb = (float)GD.RandRange(VaryVolume.X, VaryVolume.Y);

        ActiveSources.Add(src);

        return (src, this);
    }

}


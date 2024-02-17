using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SoundGroup : Node
{
    [Export] public int MaxVoices = 3;
    [Export] public SoundBUS SoundBus = SoundBUS.SFX;
    [Export] public Vector2 VaryPitch = new(0.97f, 1.03f);
    [Export] public Vector2 VaryVolume = new(0.95f, 1.0f);

    public List<AudioStreamPlayer3D> AvailableSources = new();
    public List<AudioStreamPlayer3D> ActiveSources = new();

    public int NumVariations = 0;
    
    private RandomNumberGenerator rnd = new();

    public override void _Ready()
    {
        foreach (AudioStreamPlayer3D audioStreamPlayer3D in GetChildren().Cast<AudioStreamPlayer3D>())
        {
            audioStreamPlayer3D.Finished += () => OnAudioFinished(audioStreamPlayer3D);
            AvailableSources.Add(audioStreamPlayer3D);
        }

        NumVariations = AvailableSources.Count();  
        GD.Print("Available sources: " + NumVariations);
    }

    public void OnAudioFinished(AudioStreamPlayer3D src)
    {
        GD.Print("On audio finished playing.");
        ActiveSources.Remove(src);
        AvailableSources.Append(src);
        SoundManager.Inst.HandleAudioSourceStopped(this, src);
    }

    public void Stop(AudioStreamPlayer3D src)
    {
        src.Stop();
        ActiveSources.Remove(src);
        AvailableSources.Append(src);
        SoundManager.Inst.HandleAudioSourceStopped(this, src);
    }

    public (AudioStreamPlayer3D, SoundGroup) GetAvailableSource()
    {
        AudioStreamPlayer3D src;
        
        if (AvailableSources.Count > 0 && ActiveSources.Count < MaxVoices)
        {
            GD.Print("AvailableSources.Count: " + AvailableSources.Count);
            src = AvailableSources[rnd.RandiRange(0, AvailableSources.Count)];
            ActiveSources.Add(src);
        }
        //else if (AvailableSources.Count > 0 && ActiveSources.Count >= MaxVoices)
        //{
            // Stop an active source


            // return an available source
        //}
        else if (ActiveSources.Count > 0)
        {
            GD.Print("ActiveSources.Count: " + ActiveSources.Count);
            src = ActiveSources[0];
            src.Stop();
            SoundManager.Inst.HandleAudioSourceStopped(this, src);
            ActiveSources.RemoveAt(0);
            ActiveSources.Add(src);
        }
        else
        {
            GD.PrintErr("No active or available audio sources. This is not logical.");
            return (null, this);
        }

        src.PitchScale = (float)GD.RandRange(VaryPitch.X, VaryPitch.Y);
        src.VolumeDb = (float)GD.RandRange(VaryVolume.X, VaryVolume.Y);

        return (src, this);
    }

}


using Godot;
using System.Collections.Generic;

public partial class SoundGroup : Node
{
    [Export]
    public bool Use3DSounds { get; private set; } = false;
    [Export] public bool Use3DSound = false;
    [Export] public Vector2 VaryPitch = new Vector2(0.95f, 1.05f);
    [Export] public Vector2 VaryVolume = new Vector2(0.94f, 1.0f);
    [Export] public SoundBUS SoundBUS = SoundBUS.SFX;

    private Dictionary<AudioStreamPlayer, bool> SourceCoroutines = new();
    private Queue<AudioStreamPlayer> AvailableSources = new();
    private List<AudioStreamPlayer> ActiveSources = new();

    public override void _Ready()
    {
        foreach (AudioStreamPlayer child in GetChildren())
        {
            AvailableSources.Enqueue(child);
        }
    }

    public (AudioStreamPlayer, SoundGroup) GetAvailableSource()
    {
        AudioStreamPlayer src;

        if (AvailableSources.Count > 0)
        {
            src = AvailableSources.Dequeue();
            src.Playing = true;
            ActiveSources.Add(src);
        }
        else if (ActiveSources.Count > 0)
        {
            src = ActiveSources[0];
            src.Playing = true;
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
        WaitForAudioToEnd(src);

        return (src, this);
    }

    private async void WaitForAudioToEnd(AudioStreamPlayer src)
    {
        await ToSignal(src, "finished");
        src.Playing = false;
        ActiveSources.Remove(src);
        AvailableSources.Enqueue(src);
    }
}

